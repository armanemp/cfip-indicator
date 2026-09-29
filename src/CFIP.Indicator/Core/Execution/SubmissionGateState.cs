using System;

namespace cAlgo
{
    internal sealed class SubmissionGateState
    {
        public int ConsecutiveFailures;
        public DateTime LastFailureUtc = DateTime.MinValue;
        public DateTime NextAllowedUtc = DateTime.MinValue;
        public DateTime CircuitOpenUntilUtc = DateTime.MinValue;
    }
}