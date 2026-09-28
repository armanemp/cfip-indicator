using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void ReportConfirmedPendingOrderPlacement(
            PendingOrder order,
            int direction,
            int closedM5,
            string kind)
        {
            if (order == null)
            {
                _autoOrdersBlockReason =
                    kind + " • BROKER CONFIRMATION MISSING";
                return;
            }

            _autoOrdersBlockReason =
                "ORDER PLACED • " +
                kind + " " +
                Price(order.TargetPrice);

            string brokerStop =
                order.StopLoss.HasValue &&
                IsFinitePositive(order.StopLoss.Value)
                    ? Price(order.StopLoss.Value)
                    : "RECOVERY";

            string brokerTarget =
                order.TakeProfit.HasValue &&
                IsFinitePositive(order.TakeProfit.Value)
                    ? Price(order.TakeProfit.Value)
                    : "RECOVERY";

            SendUnifiedAlert(
                "PENDING-" + kind + "|" + closedM5,
                "CFIP " + kind +
                " | " + (direction == 1 ? "BUY" : "SELL") +
                " | BROKER ENTRY " + Price(order.TargetPrice) +
                " | BROKER SL " + brokerStop +
                " | BROKER TP " + brokerTarget,
                direction,
                true);
        }
    }
}