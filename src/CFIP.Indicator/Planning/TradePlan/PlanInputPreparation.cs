using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool TryPreparePlanInputs(
            int closedM5,
            int direction,
            out double atr,
            out ExecutionModel execution,
            out double entry,
            out double stop,
            out string stopSource,
            out int stopQuality,
            out double risk)
        {
            atr = 0;
            execution = null;
            entry = 0;
            stop = 0;
            stopSource = "";
            stopQuality = 0;
            risk = 0;

            if (_m5Bars == null ||
                closedM5 < 30 ||
                (direction != 1 && direction != -1))
                return false;

            atr =
                Atr(
                    _m5Bars,
                    closedM5);

            if (!IsFinitePositive(atr))
                return false;

            execution =
                BuildExecutionModel(
                    closedM5,
                    direction);

            if (execution == null ||
                !execution.Ready ||
                !IsFinitePositive(
                    execution.ActualEntry))
                return false;

            if (execution.Mode !=
                    ExecutionMode.BreakoutMarket &&
                execution.Mode !=
                    ExecutionMode.RetestMarket)
                return false;

            entry =
                NormalizePrice(
                    execution.ActualEntry);

            if (!IsFinitePositive(entry))
                return false;

            if (RequirePrecisionEntry &&
                execution.Quality <
                ActionabilityThresholdPolicy.EffectivePrecisionEntryQualityFloor(
                    MinimumEntryQuality))
                return false;

            Plan executionProbe =
                new Plan
                {
                    Direction = direction,
                    EntryMode = execution.Mode,
                    EntryTrigger = execution.Trigger,
                    EntryZoneLow = execution.ZoneLow,
                    EntryZoneHigh = execution.ZoneHigh,
                    EntryZoneTolerance = execution.ZoneTolerance
                };

            string executionReason;

            if (!IsExecutableMarketEntry(
                    executionProbe,
                    entry,
                    out executionReason))
                return false;

            stop =
                BuildStructuralStop(
                    closedM5,
                    direction,
                    entry,
                    atr,
                    out stopSource,
                    out stopQuality);

            if (!IsFinitePositive(stop))
            {
                if (RequireStructuralStop)
                    return false;

                StructuralStopGeometrySnapshot fallbackGeometry =
                    StructuralStopGeometryRule.EvaluateFallback(
                        direction,
                        entry,
                        atr,
                        FallbackSlAtr,
                        Symbol.TickSize,
                        Symbol.Digits);

                if (!fallbackGeometry.IsValid)
                    return false;

                stop =
                    fallbackGeometry.Stop;

                stopSource =
                    "ATR FALLBACK";
                stopQuality = 50;
            }

            EntryGeometrySnapshot planGeometry =
                EntryGeometryRule.Evaluate(
                    direction,
                    execution.Mode,
                    entry,
                    execution.ZoneLow,
                    execution.ZoneHigh,
                    execution.ZoneTolerance,
                    execution.IdealEntry,
                    execution.Trigger,
                    entry,
                    atr,
                    Symbol.TickSize,
                    Symbol.PipSize,
                    AllowPrecisionBreakoutEntry,
                    false,
                    MaximumEntryExtensionAtr,
                    MaximumEntryDistanceAtr);

            if (!planGeometry.IsValid ||
                planGeometry.Mode != execution.Mode ||
                !IsFinitePositive(planGeometry.ActualEntry))
                return false;

            if (AvoidLateEntry &&
                planGeometry.IsLate)
                return false;

            risk =
                Math.Abs(
                    entry -
                    stop);

            if (!IsFinitePositive(risk))
                return false;

            double spread =
                Math.Max(
                    0,
                    Symbol.Ask -
                    Symbol.Bid);

            double riskAtr =
                risk /
                Math.Max(
                    Symbol.PipSize,
                    atr);

            return
                StructuralStopRiskRule.IsWithinPlanningRiskEnvelope(
                    riskAtr,
                    atr,
                    MinimumSlAtr,
                    MaximumSlAtr,
                    MaximumStructuralStopAtr,
                    spread,
                    Symbol.PipSize,
                    MaximumSpreadToStopRiskRatio);
        }
    }
}
