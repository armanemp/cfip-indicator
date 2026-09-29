using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool TryProcessAcceptedAggressiveFill(
            int closedM5,
            double entry,
            double atr,
            double stop,
            double target,
            ExecutionIntent aggressiveIntent,
            TradeResult result,
            out double actualStop,
            out double actualTarget)
        {
            actualStop = 0;
            actualTarget = 0;

            double actualFill =
                NormalizePrice(
                    result.Position.EntryPrice);

            string aggressiveFillReason;

            if (!ValidateActualMarketFill(
                    aggressiveIntent,
                    actualFill,
                    atr,
                    out aggressiveFillReason))
            {
                SetLifecycleState(
                    LifecycleState.ExitRequested,
                    "AGGRESSIVE FILL MISMATCH");

                bool closed =
                    TryClosePosition(
                        result.Position,
                        "AGGRESSIVE FILL MISMATCH");

                if (!closed)
                {
                    SetLifecycleState(
                        LifecycleState.RecoveryRequired,
                        "AGGRESSIVE FILL MISMATCH • CLOSE REJECTED");
                }
                else
                {
                    _autoExecutionBlockReason =
                        "FILL MISMATCH • POSITION CLOSE REQUESTED";
                }

                SendUnifiedAlert(
                    "FILL-MISMATCH|" +
                    result.Position.Id,
                    "CFIP AGGRESSIVE FILL OUTSIDE EXECUTION ENVELOPE | #" +
                    result.Position.Id +
                    " | " +
                    aggressiveFillReason,
                    _reaction.Direction,
                    true);

                return false;
            }

            string actualStopSource;
            int actualStopQuality;

            actualStop =
                BuildStructuralStop(
                    closedM5,
                    _reaction.Direction,
                    actualFill,
                    atr,
                    out actualStopSource,
                    out actualStopQuality);

            actualTarget =
                SelectStructuralAutoTarget(
                    closedM5,
                    _reaction.Direction,
                    actualFill,
                    actualStop,
                    atr,
                    AggressiveTpStage);

            if (!IsExecutionPlanConsistent(
                    _reaction.Direction,
                    actualFill,
                    actualStop,
                    actualTarget))
            {
                SetLifecycleState(
                    LifecycleState.RecoveryRequired,
                    "AGGRESSIVE POST-FILL REBUILD FAILED");
                return false;
            }

            _lastAutoM5 =
                closedM5;

            _aggressiveEntryPolicy.Invalidate();

            _autoExecutionBlockReason =
                "EXECUTED";

            _plan =
                CreateManagedPlanFromExecution(
                    _reaction.Direction,
                    result.Position.EntryPrice,
                    stop,
                    target,
                    closedM5,
                    result.Position.VolumeInUnits);

            _plan.PositionId =
                result.Position.Id;

            SetLifecycleState(
                LifecycleState.LivePosition,
                "AGGRESSIVE ENTRY • FILLED");

            ReconcileLivePlanToActualFill(
                result.Position,
                closedM5);

            double maximumFillDistance =
                Math.Max(
                    Symbol.TickSize * 2,
                    Math.Max(
                        Symbol.PipSize * 0.5,
                        Math.Max(
                            (Symbol.Ask - Symbol.Bid) * 2,
                            atr *
                            Math.Max(
                                0.10,
                                MaximumEntryExtensionAtr))));

            if (Math.Abs(
                    result.Position.EntryPrice -
                    entry) >
                maximumFillDistance)
            {
                SetLifecycleState(
                    LifecycleState.ExitRequested,
                    "AGGRESSIVE FILL MISMATCH");

                bool closed =
                    TryClosePosition(
                        result.Position,
                        "AGGRESSIVE FILL MISMATCH");

                if (!closed)
                {
                    SetLifecycleState(
                        LifecycleState.RecoveryRequired,
                        "AGGRESSIVE FILL MISMATCH • CLOSE REJECTED");
                }

                SendUnifiedAlert(
                    "FILL-MISMATCH|" +
                    result.Position.Id,
                    "CFIP AGGRESSIVE FILL OUTSIDE EXECUTION ENVELOPE | #" +
                    result.Position.Id,
                    _reaction.Direction,
                    true);

                return false;
            }

            EnrichLivePlanTargets(
                closedM5);

            return true;
        }
    }
}
