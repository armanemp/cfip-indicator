using System;
using System.Collections.Generic;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool TryBuildCanonicalTradePathGeometry(
            int closedM5,
            int direction,
            ExecutionModel execution,
            OpportunityLane lane,
            out CanonicalTradePathGeometry path,
            out string reason)
        {
            path = null;
            reason = string.Empty;

            if (_m5Bars == null ||
                closedM5 < 30 ||
                execution == null ||
                execution.Direction != direction ||
                (direction != 1 && direction != -1))
            {
                reason = "CANONICAL TRADE PATH UNAVAILABLE";
                return false;
            }

            if (!execution.Ready)
            {
                reason = "EXECUTION MODEL NOT READY";
                return false;
            }

            if (execution.Mode != ExecutionMode.RetestMarket &&
                execution.Mode != ExecutionMode.BreakoutMarket)
            {
                reason = "NON-MARKET EXECUTION MODE";
                return false;
            }

            double atr =
                Atr(
                    _m5Bars,
                    closedM5);

            if (!IsFinitePositive(atr))
            {
                reason = "ATR UNAVAILABLE";
                return false;
            }

            double entry =
                NormalizePrice(
                    execution.ActualEntry);

            if (!IsFinitePositive(entry))
            {
                reason = "ACTUAL ENTRY UNAVAILABLE";
                return false;
            }

            double stop =
                BuildStructuralStop(
                    closedM5,
                    direction,
                    entry,
                    atr,
                    out string stopSource,
                    out int stopQuality);

            if (!IsFinitePositive(stop))
            {
                if (RequireStructuralStop)
                {
                    reason = "STRUCTURAL STOP UNAVAILABLE";
                    return false;
                }

                StructuralStopGeometrySnapshot fallback =
                    StructuralStopGeometryRule.EvaluateFallback(
                        direction,
                        entry,
                        atr,
                        FallbackSlAtr,
                        Symbol.TickSize,
                        Symbol.Digits);

                if (!fallback.IsValid)
                {
                    reason = "STOP FALLBACK INVALID";
                    return false;
                }

                stop = fallback.Stop;
                stopSource = "ATR FALLBACK";
                stopQuality = 50;
            }

            stop = NormalizePrice(stop);

            if (!IsValidStop(
                    direction,
                    entry,
                    stop))
            {
                reason = "STOP WRONG SIDE";
                return false;
            }

            double risk =
                Math.Abs(
                    entry - stop);

            if (!IsFinitePositive(risk))
            {
                reason = "RISK INVALID";
                return false;
            }

            double riskAtr =
                risk /
                Math.Max(
                    Symbol.PipSize,
                    atr);

            double spread =
                Math.Max(
                    0,
                    Symbol.Ask - Symbol.Bid);

            if (!StructuralStopRiskRule.IsWithinPlanningRiskEnvelope(
                    riskAtr,
                    atr,
                    MinimumSlAtr,
                    MaximumSlAtr,
                    MaximumStructuralStopAtr,
                    spread,
                    Symbol.PipSize,
                    MaximumSpreadToStopRiskRatio))
            {
                reason = "STOP RISK ENVELOPE";
                return false;
            }

            List<Level> candidates =
                BuildTargetLevels(
                    closedM5,
                    direction,
                    entry,
                    atr);

            if (candidates == null ||
                candidates.Count == 0)
            {
                reason = "NO TARGET CANDIDATES";
                return false;
            }

            List<Level> selected =
                SelectTargets(
                    candidates,
                    closedM5,
                    entry,
                    risk,
                    direction,
                    atr,
                    lane);

            double tp1;
            double tp2;
            double tp3;
            double tp4;

            if (!TryBuildPlanTargets(
                    candidates,
                    selected,
                    closedM5,
                    entry,
                    risk,
                    direction,
                    atr,
                    lane,
                    out tp1,
                    out tp2,
                    out tp3,
                    out tp4))
            {
                reason = "TARGET PATH INVALID";
                return false;
            }

            RiskRewardMathResult rrGeometry =
                RiskRewardMathRule.Evaluate(
                    direction,
                    entry,
                    stop,
                    tp1,
                    spread,
                    0,
                    Math.Max(0, MaximumRewardRR),
                    0);

            double tp1RR =
                rrGeometry.Valid
                    ? rrGeometry.NominalRR
                    : 0;

            if (!IsFinitePositive(tp1RR))
            {
                reason =
                    string.IsNullOrWhiteSpace(rrGeometry.Reason)
                        ? "TP1 RR INVALID"
                        : rrGeometry.Reason;
                return false;
            }

            TradeSetupPreview preview =
                new TradeSetupPreview
                {
                    Direction = direction,
                    EntryMode = execution.Mode,
                    CreatedM5 = closedM5,
                    Entry = entry,
                    IdealEntry = NormalizePrice(execution.IdealEntry),
                    ZoneLow = NormalizePrice(execution.ZoneLow),
                    ZoneHigh = NormalizePrice(execution.ZoneHigh),
                    ZoneTolerance =
                        Math.Max(
                            0,
                            execution.ZoneTolerance),
                    Trigger = NormalizePrice(execution.Trigger),
                    Invalidation = NormalizePrice(execution.Invalidation),
                    Stop = stop,
                    Tp1 = tp1,
                    Tp2 = tp2,
                    Tp3 = tp3,
                    Tp4 = tp4,
                    Risk = risk
                };

            path =
                new CanonicalTradePathGeometry(
                    direction,
                    execution.Mode,
                    entry,
                    NormalizePrice(execution.IdealEntry),
                    stop,
                    tp1,
                    tp2,
                    tp3,
                    tp4,
                    risk,
                    tp1RR,
                    stopSource,
                    stopQuality,
                    preview);

            return path.IsValid;
        }
    }
}
