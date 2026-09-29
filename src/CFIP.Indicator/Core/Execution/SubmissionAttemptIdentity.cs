using System;

namespace cAlgo
{
    internal readonly struct SubmissionAttemptIdentity
    {
        public SubmissionAttemptIdentity(
            string signalKey,
            string attemptKey,
            ExecutionSubmissionPath path,
            string scenarioId = "")
        {
            SignalKey = signalKey ?? string.Empty;
            AttemptKey = attemptKey ?? string.Empty;
            Path = path;
            ScenarioId =
                string.IsNullOrWhiteSpace(scenarioId)
                    ? "UNSCOPED"
                    : scenarioId.Trim().Replace("|", "/");
        }

        public string SignalKey { get; }
        public string AttemptKey { get; }
        public ExecutionSubmissionPath Path { get; }
        public string ScenarioId { get; }

        public string CanonicalKey
        {
            get
            {
                return SignalKey + "|" +
                       AttemptKey + "|" +
                       ((int)Path).ToString() + "|" +
                       ScenarioId;
            }
        }
    }
}