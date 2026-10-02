using System;
using System.Collections.Generic;
using CFIP.Contracts;

namespace CFIP.cBot.Shadow
{
    public sealed class ShadowHostCoordinator
    {
        private const int MaxSeenIdempotencyKeys = 128;
        private readonly HashSet<string> _seenIdempotencyKeys =
            new HashSet<string>(StringComparer.Ordinal);
        private readonly Queue<string> _seenOrder =
            new Queue<string>();

        private long _lastAcceptedRevision = -1;
        private string _lastAcceptedIdempotencyKey = "";
        private DateTime _lastBrokerRecheckUtc = DateTime.MinValue;

        public long LastAcceptedRevision =>
            _lastAcceptedRevision;

        public string LastAcceptedIdempotencyKey =>
            _lastAcceptedIdempotencyKey;

        public ShadowHostResult Observe(
            SignalEnvelope envelope,
            ShadowBrokerSnapshot broker,
            int contractVersion,
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

            bool sameCurrent =
                identity.Revision == _lastAcceptedRevision &&
                string.Equals(
                    identity.IdempotencyKey,
                    _lastAcceptedIdempotencyKey,
                    StringComparison.Ordinal);

            if (sameCurrent)
            {
                if ((nowUtc - _lastBrokerRecheckUtc).TotalMilliseconds < 500)
                {
                    return new ShadowHostResult(
                        ShadowHostState.Ready,
                        "CURRENT SHADOW STATE",
                        false,
                        identity.Revision,
                        identity.SignalId,
                        identity.ScenarioId,
                        identity.PlanId,
                        identity.IdempotencyKey);
                }

                _lastBrokerRecheckUtc = nowUtc;

                return ShadowHostValidator.RevalidateBrokerSafety(
                    envelope,
                    broker);
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
                        _lastAcceptedRevision,
                        _lastAcceptedIdempotencyKey,
                        nowUtc);

            if (result.State == ShadowHostState.Ready ||
                result.State == ShadowHostState.Observing ||
                result.State == ShadowHostState.Expired ||
                (result.State == ShadowHostState.Blocked &&
                 !IsTransientBrokerBlock(result.Reason)))
            {
                Remember(
                    identity.Revision,
                    identity.IdempotencyKey);
            }

            return result;
        }

        private void Remember(
            long revision,
            string idempotencyKey)
        {
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
