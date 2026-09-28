using System;

namespace cAlgo
{
    internal sealed class SubmissionGate
    {
        private const int BaseBackoffSeconds = 1;
        private const int MaximumBackoffSeconds = 30;
        private const int CircuitFailureThreshold = 4;
        private const int CircuitCooldownSeconds = 60;

        private DateTime _nextAllowedUtc = DateTime.MinValue;
        private DateTime _circuitOpenUntilUtc = DateTime.MinValue;
        private int _consecutiveFailures;
        private string _lastKey = string.Empty;

        public bool TryAcquire(
            DateTime nowUtc,
            string key,
            out string reason)
        {
            reason = string.Empty;

            if (nowUtc < _circuitOpenUntilUtc)
            {
                reason =
                    "SUBMISSION CIRCUIT BREAKER";
                return false;
            }

            if (nowUtc < _nextAllowedUtc)
            {
                reason =
                    "SUBMISSION BACKOFF";
                return false;
            }

            if (!string.Equals(
                    _lastKey,
                    key,
                    StringComparison.Ordinal))
            {
                _lastKey =
                    key ?? string.Empty;
            }

            _nextAllowedUtc =
                nowUtc.AddMilliseconds(500);

            return true;
        }

        public void Record(
            DateTime nowUtc,
            bool success)
        {
            if (success)
            {
                _consecutiveFailures = 0;
                _nextAllowedUtc =
                    DateTime.MinValue;
                _circuitOpenUntilUtc =
                    DateTime.MinValue;
                return;
            }

            _consecutiveFailures++;

            int exponent =
                Math.Min(
                    5,
                    _consecutiveFailures - 1);

            int delay =
                BaseBackoffSeconds *
                (1 << exponent);

            delay =
                Math.Min(
                    MaximumBackoffSeconds,
                    delay);

            _nextAllowedUtc =
                nowUtc.AddSeconds(
                    delay);

            if (_consecutiveFailures >=
                CircuitFailureThreshold)
            {
                _circuitOpenUntilUtc =
                    nowUtc.AddSeconds(
                        CircuitCooldownSeconds);
            }
        }
    }
}
