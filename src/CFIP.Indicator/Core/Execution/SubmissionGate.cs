using System;
using System.Collections.Generic;

namespace cAlgo
{
    internal sealed class SubmissionGate
    {
        private const int BaseBackoffSeconds = 1;
        private const int MaximumBackoffSeconds = 30;
        private const int CircuitFailureThreshold = 4;
        private const int CircuitCooldownSeconds = 60;
        private const int MaximumRetainedStates = 128;

        private readonly Dictionary<SubmissionAttemptIdentity, SubmissionGateState> _states =
            new Dictionary<SubmissionAttemptIdentity, SubmissionGateState>();

        public bool TryAcquire(
            SubmissionAttemptIdentity identity,
            DateTime nowUtc,
            out string reason)
        {
            reason = string.Empty;

            if (identity.SignalKey.Length == 0 ||
                identity.AttemptKey.Length == 0)
            {
                reason = "INVALID SUBMISSION IDENTITY";
                return false;
            }

            SubmissionGateState state;

            if (!_states.TryGetValue(identity, out state))
                return true;

            if (nowUtc < state.CircuitOpenUntilUtc)
            {
                reason = "SUBMISSION CIRCUIT BREAKER";
                return false;
            }

            if (nowUtc < state.NextAllowedUtc)
            {
                reason = "SUBMISSION BACKOFF";
                return false;
            }

            return true;
        }

        public void Record(
            SubmissionAttemptIdentity identity,
            DateTime nowUtc,
            bool success)
        {
            if (identity.SignalKey.Length == 0 ||
                identity.AttemptKey.Length == 0)
                return;

            if (success)
            {
                _states.Remove(identity);
                return;
            }

            SubmissionGateState state;

            if (!_states.TryGetValue(identity, out state))
            {
                state = new SubmissionGateState();
                _states[identity] = state;
            }

            state.ConsecutiveFailures =
                Math.Max(1, state.ConsecutiveFailures + 1);
            state.LastFailureUtc = nowUtc;

            int exponent = Math.Min(
                5,
                state.ConsecutiveFailures - 1);

            int delaySeconds =
                Math.Min(
                    MaximumBackoffSeconds,
                    BaseBackoffSeconds * (1 << exponent));

            state.NextAllowedUtc =
                nowUtc.AddSeconds(delaySeconds);

            if (state.ConsecutiveFailures >=
                CircuitFailureThreshold)
            {
                state.CircuitOpenUntilUtc =
                    nowUtc.AddSeconds(CircuitCooldownSeconds);
            }

            PruneInactiveStates(nowUtc);
        }

        private void PruneInactiveStates(DateTime nowUtc)
        {
            if (_states.Count <= MaximumRetainedStates)
                return;

            List<SubmissionAttemptIdentity> removable =
                new List<SubmissionAttemptIdentity>();

            foreach (KeyValuePair<SubmissionAttemptIdentity, SubmissionGateState> entry in _states)
            {
                SubmissionGateState state = entry.Value;

                if (nowUtc >= state.NextAllowedUtc &&
                    nowUtc >= state.CircuitOpenUntilUtc)
                {
                    removable.Add(entry.Key);
                }
            }

            foreach (SubmissionAttemptIdentity identity in removable)
            {
                _states.Remove(identity);

                if (_states.Count <= MaximumRetainedStates)
                    break;
            }
        }
    }
}