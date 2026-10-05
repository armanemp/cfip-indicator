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
                IsExistingManagedStopHealthy(
                    direction,
                    position.EntryPrice,
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
                AdoptServerSideTakeProfitLadder(
                    position);

            return
                brokerStopValid &&
                (serverTakeProfitLadderActive ||
                 !CbotCanManage() ||
                 brokerTargetValid);
        }
    }
}
