using System;

namespace cAlgo
{
    internal static class Program
    {
        private static void Main()
        {
            VerifyRejectedResultsNeverAdopted();
            VerifyConfirmedPositionIsAdopted();
            VerifyConfirmedPendingOrderIsAdopted();
            VerifyMissingEntitiesAreRejected();
            VerifyMutationSuccessRequiresResult();

            Console.WriteLine("Execution contracts OK");
        }

        private static void VerifyRejectedResultsNeverAdopted()
        {
            Assert(
                !BrokerConfirmationPolicy.CanAdoptPosition(
                    true,
                    false,
                    true),
                "rejected position mutation");

            Assert(
                !BrokerConfirmationPolicy.CanAdoptPendingOrder(
                    true,
                    false,
                    true),
                "rejected pending mutation");
        }

        private static void VerifyConfirmedPositionIsAdopted()
        {
            Assert(
                BrokerConfirmationPolicy.CanAdoptPosition(
                    true,
                    true,
                    true),
                "confirmed position mutation");
        }

        private static void VerifyConfirmedPendingOrderIsAdopted()
        {
            Assert(
                BrokerConfirmationPolicy.CanAdoptPendingOrder(
                    true,
                    true,
                    true),
                "confirmed pending mutation");
        }

        private static void VerifyMissingEntitiesAreRejected()
        {
            Assert(
                !BrokerConfirmationPolicy.CanAdoptPosition(
                    true,
                    true,
                    false),
                "missing position entity");

            Assert(
                !BrokerConfirmationPolicy.CanAdoptPendingOrder(
                    true,
                    true,
                    false),
                "missing pending entity");
        }

        private static void VerifyMutationSuccessRequiresResult()
        {
            Assert(
                !BrokerConfirmationPolicy.IsSuccessfulMutation(
                    false,
                    true),
                "missing trade result");

            Assert(
                BrokerConfirmationPolicy.IsSuccessfulMutation(
                    true,
                    true),
                "successful trade result");
        }

        private static void Assert(bool condition, string name)
        {
            if (!condition)
                throw new InvalidOperationException(
                    "Execution contract failed: " + name);
        }
    }
}
