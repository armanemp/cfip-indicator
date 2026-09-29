using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool EvaluateBrokerProtection(
            Position position,
            out bool brokerStopValid,
            out bool brokerTargetValid)
        {
            brokerStopValid = false;
            brokerTargetValid = false;

            if (position == null)
                return false;

            int direction =
                position.TradeType == TradeType.Buy
                    ? 1
                    : -1;

            double market =
                direction == 1
                    ? Symbol.Bid
                    : Symbol.Ask;

            brokerStopValid =
                position.StopLoss.HasValue &&
                IsFinitePositive(
                    position.StopLoss.Value) &&
                IsValidManagedStop(
                    direction,
                    position.EntryPrice,
                    market,
                    position.StopLoss.Value);

            brokerTargetValid =
                position.TakeProfit.HasValue &&
                IsFinitePositive(
                    position.TakeProfit.Value) &&
                IsValidTarget(
                    direction,
                    position.EntryPrice,
                    position.TakeProfit.Value);


            bool serverTakeProfitLadderActive =
                IsServerSideTakeProfitLadderActive(
                    position);

            return
                brokerStopValid &&
                (serverTakeProfitLadderActive ||
                 !SyncBrokerTakeProfit ||
                 brokerTargetValid);
        }
        private bool IsServerSideTakeProfitLadderActive(
            Position position)
        {
            if (position == null)
                return false;

            try
            {
                AbsoluteTakeProfitProtections protections =
                    position.AbsoluteTakeProfitProtections;

                return protections != null &&
                       protections.FirstTakeProfit != null &&
                       protections.SecondTakeProfit != null &&
                       protections.LastTakeProfit != null;
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP server TP ladder health check failed: {0}",
                    ex.Message);
                return false;
            }
        }
    }
}
