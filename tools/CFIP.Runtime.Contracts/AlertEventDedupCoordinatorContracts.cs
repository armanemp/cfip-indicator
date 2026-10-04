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

            string eventKey =
                "BTCUSD|SIGNAL-1|SCENARIO-1|PLAN-1|639267324000000000|1|BOS|2000|1";

            AlertEventDedupCoordinator.Release(
                eventKey);

            Assert(
                AlertEventDedupCoordinator.TryClaim(
                    eventKey,
                    now),
                "first causal alert claim must succeed");

            Assert(
                !AlertEventDedupCoordinator.TryClaim(
                    eventKey,
                    now.AddSeconds(1)),
                "same causal alert must be rejected while within the shared claim lifetime");

            string persisted =
                AlertEventDedupCoordinator.Serialize(
                    now.AddSeconds(2));

            AlertEventDedupCoordinator.Release(
                eventKey);

            AlertEventDedupCoordinator.Import(
                persisted,
                now.AddSeconds(2));

            Assert(
                !AlertEventDedupCoordinator.TryClaim(
                    eventKey,
                    now.AddSeconds(3)),
                "persisted causal alert claim must reject an equivalent claim from another runtime instance");

            Assert(
                AlertEventDedupCoordinator.TryClaim(
                    eventKey,
                    now.AddSeconds(
                        AlertEventDedupCoordinator.ClaimLifetime +
                        1)),
                "same causal alert may re-arm after the bounded shared dedup lifetime");

            AlertEventDedupCoordinator.Release(
                eventKey);

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