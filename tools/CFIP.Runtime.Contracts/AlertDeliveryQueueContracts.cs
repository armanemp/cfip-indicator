using System;
using CFIP.Contracts;

namespace cAlgo
{
    internal static class AlertDeliveryQueueContracts
    {
        private static DateTime Utc(int hour)
        {
            return new DateTime(
                2026,
                10,
                3,
                hour,
                0,
                0,
                DateTimeKind.Utc);
        }

        internal static void Run()
        {
            DateTime now = Utc(12);

            ContractIdentity identity1 =
                new ContractIdentity(
                    ContractVersion.Current,
                    "SIG-1",
                    "SCENARIO-1",
                    "PLAN-1",
                    "TEST",
                    TradeDirection.Buy,
                    CFIP.Contracts.OpportunityLane.Tactical,
                    "M5",
                    now,
                    10,
                    null,
                    1,
                    "SIG-1",
                    "EVENT-1");

            ContractIdentity identity2 =
                new ContractIdentity(
                    ContractVersion.Current,
                    "SIG-1",
                    "SCENARIO-1",
                    "PLAN-1",
                    "TEST",
                    TradeDirection.Buy,
                    OpportunityLane.Tactical,
                    "M5",
                    now,
                    10,
                    null,
                    2,
                    "SIG-1",
                    "EVENT-2");

            AlertEnvelope first =
                new AlertEnvelope(
                    identity1,
                    SignalStage.Watch,
                    "ALERT-REV-1",
                    "WATCH|10|1",
                    "watch",
                    false,
                    true,
                    now);

            AlertEnvelope revision =
                new AlertEnvelope(
                    identity2,
                    SignalStage.Watch,
                    "ALERT-REV-2",
                    "WATCH|10|1",
                    "watch-revision",
                    false,
                    true,
                    now);

            AlertDelivery firstDelivery =
                new AlertDelivery(
                    first,
                    1,
                    true,
                    "Announcement",
                    "");

            AlertDelivery revisionDelivery =
                new AlertDelivery(
                    revision,
                    1,
                    true,
                    "Announcement",
                    "");

            AlertDeliveryQueue queue =
                new AlertDeliveryQueue(3);

            Assert(
                queue.Enqueue(firstDelivery),
                "canonical alert queue accepts first event");

            Assert(
                !queue.Enqueue(revisionDelivery) &&
                queue.Count == 1,
                "same causal alert event is rejected even when alert id and revision change");

            AlertDelivery delivered;
            Assert(
                queue.TryDequeue(out delivered) &&
                delivered.Message == "watch",
                "first canonical alert remains the only pending delivery");

            Assert(
                queue.Enqueue(revisionDelivery) &&
                queue.Count == 1,
                "same causal event may be re-armed after actual delivery");

            Console.WriteLine(
                "AlertDeliveryQueue canonical identity contract PASS");
        }

        private static void Assert(
            bool condition,
            string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }
    }
}
