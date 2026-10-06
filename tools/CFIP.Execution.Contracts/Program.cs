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
            VerifyContractVersionDecodeBoundary();

            VerifyLifecycleEventIdempotency();
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
        }        private static void VerifyLifecycleEventIdempotency()
        {
            LifecycleEventIdempotencyGuard guard =
                new LifecycleEventIdempotencyGuard();

            Assert(
                guard.TryBegin("POSITION_OPENED", 101),
                "first position-open event");

            Assert(
                !guard.TryBegin("POSITION_OPENED", 101),
                "duplicate position-open event blocked");

            Assert(
                guard.TryBegin("POSITION_OPENED", 102),
                "different entity remains independent");

            Assert(
                guard.TryBegin("POSITION_CLOSED", 101),
                "different event type remains independent");

            Assert(
                !guard.TryBegin("POSITION_CLOSED", 101),
                "duplicate position-close event blocked");

            Assert(
                !guard.TryBegin("POSITION_OPENED", 0),
                "invalid entity blocked");
        }



        private static void VerifyContractVersionDecodeBoundary()
        {
            Assert(
                !CbotExecutionStateCodec.TryDeserializePresence(
                    "{\"ContractVersion\":999}",
                    out _),
                "unsupported cBot presence contract version");

            Assert(
                !CbotExecutionStateCodec.TryDeserialize(
                    "{\"ContractVersion\":999}",
                    out _),
                "unsupported cBot execution-state contract version");

            Assert(
                ContractVersion.Current == 2,
                "current contract version remains canonical");
        }

        private static void Assert(bool condition, string name)
        {
            if (!condition)
                throw new InvalidOperationException(
                    "Execution contract failed: " + name);
        }
    }
}
