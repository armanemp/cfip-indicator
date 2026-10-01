// ============================================================================
// CFIP Indicator — BrokerProtectionExecution.cs
// ============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool ProtectOrphanManagedPosition(
                                    Position position,
                                    int closedM5)
                                {
                                    int direction =
                                        position.TradeType == TradeType.Buy
                                            ? 1
                                            : -1;

                                    double atr =
                                        _m5Bars == null
                                            ? 0
                                            : Atr(
                                                _m5Bars,
                                                Math.Max(
                                                    1,
                                                    closedM5));

                                    if (!IsFinitePositive(atr))
                                        atr =
                                            Math.Max(
                                                Symbol.PipSize * 20,
                                                Math.Abs(
                                                    Symbol.Ask -
                                                    Symbol.Bid) * 10);

                                    string stopSource;
                                    int stopQuality;

                                    double stop =
                                        position.StopLoss.HasValue &&
                                        IsValidManagedStop(
                                            direction,
                                            position.EntryPrice,
                                            direction == 1
                                                ? Symbol.Bid
                                                : Symbol.Ask,
                                            position.StopLoss.Value)
                                            ? position.StopLoss.Value
                                            : BuildStructuralStop(
                                                Math.Max(1, closedM5),
                                                direction,
                                                position.EntryPrice,
                                                atr,
                                                out stopSource,
                                                out stopQuality);

                                    if (!IsValidStop(
                                            direction,
                                            position.EntryPrice,
                                            stop))
                                    {
                                        StructuralStopGeometrySnapshot fallbackGeometry =
                                            StructuralStopGeometryRule.EvaluateFallback(
                                                direction,
                                                position.EntryPrice,
                                                atr,
                                                Math.Max(0.10, FallbackSlAtr),
                                                Symbol.TickSize,
                                                Symbol.Digits);

                                        stop =
                                            fallbackGeometry.IsValid
                                                ? fallbackGeometry.Stop
                                                : 0;

                                        if (IsFinitePositive(stop))
                                        {
                                            double riskAtr =
                                                Math.Abs(
                                                    position.EntryPrice - stop) /
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
                                                    MaximumSpreadToStopRiskRatio) ||
                                                !IsValidStop(
                                                    direction,
                                                    position.EntryPrice,
                                                    stop))
                                                stop = 0;
                                        }
                                    }

                                    if (!IsValidStop(
                                            direction,
                                            position.EntryPrice,
                                            stop))
                                    {
                                        SendUnifiedAlert(
                                            "ORPHAN-PROTECTION-FAILED|" +
                                            position.Id,
                                            "CFIP ORPHAN POSITION PROTECTION FAILED • INVALID STOP • RETRY | #" +
                                            position.Id,
                                            direction,
                                            true);

                                        return OrphanManagedProtectionRule.CanReportSuccess(
                                            direction,
                                            false,
                                            false);
                                    }

                                    double target =
                                        position.TakeProfit.HasValue &&
                                        IsValidTarget(
                                            direction,
                                            position.EntryPrice,
                                            position.TakeProfit.Value)
                                            ? position.TakeProfit.Value
                                            : SelectStructuralAutoTarget(
                                                Math.Max(1, closedM5),
                                                direction,
                                                position.EntryPrice,
                                                stop,
                                                atr,
                                                EffectiveAutoTpStage());

                                    if (!IsValidTarget(
                                            direction,
                                            position.EntryPrice,
                                            target))
                                    {
                                        double risk =
                                            Math.Max(
                                                Symbol.PipSize,
                                                Math.Abs(
                                                    position.EntryPrice -
                                                    stop));

                                        target =
                                            direction == 1
                                                ? position.EntryPrice +
                                                  risk *
                                                  Math.Max(
                                                      1,
                                                      MinimumRequiredRR())
                                                : position.EntryPrice -
                                                  risk *
                                                  Math.Max(
                                                      1,
                                                      MinimumRequiredRR());
                                    }

                                    bool brokerProtectionConfirmed =
                                        EnsureBrokerProtectionForPosition(
                                            position,
                                            stop,
                                            target,
                                            "ORPHAN MANAGED POSITION",
                                            direction,
                                            false);

                                    return OrphanManagedProtectionRule.CanReportSuccess(
                                        direction,
                                        IsValidStop(
                                            direction,
                                            position.EntryPrice,
                                            stop),
                                        brokerProtectionConfirmed);
                                }
    }
}
