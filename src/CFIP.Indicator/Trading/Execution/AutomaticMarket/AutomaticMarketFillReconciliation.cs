using System;
using CFIP.Contracts;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool TryAcceptAutomaticMarketFill(
            int closedM5,
            TradeResult result)
        {            _lastAutoM5 =
                closedM5;

            _plan.OriginalVolume =
                result.Position.VolumeInUnits;

            _plan.PositionId =
                result.Position.Id;

            string fillExecutionReason;

            if (!IsExecutableFillPrice(
                    _plan,
                    result.Position.EntryPrice,
                    out fillExecutionReason))
            {
                SetLifecycleState(
                    LifecycleState.ExitRequested,
                    "MARKET FILL MISMATCH");

                _autoExecutionBlockReason =
                    "FILL MISMATCH • " +
                    fillExecutionReason;

                SetAutoTradingState(
                    "ERROR",
                    fillExecutionReason);

                ManagementCommandRequestStatus closeStatus =
                    RequestClosePosition(
                        result.Position,
                        "MARKET FILL MISMATCH");

                if (!closeStatus.IsAccepted())
                {
                    SetLifecycleState(
                        LifecycleState.RecoveryRequired,
                        "MARKET FILL MISMATCH • CLOSE REJECTED");
                }

                SendUnifiedAlert(
                    "FILL-MISMATCH|" +
                    result.Position.Id,
                    "CFIP ACCEPTED BROKER FILL OUTSIDE EXECUTION ENVELOPE | #" +
                    result.Position.Id +
                    " | " +
                    fillExecutionReason,
                    _plan.Direction,
                    true);

                return false;
            }

            bool fillPlanReconciled =
                ReconcileLivePlanToActualFill(
                    result.Position,
                    closedM5);

            if (!fillPlanReconciled)
            {
                _brokerProtectionRecoveryRequired = true;
                SetLifecycleState(
                    LifecycleState.RecoveryRequired,
                    "MARKET POST-FILL RECONCILIATION FAILED");

                _autoExecutionBlockReason =
                    "MARKET POST-FILL RECONCILIATION FAILED";

                return false;
            }

            _plan.IsLivePosition = true;

            SetLifecycleState(
                LifecycleState.LivePosition,
                "MARKET ENTRY • FILLED");

            _autoExecutionBlockReason =
                "EXECUTED";

            return true;
        }
    }
}
