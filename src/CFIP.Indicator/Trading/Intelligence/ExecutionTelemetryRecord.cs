using System;

namespace cAlgo
{
    internal sealed class ExecutionTelemetryRecord
    {
        public DateTime Utc;
        public int M5;
        public string Path;
        public string State;
        public string Reason;
    }
}
