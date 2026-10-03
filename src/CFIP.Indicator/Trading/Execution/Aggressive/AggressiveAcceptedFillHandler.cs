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

                ManagementCommandRequestStatus closeStatus =
                    TryClosePosition(
                        result.Position,
                        "AGGRESSIVE FILL MISMATCH");

                if (!closeStatus.IsAccepted())
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

            _aggressiveEntryPolicy.ResetQualification();

            _plan =
                CreateManagedPlanFromExecution(
                    _reaction.Direction,
                    result.Position.EntryPrice,
                    actualStop,
                    actualTarget,
                    closedM5,
                    result.Position.VolumeInUnits,
                    ExecutionMode.BreakoutMarket,
                    true);

            _plan.PositionId =
                result.Position.Id;

            bool fillPlanReconciled =
                ReconcileLivePlanToActualFill(
                    result.Position,
                    closedM5);

            if (!fillPlanReconciled)
            {
                _brokerProtectionRecoveryRequired = true;
                SetLifecycleState(
                    LifecycleState.RecoveryRequired,
                    "AGGRESSIVE POST-FILL RECONCILIATION FAILED");

                _autoExecutionBlockReason =
                    "AGGRESSIVE POST-FILL RECONCILIATION FAILED";

                SendUnifiedAlert(
                    "AGGRESSIVE-POST-FILL-RECONCILIATION-FAILED|" +
                    result.Position.Id,
                    "CFIP AGGRESSIVE POST-FILL EXIT RECONCILIATION FAILED | #" +
                    result.Position.Id,
                    _reaction.Direction,
                    true);

                return false;
            }

            actualStop =
                _plan.Stop;

            actualTarget =
                AutoTarget(
                    _plan,
                    EffectiveAutoTpStage());

            if (!IsExecutionPlanConsistent(
                    _reaction.Direction,
                    _plan.Entry,
                    actualStop,
                    actualTarget))
            {
                _brokerProtectionRecoveryRequired = true;
                SetLifecycleState(
                    LifecycleState.RecoveryRequired,
                    "AGGRESSIVE POST-FILL PLAN GEOMETRY INVALID");

                _autoExecutionBlockReason =
                    "AGGRESSIVE POST-FILL PLAN GEOMETRY INVALID";

                SendUnifiedAlert(
                    "AGGRESSIVE-POST-FILL-PLAN-INVALID|" +
                    result.Position.Id,
                    "CFIP AGGRESSIVE POST-FILL MANAGED PLAN GEOMETRY INVALID | #" +
                    result.Position.Id,
                    _reaction.Direction,
                    true);

                return false;
            }

            SetLifecycleState(
                LifecycleState.LivePosition,
                "AGGRESSIVE ENTRY • FILLED");

            _autoExecutionBlockReason =
                "EXECUTED";

            EnrichLivePlanTargets(
                closedM5);

            return true;
        }
    }
}
