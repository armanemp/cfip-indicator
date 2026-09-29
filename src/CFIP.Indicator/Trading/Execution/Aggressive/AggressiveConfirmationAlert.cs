using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void SendAggressiveConfirmationAlert(
            int closedM5,
            int direction,
            long positionId,
            double entryPrice,
            double confirmedStop,
            double confirmedTarget)
        {
            SendUnifiedAlert(
                "AUTO-REACTION|" +
                closedM5,
                "CFIP AUTO REACTION " +
                (direction == 1 ? "BUY" : "SELL") +
                " EXECUTED | #" +
                positionId +
                " | ENTRY " +
                Price(entryPrice) +
                " | BROKER SL " +
                (IsFinitePositive(confirmedStop)
                    ? Price(confirmedStop)
                    : "RECOVERY") +
                " | BROKER TP " +
                (IsFinitePositive(confirmedTarget)
                    ? Price(confirmedTarget)
                    : "RECOVERY"),
                direction,
                true);
        }
    }
}
