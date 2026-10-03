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

        private readonly Dictionary<string, DateTime> _lastBrokerRecheckUtcByScenario =
            new Dictionary<string, DateTime>(StringComparer.Ordinal);
        private readonly Dictionary<string, ShadowHostResult> _lastResultByScenario =
            new Dictionary<string, ShadowHostResult>(StringComparer.Ordinal);

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

                SetScenarioResult(
                    scenarioKey,
                    mismatch);
                return mismatch;
            }

            string scenarioKey =
                identity.ScenarioId ?? string.Empty;

            long lastScenarioRevision =
                GetLastRevision(scenarioKey);

            string lastScenarioIdempotencyKey =
                GetLastIdempotencyKey(scenarioKey);

            ShadowHostResult scenarioResult =
                GetLastResult(scenarioKey);

            DateTime scenarioRecheckUtc =
                GetLastBrokerRecheck(scenarioKey);

            bool sameCurrent =
                identity.Revision == lastScenarioRevision &&
                string.Equals(
                    identity.IdempotencyKey,
                    lastScenarioIdempotencyKey,
                    StringComparison.Ordinal);

            if (sameCurrent)
            {
                if (scenarioResult != null &&
                    (scenarioResult.State == ShadowHostState.Expired ||
                     scenarioResult.State == ShadowHostState.Duplicate ||
                     (scenarioResult.State == ShadowHostState.Blocked &&
                      !IsTransientBrokerBlock(scenarioResult.Reason))))
                {
                    return scenarioResult;
                }

                if ((nowUtc - scenarioRecheckUtc).TotalMilliseconds < 500)
                    return scenarioResult ??
                        ShadowHostValidator.RevalidateBrokerSafety(
                            envelope,
                            broker);

                _lastBrokerRecheckUtc = nowUtc;

                SetScenarioBrokerRecheck(
                    scenarioKey,
                    nowUtc);

                ShadowHostResult rechecked =
                    ShadowHostValidator.RevalidateBrokerSafety(
                        envelope,
                        broker);

                SetScenarioResult(
                    scenarioKey,
                    rechecked);

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

            SetScenarioResult(
                scenarioKey,
                result);

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

        private ShadowHostResult GetLastResult(
            string scenarioKey)
        {
            return _lastResultByScenario.TryGetValue(
                scenarioKey ?? string.Empty,
                out ShadowHostResult result)
                ? result
                : null;
        }

        private DateTime GetLastBrokerRecheck(
            string scenarioKey)
        {
            return _lastBrokerRecheckUtcByScenario.TryGetValue(
                scenarioKey ?? string.Empty,
                out DateTime value)
                ? value
                : DateTime.MinValue;
        }

        private void SetScenarioResult(
            string scenarioKey,
            ShadowHostResult result)
        {
            string key =
                scenarioKey ?? string.Empty;

            if (result != null)
                _lastResultByScenario[key] =
                    result;
            else
                _lastResultByScenario.Remove(key);

            _lastResult = result;
        }

        private void SetScenarioBrokerRecheck(
            string scenarioKey,
            DateTime nowUtc)
        {
            _lastBrokerRecheckUtcByScenario[
                scenarioKey ?? string.Empty] =
                nowUtc;
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
