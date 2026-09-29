using System;

namespace cAlgo
{
    internal readonly struct SubmissionAttemptIdentity
    {
        public SubmissionAttemptIdentity(
            string signalKey,
            string attemptKey,
            ExecutionSubmissionPath path)
        {
            SignalKey = signalKey ?? string.Empty;
            AttemptKey = attemptKey ?? string.Empty;
            Path = path;
        }

        public string SignalKey { get; }
        public string AttemptKey { get; }
        public ExecutionSubmissionPath Path { get; }

        public string CanonicalKey
        {
            get
            {
                return SignalKey + "|" +
                       AttemptKey + "|" +
                       ((int)Path).ToString();
            }
        }
    }
}