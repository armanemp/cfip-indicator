using System;
using System.Collections.Generic;
using CFIP.Contracts;

namespace CFIP.cBot.Shadow
{
    public sealed class ShadowHostCoordinator
    {
        private const int MaxSeenIdempotencyKeys = 128;
        private readonly Dictionary<string, long> _lastAcceptedRevisionByScenario =
            new Dictionary<string, long>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> _lastAcceptedIdempotencyKeyByScenario =
            new Dictionary<string, string>(StringComparer.Ordinal);

        private readonly HashSet<string> _seenIdempotencyKeys =
            new HashSet<string>(StringComparer.Ordinal);
        private readonly Queue<string> _seenOrder =
            new Queue<string>();

        private long _lastAcceptedRevision = -1;
        private string _lastAcceptedIdempotencyKey = "";
        private DateTime _lastBrokerRecheckUtc = DateTime.MinValue;
        private ShadowHostResult _lastResult;

        public long LastAcceptedRevision =>
            _lastAcceptedRevision;

        public string LastAcceptedIdempotencyKey =>
            _lastAcceptedIdempotencyKey;

        public ShadowHostResult Observe(
            SignalEnvelope envelope,
            ShadowBrokerSnapshot broker,
            int contractVersion,
            long providerRevision,
            DateTime nowUtc)
        {
            if (envelope == null)
            {
                return ShadowHostValidator.Validate(
                    null,
                    broker,
                    contractVersion,
                    _lastAcceptedRevision,
                    _lastAcceptedIdempotencyKey,
                    nowUtc);
            }

            ContractIdentity identity =
                envelope.Identity;

            if (identity == null)
            {
                return ShadowHostValidator.Validate(
                    envelope,
                    broker,
                    contractVersion,
                    _lastAcceptedRevision,
                    _lastAcceptedIdempotencyKey,
                    nowUtc);
            }

            if (providerRevision != identity.Revision)
            {
                ShadowHostResult mismatch =
                    new ShadowHostResult(
                        ShadowHostState.Blocked,
                        "PROVIDER REVISION MISMATCH",
                        false,
                        identity.Revision,
                        identity.SignalId,
                        identity.ScenarioId,
                        identity.PlanId,
                        identity.IdempotencyKey);

                _lastResult = mismatch;
                return mismatch;
            }

            string scenarioKey =
                identity.ScenarioId ?? string.Empty;

            long lastScenarioRevision =
                GetLastRevision(scenarioKey);

            string lastScenarioIdempotencyKey =
                GetLastIdempotencyKey(scenarioKey);

            bool sameCurrent =
                identity.Revision == lastScenarioRevision &&
                string.Equals(
                    identity.IdempotencyKey,
                    lastScenarioIdempotencyKey,
                    StringComparison.Ordinal);

            if (sameCurrent)
            {
                if (_lastResult != null &&
                    (_lastResult.State == ShadowHostState.Expired ||
                     _lastResult.State == ShadowHostState.Duplicate ||
                     (_lastResult.State == ShadowHostState.Blocked &&
                      !IsTransientBrokerBlock(_lastResult.Reason))))
                {
                    return _lastResult;
                }

                if ((nowUtc - _lastBrokerRecheckUtc).TotalMilliseconds < 500)
                    return _lastResult ?? ShadowHostValidator.RevalidateBrokerSafety(
                        envelope,
                        broker);

                _lastBrokerRecheckUtc = nowUtc;

                ShadowHostResult rechecked =
                    ShadowHostValidator.RevalidateBrokerSafety(
                        envelope,
                        broker);

                _lastResult = rechecked;

                return rechecked;
            }

            bool alreadySeen =
                !string.IsNullOrWhiteSpace(identity.IdempotencyKey) &&
                _seenIdempotencyKeys.Contains(
                    identity.IdempotencyKey);

            ShadowHostResult result =
                alreadySeen
                    ? new ShadowHostResult(
                        ShadowHostState.Duplicate,
                        "DUPLICATE IDEMPOTENCY KEY",
                        false,
                        identity.Revision,
                        identity.SignalId,
                        identity.ScenarioId,
                        identity.PlanId,
                        identity.IdempotencyKey)
                    : ShadowHostValidator.Validate(
                        envelope,
                        broker,
                        contractVersion,
                        lastScenarioRevision,
                        lastScenarioIdempotencyKey,
                        nowUtc);

            _lastResult = result;

            if (result.State == ShadowHostState.Ready ||
                result.State == ShadowHostState.Observing ||
                result.State == ShadowHostState.Expired ||
                (result.State == ShadowHostState.Blocked &&
                 !IsTransientBrokerBlock(result.Reason)))
            {
                Remember(
                    scenarioKey,
                    identity.Revision,
                    identity.IdempotencyKey);
            }

            return result;
        }

        private void Remember(
            string scenarioKey,
            long revision,
            string idempotencyKey)
        {
            scenarioKey =
                scenarioKey ?? string.Empty;

            if (revision >
                GetLastRevision(scenarioKey))
            {
                _lastAcceptedRevisionByScenario[scenarioKey] =
                    revision;
            }

            _lastAcceptedIdempotencyKeyByScenario[scenarioKey] =
                idempotencyKey ?? "";

            if (revision > _lastAcceptedRevision)
                _lastAcceptedRevision = revision;

            _lastAcceptedIdempotencyKey =
                idempotencyKey ?? "";

            if (string.IsNullOrWhiteSpace(idempotencyKey) ||
                !_seenIdempotencyKeys.Add(idempotencyKey))
                return;

            _seenOrder.Enqueue(idempotencyKey);

            while (_seenOrder.Count > MaxSeenIdempotencyKeys)
            {
                string oldest =
                    _seenOrder.Dequeue();

                _seenIdempotencyKeys.Remove(oldest);
            }
        }

        private long GetLastRevision(
            string scenarioKey)
        {
            return _lastAcceptedRevisionByScenario.TryGetValue(
                scenarioKey ?? string.Empty,
                out long revision)
                ? revision
                : -1;
        }

        private string GetLastIdempotencyKey(
            string scenarioKey)
        {
            return _lastAcceptedIdempotencyKeyByScenario.TryGetValue(
                scenarioKey ?? string.Empty,
                out string key)
                ? key ?? string.Empty
                : string.Empty;
        }

        private static bool IsTransientBrokerBlock(
            string reason)
        {
            return
                string.Equals(
                    reason,
                    "BROKER STATE UNAVAILABLE",
                    StringComparison.Ordinal) ||
                string.Equals(
                    reason,
                    "TRADING PERMISSION BLOCKED",
                    StringComparison.Ordinal) ||
                string.Equals(
                    reason,
                    "SINGLE-PLAN CAPACITY BLOCKED",
                    StringComparison.Ordinal) ||
                string.Equals(
                    reason,
                    "INVALID LIVE QUOTE",
                    StringComparison.Ordinal) ||
                string.Equals(
                    reason,
                    "INVALID PIP SIZE",
                    StringComparison.Ordinal);
        }
    }
}
