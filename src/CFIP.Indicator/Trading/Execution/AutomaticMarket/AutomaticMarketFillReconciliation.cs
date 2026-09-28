using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool TryAcceptAutomaticMarketFill(
            int closedM5,
            TradeResult result,
            out string reason)
        {
            reason = "";

            if (!BrokerConfirmationPolicy.CanAdoptPosition(
                    true,
                    result != null &&
                    result.IsSuccessful,
                    result != null &&
                    result.Position != null))
            {
                reason =
                    result != null &&
                    result.Error.HasValue
                        ? result.Error.Value.ToString()
                        : "TRADE REJECTED";
                return false;
            }

            _lastAutoM5 =
                closedM5;

            _autoExecutionBlockReason =
                "EXECUTED";

            _plan.OriginalVolume =
                result.Position.VolumeInUnits;

            _plan.IsLivePosition = true;
            _plan.PositionId =
                result.Position.Id;

            SetLifecycleState(
                LifecycleState.LivePosition,
                "MARKET ENTRY • FILLED");

            ReconcileLivePlanToActualFill(
                result.Position,
                closedM5);

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

                bool closed =
                    TryClosePosition(
                        result.Position,
                        "MARKET FILL MISMATCH");

                if (!closed)
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

            return true;
        }
    }
}
