// CFIP Indicator — PendingFillPlanBuilder.cs
// Single-responsibility conversion of a broker-confirmed pending fill into a managed plan.

using System;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private Plan BuildPendingFillPlan(
            PendingOrderFilledEventArgs args,
            int closedM5,
            int direction,
            double atr,
            out double stop,
            out double target,
            out bool protectionMissing)
        {
            stop =
                args.Position.StopLoss.HasValue &&
                IsValidStop(
                    direction,
                    args.Position.EntryPrice,
                    args.Position.StopLoss.Value)
                    ? NormalizePrice(
                        args.Position.StopLoss.Value)
                    : 0;

            target =
                args.Position.TakeProfit.HasValue &&
                IsValidTarget(
                    direction,
                    args.Position.EntryPrice,
                    args.Position.TakeProfit.Value)
                    ? NormalizePrice(
                        args.Position.TakeProfit.Value)
                    : 0;

            protectionMissing =
                !IsFinitePositive(stop) ||
                !IsFinitePositive(target);

            if (!IsFinitePositive(stop))
            {
                string stopSource;
                int stopQuality;

                stop =
                    BuildStructuralStop(
                        closedM5,
                        direction,
                        args.Position.EntryPrice,
                        atr,
                        out stopSource,
                        out stopQuality);

                if (!IsValidStop(
                        direction,
                        args.Position.EntryPrice,
                        stop))
                {
                    StructuralStopGeometrySnapshot fallbackGeometry =
                        StructuralStopGeometryRule.EvaluateFallback(
                            direction,
                            args.Position.EntryPrice,
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
                                args.Position.EntryPrice - stop) /
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
                                args.Position.EntryPrice,
                                stop))
                            stop = 0;
                    }
                }
            }

            if (!IsFinitePositive(target))
            {
                target =
                    SelectStructuralAutoTarget(
                        closedM5,
                        direction,
                        args.Position.EntryPrice,
                        stop,
                        atr,
                        EffectiveAutoTpStage());

                if (!IsValidTarget(
                        direction,
                        args.Position.EntryPrice,
                        target))
                {
                    double risk =
                        Math.Max(
                            Symbol.PipSize,
                            Math.Abs(
                                args.Position.EntryPrice -
                                stop));

                    double fallbackDistance =
                        risk *
                        Math.Max(
                            1.0,
                            MinimumRequiredRR());

                    target =
                        direction == 1
                            ? args.Position.EntryPrice + fallbackDistance
                            : args.Position.EntryPrice - fallbackDistance;

                    target =
                        NormalizePrice(target);
                }
            }

            return
                CreateManagedPlanFromExecution(
                    direction,
                    args.Position.EntryPrice,
                    stop,
                    target,
                    closedM5,
                    args.Position.VolumeInUnits,
                    args.PendingOrder.OrderType ==
                        PendingOrderType.Stop
                        ? ExecutionMode.ContinuationStop
                        : ExecutionMode.ReversalLimit);
        }
    }
}
