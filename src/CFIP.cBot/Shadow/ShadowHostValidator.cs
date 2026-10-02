using System;
using CFIP.Contracts;

namespace CFIP.cBot.Shadow
{
    public static class ShadowHostValidator
    {
        public const string ManagedLabel = "CFIP-SMART";
        public const string PendingManagedLabel = "CFIP-SMART-PENDING";

        public static ShadowHostResult Validate(
            SignalEnvelope envelope,
            ShadowBrokerSnapshot broker,
            int currentContractVersion,
            long lastRevision,
            string lastIdempotencyKey,
            DateTime nowUtc)
        {
            if (envelope == null)
                return Block("NO SNAPSHOT", false, 0, "", "", "", "");

            ContractIdentity identity =
                envelope.Identity;

            if (identity == null)
                return Block("MISSING IDENTITY", false, 0, "", "", "", "");

            bool newRevision =
                identity.Revision > lastRevision;

            if (identity.ContractVersion != currentContractVersion)
                return Block("INCOMPATIBLE CONTRACT VERSION", newRevision, identity.Revision, identity.SignalId, identity.ScenarioId, identity.PlanId, identity.IdempotencyKey);

            if (identity.Revision <= 0 ||
                string.IsNullOrWhiteSpace(identity.SignalId) ||
                string.IsNullOrWhiteSpace(identity.ScenarioId) ||
                string.IsNullOrWhiteSpace(identity.PlanId) ||
                string.IsNullOrWhiteSpace(identity.Symbol) ||
                string.IsNullOrWhiteSpace(identity.SourceTimeframe) ||
                string.IsNullOrWhiteSpace(identity.CorrelationId) ||
                string.IsNullOrWhiteSpace(identity.IdempotencyKey))
            {
                return Block("INVALID IDENTITY", newRevision, identity.Revision, identity.SignalId, identity.ScenarioId, identity.PlanId, identity.IdempotencyKey);
            }

            if (!string.IsNullOrWhiteSpace(broker?.Symbol) &&
                !string.Equals(
                    identity.Symbol,
                    broker.Symbol,
                    StringComparison.Ordinal))
            {
                return Block("SYMBOL SCOPE MISMATCH", newRevision, identity.Revision, identity.SignalId, identity.ScenarioId, identity.PlanId, identity.IdempotencyKey);
            }

            if (identity.Revision < lastRevision)
                return Block("STALE REVISION", false, identity.Revision, identity.SignalId, identity.ScenarioId, identity.PlanId, identity.IdempotencyKey);

            if (identity.Revision == lastRevision &&
                string.Equals(
                    identity.IdempotencyKey,
                    lastIdempotencyKey,
                    StringComparison.Ordinal))
            {
                return new ShadowHostResult(
                    ShadowHostState.Duplicate,
                    "DUPLICATE IDEMPOTENCY KEY",
                    false,
                    identity.Revision,
                    identity.SignalId,
                    identity.ScenarioId,
                    identity.PlanId,
                    identity.IdempotencyKey);
            }

            if (identity.Revision == lastRevision &&
                !string.Equals(
                    identity.IdempotencyKey,
                    lastIdempotencyKey,
                    StringComparison.Ordinal))
            {
                return Block("REVISION CONFLICT", false, identity.Revision, identity.SignalId, identity.ScenarioId, identity.PlanId, identity.IdempotencyKey);
            }

            if (identity.ExpiryUtc.HasValue &&
                identity.ExpiryUtc.Value <= nowUtc)
            {
                return new ShadowHostResult(
                    ShadowHostState.Expired,
                    "SIGNAL EXPIRED",
                    newRevision,
                    identity.Revision,
                    identity.SignalId,
                    identity.ScenarioId,
                    identity.PlanId,
                    identity.IdempotencyKey);
            }

            if (envelope.Intent != null &&
                envelope.Intent.ExpiryUtc.HasValue &&
                envelope.Intent.ExpiryUtc.Value <= nowUtc)
            {
                return new ShadowHostResult(
                    ShadowHostState.Expired,
                    "EXECUTION INTENT EXPIRED",
                    newRevision,
                    identity.Revision,
                    identity.SignalId,
                    identity.ScenarioId,
                    identity.PlanId,
                    identity.IdempotencyKey);
            }

            if (!ValidateIntentIdentity(
                    identity,
                    envelope.Intent))
            {
                return Block("INTENT IDENTITY MISMATCH", newRevision, identity.Revision, identity.SignalId, identity.ScenarioId, identity.PlanId, identity.IdempotencyKey);
            }

            if (!ValidatePlanIntegrity(
                    identity.Direction,
                    envelope.Plan,
                    out string planReason))
            {
                return Block(
                    planReason,
                    newRevision,
                    identity.Revision,
                    identity.SignalId,
                    identity.ScenarioId,
                    identity.PlanId,
                    identity.IdempotencyKey);
            }

            if (envelope.Stage == SignalStage.Unavailable ||
                envelope.Stage == SignalStage.Initializing)
            {
                return new ShadowHostResult(
                    ShadowHostState.Waiting,
                    "PROVIDER NOT READY",
                    newRevision,
                    identity.Revision,
                    identity.SignalId,
                    identity.ScenarioId,
                    identity.PlanId,
                    identity.IdempotencyKey);
            }

            if (envelope.Stage == SignalStage.Blocked)
                return Block("PROVIDER BLOCKED", newRevision, identity.Revision, identity.SignalId, identity.ScenarioId, identity.PlanId, identity.IdempotencyKey);

            if (envelope.Stage == SignalStage.Expired)
            {
                return new ShadowHostResult(
                    ShadowHostState.Expired,
                    "PROVIDER EXPIRED",
                    newRevision,
                    identity.Revision,
                    identity.SignalId,
                    identity.ScenarioId,
                    identity.PlanId,
                    identity.IdempotencyKey);
            }

            if (envelope.Stage != SignalStage.Confirmed ||
                envelope.Intent == null ||
                envelope.Intent.Action == ExecutionAction.None)
            {
                return new ShadowHostResult(
                    ShadowHostState.Observing,
                    envelope.Stage == SignalStage.Active
                        ? "ACTIVE STATE • NO NEW ACTION"
                        : "NO EXECUTION INTENT",
                    newRevision,
                    identity.Revision,
                    identity.SignalId,
                    identity.ScenarioId,
                    identity.PlanId,
                    identity.IdempotencyKey);
            }

            if (!ValidateBrokerSafety(
                    identity,
                    envelope.Intent,
                    broker,
                    out string brokerReason))
            {
                return Block(
                    brokerReason,
                    newRevision,
                    identity.Revision,
                    identity.SignalId,
                    identity.ScenarioId,
                    identity.PlanId,
                    identity.IdempotencyKey);
            }

            return new ShadowHostResult(
                ShadowHostState.Ready,
                "SHADOW ELIGIBLE • BROKER MUTATION DISARMED",
                newRevision,
                identity.Revision,
                identity.SignalId,
                identity.ScenarioId,
                identity.PlanId,
                identity.IdempotencyKey);
        }

        private static bool ValidateIntentIdentity(
            ContractIdentity identity,
            ExecutionIntent intent)
        {
            if (intent == null)
                return true;

            ContractIdentity intentIdentity =
                intent.Identity;

            return
                intentIdentity != null &&
                string.Equals(
                    intentIdentity.SignalId,
                    identity.SignalId,
                    StringComparison.Ordinal) &&
                string.Equals(
                    intentIdentity.ScenarioId,
                    identity.ScenarioId,
                    StringComparison.Ordinal) &&
                string.Equals(
                    intentIdentity.PlanId,
                    identity.PlanId,
                    StringComparison.Ordinal) &&
                string.Equals(
                    intentIdentity.Symbol,
                    identity.Symbol,
                    StringComparison.Ordinal) &&
                intentIdentity.Direction == identity.Direction &&
                intentIdentity.Lane == identity.Lane &&
                intentIdentity.Revision == identity.Revision;
        }

        private static bool ValidatePlanIntegrity(
            TradeDirection direction,
            PlanSnapshot plan,
            out string reason)
        {
            reason = "";

            if (plan == null)
                return true;

            if (!IsFinitePositiveOrZero(plan.Entry) ||
                !IsFinitePositiveOrZero(plan.Stop) ||
                !IsFinitePositiveOrZero(plan.Tp1))
            {
                reason = "NON-FINITE PLAN GEOMETRY";
                return false;
            }

            if (direction == TradeDirection.Buy)
            {
                if (plan.Stop > 0 &&
                    plan.Entry > 0 &&
                    plan.Stop >= plan.Entry)
                {
                    reason = "BUY PLAN STOP WRONG SIDE";
                    return false;
                }

                if (plan.Tp1 > 0 &&
                    plan.Entry > 0 &&
                    plan.Tp1 <= plan.Entry)
                {
                    reason = "BUY PLAN TP1 WRONG SIDE";
                    return false;
                }
            }
            else if (direction == TradeDirection.Sell)
            {
                if (plan.Stop > 0 &&
                    plan.Entry > 0 &&
                    plan.Stop <= plan.Entry)
                {
                    reason = "SELL PLAN STOP WRONG SIDE";
                    return false;
                }

                if (plan.Tp1 > 0 &&
                    plan.Entry > 0 &&
                    plan.Tp1 >= plan.Entry)
                {
                    reason = "SELL PLAN TP1 WRONG SIDE";
                    return false;
                }
            }

            return true;
        }

        public static ShadowHostResult RevalidateBrokerSafety(
            SignalEnvelope envelope,
            ShadowBrokerSnapshot broker)
        {
            if (envelope == null ||
                envelope.Identity == null)
            {
                return Block(
                    "NO SNAPSHOT",
                    false,
                    0,
                    "",
                    "",
                    "",
                    "");
            }

            if (envelope.Intent == null ||
                envelope.Intent.Action == ExecutionAction.None)
            {
                return new ShadowHostResult(
                    ShadowHostState.Observing,
                    "NO EXECUTION INTENT",
                    false,
                    envelope.Identity.Revision,
                    envelope.Identity.SignalId,
                    envelope.Identity.ScenarioId,
                    envelope.Identity.PlanId,
                    envelope.Identity.IdempotencyKey);
            }

            if (!ValidateBrokerSafety(
                    envelope.Identity,
                    envelope.Intent,
                    broker,
                    out string reason))
            {
                return Block(
                    reason,
                    false,
                    envelope.Identity.Revision,
                    envelope.Identity.SignalId,
                    envelope.Identity.ScenarioId,
                    envelope.Identity.PlanId,
                    envelope.Identity.IdempotencyKey);
            }

            return new ShadowHostResult(
                ShadowHostState.Ready,
                "SHADOW ELIGIBLE • BROKER MUTATION DISARMED",
                false,
                envelope.Identity.Revision,
                envelope.Identity.SignalId,
                envelope.Identity.ScenarioId,
                envelope.Identity.PlanId,
                envelope.Identity.IdempotencyKey);
        }

        private static bool ValidateBrokerSafety(
            ContractIdentity identity,
            ExecutionIntent intent,
            ShadowBrokerSnapshot broker,
            out string reason)
        {
            reason = "";

            if (broker == null)
            {
                reason = "BROKER STATE UNAVAILABLE";
                return false;
            }

            if (!broker.TradingPermissionAllowed)
            {
                reason = "TRADING PERMISSION BLOCKED";
                return false;
            }

            if (broker.ManagedPositionCount +
                broker.ManagedPendingOrderCount >= 1)
            {
                reason = "SINGLE-PLAN CAPACITY BLOCKED";
                return false;
            }

            if (double.IsNaN(broker.Bid) ||
                double.IsInfinity(broker.Bid) ||
                double.IsNaN(broker.Ask) ||
                double.IsInfinity(broker.Ask) ||
                broker.Bid <= 0 ||
                broker.Ask <= 0)
            {
                reason = "INVALID LIVE QUOTE";
                return false;
            }

            if (broker.PipSize <= 0 ||
                double.IsNaN(broker.PipSize) ||
                double.IsInfinity(broker.PipSize))
            {
                reason = "INVALID PIP SIZE";
                return false;
            }

            if (intent.RequestedVolume.HasValue &&
                (!IsFinitePositiveOrZero(intent.RequestedVolume.Value) ||
                 intent.RequestedVolume.Value < 0))
            {
                reason = "INVALID REQUESTED VOLUME";
                return false;
            }

            if (!IsFinitePositive(intent.RequestedEntry) ||
                !IsFinitePositive(intent.Stop) ||
                !IsFinitePositive(intent.InitialTarget))
            {
                reason = "INVALID EXECUTION GEOMETRY";
                return false;
            }

            if (identity.Direction == TradeDirection.Buy)
            {
                if (intent.Stop >= intent.RequestedEntry ||
                    intent.InitialTarget <= intent.RequestedEntry)
                {
                    reason = "BUY EXECUTION GEOMETRY WRONG SIDE";
                    return false;
                }
            }
            else if (identity.Direction == TradeDirection.Sell)
            {
                if (intent.Stop <= intent.RequestedEntry ||
                    intent.InitialTarget >= intent.RequestedEntry)
                {
                    reason = "SELL EXECUTION GEOMETRY WRONG SIDE";
                    return false;
                }
            }
            else
            {
                return Fail(
                    "INVALID EXECUTION DIRECTION",
                    out reason);
            }

            return true;
        }

        private static bool IsFinitePositiveOrZero(double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value) &&
                value >= 0;
        }

        private static bool IsFinitePositive(double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value) &&
                value > 0;
        }

        private static bool Fail(
            string message,
            out string reason)
        {
            reason = message;
            return false;
        }

        private static ShadowHostResult Block(
            string reason,
            bool newRevision,
            long revision,
            string signalId,
            string scenarioId,
            string planId,
            string idempotencyKey)
        {
            return new ShadowHostResult(
                ShadowHostState.Blocked,
                reason,
                newRevision,
                revision,
                signalId,
                scenarioId,
                planId,
                idempotencyKey);
        }
    }
}