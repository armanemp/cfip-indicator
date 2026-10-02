using System;
using CFIP.Contracts;
using CFIP.cBot.Shadow;

namespace CFIP.cBot.Shadow.Tests
{
    internal static class Program
    {
        private static readonly DateTime Now =
            new DateTime(2026, 10, 2, 4, 0, 0, DateTimeKind.Utc);

        private static ShadowBrokerSnapshot Safe =
            new ShadowBrokerSnapshot(0, 0, "XAUUSD", 2650, 2650.1, 0.1);

        private static void Main()
        {
            Ready(TradeDirection.Buy, ExecutionAction.Market, 1, "K1");
            Ready(TradeDirection.Sell, ExecutionAction.PendingLimit, -1, "K2");
            ObserveNoIntent();
            Expiry();
            Version();
            IdentityMismatch();
            Symbol();
            Capacity();
            Quote();
            WrongSide();
            RevisionRules();
            ProviderRevisionMismatch();
            CoordinatorRecheck();
            Console.WriteLine("CBOT-P3 shadow host behavioral contracts PASS");
        }

        private static void Ready(
            TradeDirection direction,
            ExecutionAction action,
            int expectedDirection,
            string key)
        {
            double entry = 100;
            double stop = direction == TradeDirection.Buy ? 99 : 101;
            double target = direction == TradeDirection.Buy ? 101 : 99;

            ShadowHostResult r = Validate(
                Build(1, direction, action, key, Now.AddMinutes(5), entry, stop, target, SignalStage.Confirmed),
                Safe);

            Assert(
                r.State == ShadowHostState.Ready &&
                (int)direction == expectedDirection &&
                r.NewRevision,
                "valid BUY/SELL intent must be shadow-ready");
        }

        private static void ObserveNoIntent()
        {
            ShadowHostResult r = Validate(
                Build(1, TradeDirection.Buy, ExecutionAction.None, "K3", Now.AddMinutes(5), 100, 99, 101, SignalStage.Watch),
                Safe);

            Assert(
                r.State == ShadowHostState.Observing &&
                r.Reason == "NO EXECUTION INTENT",
                "watch without intent must stay observing");
        }

        private static void Expiry()
        {
            ShadowHostResult r = Validate(
                Build(1, TradeDirection.Buy, ExecutionAction.Market, "K4", Now.AddSeconds(-1), 100, 99, 101, SignalStage.Confirmed),
                Safe);

            Assert(r.State == ShadowHostState.Expired, "expired signal must fail closed");
        }

        private static void Version()
        {
            ShadowHostResult r = Validate(
                Build(1, TradeDirection.Buy, ExecutionAction.Market, "K5", Now.AddMinutes(5), 100, 99, 101, SignalStage.Confirmed, ContractVersion.Current + 1),
                Safe);

            Assert(r.State == ShadowHostState.Blocked &&
                   r.Reason == "INCOMPATIBLE CONTRACT VERSION",
                   "unsupported contract version must block");
        }

        private static void IdentityMismatch()
        {
            SignalEnvelope e = Build(
                1, TradeDirection.Buy, ExecutionAction.Market, "K6",
                Now.AddMinutes(5), 100, 99, 101, SignalStage.Confirmed);

            ExecutionIntent badIntent =
                new ExecutionIntent(
                    new ContractIdentity(
                        ContractVersion.Current,
                        "OTHER",
                        e.Identity.ScenarioId,
                        e.Identity.PlanId,
                        e.Identity.Symbol,
                        e.Identity.Direction,
                        e.Identity.Lane,
                        e.Identity.SourceTimeframe,
                        e.Identity.CreatedUtc,
                        e.Identity.CreatedClosedM5,
                        e.Identity.ExpiryUtc,
                        e.Identity.Revision,
                        e.Identity.CorrelationId,
                        "OTHER"),
                    ExecutionAction.Market, 100, 99, 101, 1000, "UNIT",
                    Now, Now.AddMinutes(5), "bad");

            SignalEnvelope invalid =
                new SignalEnvelope(e.Identity, e.Stage, e.Plan, badIntent, Now);

            ShadowHostResult r = Validate(invalid, Safe);

            Assert(r.State == ShadowHostState.Blocked &&
                   r.Reason == "INTENT IDENTITY MISMATCH",
                   "intent identity mismatch must block");
        }

        private static void Symbol()
        {
            ShadowHostResult r = Validate(
                Build(1, TradeDirection.Buy, ExecutionAction.Market, "K7",
                    Now.AddMinutes(5), 100, 99, 101, SignalStage.Confirmed, ContractVersion.Current, "EURUSD"),
                Safe);

            Assert(r.State == ShadowHostState.Blocked &&
                   r.Reason == "SYMBOL SCOPE MISMATCH",
                   "wrong symbol must block");
        }

        private static void Capacity()
        {
            ShadowBrokerSnapshot broker =
                new ShadowBrokerSnapshot(1, 0, "XAUUSD", 2650, 2650.1, 0.1);

            ShadowHostResult r = Validate(
                Build(1, TradeDirection.Buy, ExecutionAction.Market, "K9",
                    Now.AddMinutes(5), 100, 99, 101, SignalStage.Confirmed),
                broker);

            Assert(r.State == ShadowHostState.Blocked &&
                   r.Reason == "SINGLE-PLAN CAPACITY BLOCKED",
                   "single-plan capacity must block");
        }

        private static void Quote()
        {
            ShadowBrokerSnapshot broker =
                new ShadowBrokerSnapshot(0, 0, "XAUUSD", double.NaN, 2650.1, 0.1);

            ShadowHostResult r = Validate(
                Build(1, TradeDirection.Buy, ExecutionAction.Market, "K10",
                    Now.AddMinutes(5), 100, 99, 101, SignalStage.Confirmed),
                broker);

            Assert(r.State == ShadowHostState.Blocked &&
                   r.Reason == "INVALID LIVE QUOTE",
                   "invalid quote must fail closed");
        }

        private static void WrongSide()
        {
            ShadowHostResult buy =
                Validate(
                    Build(1, TradeDirection.Buy, ExecutionAction.Market, "K11",
                        Now.AddMinutes(5), 100, 101, 99, SignalStage.Confirmed),
                    Safe);

            ShadowHostResult sell =
                Validate(
                    Build(1, TradeDirection.Sell, ExecutionAction.Market, "K12",
                        Now.AddMinutes(5), 100, 99, 101, SignalStage.Confirmed),
                    Safe);

            Assert(
                buy.State == ShadowHostState.Blocked &&
                sell.State == ShadowHostState.Blocked,
                "BUY/SELL wrong-side plan geometry must be symmetric");
        }

        private static void RevisionRules()
        {
            SignalEnvelope e =
                Build(4, TradeDirection.Buy, ExecutionAction.Market, "K15",
                    Now.AddMinutes(5), 100, 99, 101, SignalStage.Confirmed);

            ShadowHostResult stale =
                Validate(e, Safe, 5, "K14");

            ShadowHostResult conflict =
                Validate(e, Safe, 4, "OTHER");

            ShadowHostResult duplicate =
                Validate(e, Safe, 4, "K15");

            Assert(stale.Reason == "STALE REVISION", "lower revision must be stale");
            Assert(conflict.Reason == "REVISION CONFLICT", "same revision with different key must conflict");
            Assert(duplicate.State == ShadowHostState.Duplicate, "same revision/key must be duplicate");
        }

        private static void ProviderRevisionMismatch()
        {
            SignalEnvelope e =
                Build(18, TradeDirection.Buy, ExecutionAction.Market, "K18",
                    Now.AddMinutes(5), 100, 99, 101, SignalStage.Confirmed);

            ShadowHostCoordinator c = new ShadowHostCoordinator();

            ShadowHostResult r =
                c.Observe(
                    e,
                    Safe,
                    ContractVersion.Current,
                    17,
                    Now);

            Assert(
                r.State == ShadowHostState.Blocked &&
                r.Reason == "PROVIDER REVISION MISMATCH",
                "provider revision drift must fail closed");
        }

        private static void CoordinatorRecheck()
        {
            ShadowHostCoordinator c = new ShadowHostCoordinator();

            SignalEnvelope e =
                Build(6, TradeDirection.Buy, ExecutionAction.Market, "K17",
                    Now.AddMinutes(5), 100, 99, 101, SignalStage.Confirmed);

            ShadowHostResult blocked =
                c.Observe(
                    e,
                    new ShadowBrokerSnapshot(1, 0, "XAUUSD", 2650, 2650.1, 0.1),
                    ContractVersion.Current,
                    6,
                    Now);

            ShadowHostResult ready =
                c.Observe(e, Safe, ContractVersion.Current, 6, Now.AddSeconds(1));

            ShadowHostResult same =
                c.Observe(e, Safe, ContractVersion.Current, 6, Now.AddSeconds(1.1));

            Assert(
                blocked.Reason == "SINGLE-PLAN CAPACITY BLOCKED" &&
                ready.State == ShadowHostState.Ready &&
                same.State == ShadowHostState.Ready &&
                c.LastAcceptedRevision == 6,
                "transient broker block must recheck and dedupe thereafter");
        }

        private static ShadowHostResult Validate(
            SignalEnvelope e,
            ShadowBrokerSnapshot broker,
            long lastRevision = -1,
            string lastKey = "")
        {
            return ShadowHostValidator.Validate(
                e, broker, ContractVersion.Current, lastRevision, lastKey, Now);
        }

        private static SignalEnvelope Build(
            long revision,
            TradeDirection direction,
            ExecutionAction action,
            string key,
            DateTime expiry,
            double entry,
            double stop,
            double target,
            SignalStage stage,
            int version = ContractVersion.Current,
            string symbol = "XAUUSD")
        {
            ContractIdentity id =
                new ContractIdentity(
                    version,
                    "SIGNAL-" + revision,
                    "SCENARIO-" + revision,
                    "PLAN-" + revision,
                    symbol,
                    direction,
                    OpportunityLane.Tactical,
                    "M5",
                    Now.AddMinutes(-1),
                    10,
                    expiry,
                    revision,
                    "CORR-" + revision,
                    key);

            PlanSnapshot plan =
                new PlanSnapshot(
                    entry, entry, entry, entry, entry, 0,
                    stop, target, 0, 0, 0,
                    Math.Abs(entry - stop),
                    1, 1, 1, 1,
                    80, 80, 80, 80, 80, 0, 0, 0,
                    "TEST", "TEST", "TEST", "", "", "", "fixture");

            ExecutionIntent intent =
                action == ExecutionAction.None
                    ? null
                    : new ExecutionIntent(
                        id, action, entry, stop, target, 1000, "UNIT",
                        Now, expiry, "fixture");

            return new SignalEnvelope(id, stage, plan, intent, Now);
        }

        private static void Assert(bool ok, string message)
        {
            if (!ok)
                throw new InvalidOperationException(
                    "CBOT-P3 shadow behavioral contract failed: " + message);
        }
    }
}