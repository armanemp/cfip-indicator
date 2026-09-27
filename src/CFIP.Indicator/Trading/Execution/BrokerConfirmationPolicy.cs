// CFIP Indicator — BrokerConfirmationPolicy.cs
// Deterministic adoption rules for broker mutation results.

namespace cAlgo
{
    internal static class BrokerConfirmationPolicy
    {
        public static bool IsSuccessfulMutation(
            bool resultPresent,
            bool successful)
        {
            return resultPresent && successful;
        }

        public static bool CanAdoptPosition(
            bool resultPresent,
            bool successful,
            bool positionPresent)
        {
            return
                IsSuccessfulMutation(
                    resultPresent,
                    successful) &&
                positionPresent;
        }

        public static bool CanAdoptPendingOrder(
            bool resultPresent,
            bool successful,
            bool pendingOrderPresent)
        {
            return
                IsSuccessfulMutation(
                    resultPresent,
                    successful) &&
                pendingOrderPresent;
        }
    }
}
