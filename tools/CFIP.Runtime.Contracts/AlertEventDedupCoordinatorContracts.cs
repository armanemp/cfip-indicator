using System;

namespace cAlgo
{
    internal static class AlertEventDedupCoordinatorContracts
    {
        public static void Run()
        {
            DateTime now =
                new DateTime(
                    2026,
                    10,
                    4,
                    18,
                    0,
                    0,
                    DateTimeKind.Utc);

            string primaryEvent =
                "BTCUSD|SIGNAL-1|SCENARIO-1|639267324000000000|1|BOS|2000|1";

            string independentEvent =
                "BTCUSD|SIGNAL-1|SCENARIO-2|639267324000000000|1|ACTION|2001|1";

            AlertEventDedupCoordinator.Release(
                primaryEvent);
            AlertEventDedupCoordinator.Release(
                independentEvent);

            Assert(
                AlertEventDedupCoordinator.TryClaim(
                    primaryEvent,
                    now),
                "first causal alert claim must succeed");

            Assert(
                !AlertEventDedupCoordinator.TryClaim(
                    primaryEvent,
                    now.AddSeconds(1)),
                "same causal alert must be rejected while within the shared claim lifetime");

            Assert(
                AlertEventDedupCoordinator.TryClaim(
                    independentEvent,
                    now.AddSeconds(1)),
                "independent alert event must not be suppressed by another event claim");

            Assert(
                AlertEventDedupCoordinator.TryClaim(
                    primaryEvent,
                    now.AddSeconds(
                        AlertEventDedupCoordinator.ClaimLifetime +
                        1)),
                "same causal alert may re-arm after the bounded shared dedup lifetime");

            AlertEventDedupCoordinator.Release(
                primaryEvent);
            AlertEventDedupCoordinator.Release(
                independentEvent);

            Console.WriteLine(
                "Cross-instance alert dedup contracts PASS");
        }

        private static void Assert(
            bool condition,
            string message)
        {
            if (!condition)
                throw new InvalidOperationException(
                    "Cross-instance alert dedup contract failed: " +
                    message);
        }
    }
}
