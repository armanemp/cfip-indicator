namespace cAlgo
{
    /// <summary>
    /// Canonical timeframe roles for the active execution architecture.
    /// M15 is the execution clock. M5/M1 are defensive tuning layers.
    /// H1+ provide higher-timeframe context and extended reward-path evidence.
    /// </summary>
    internal static class ExecutionTimeframePolicy
    {
        internal const string PrimaryExecution = "M15";
        internal const string LowerDefensiveM5 = "M5";
        internal const string LowerDefensiveM1 = "M1";

        public static bool IsPrimaryExecution(string timeframe)
        {
            return string.Equals(
                timeframe,
                PrimaryExecution,
                System.StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsLowerDefensive(string timeframe)
        {
            return string.Equals(
                    timeframe,
                    LowerDefensiveM5,
                    System.StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    timeframe,
                    LowerDefensiveM1,
                    System.StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsHigherContext(string timeframe)
        {
            if (string.IsNullOrWhiteSpace(timeframe))
                return false;

            string normalized = timeframe.Trim().ToUpperInvariant();
            return normalized == "H1" ||
                   normalized == "H4" ||
                   normalized == "D1" ||
                   normalized == "W1";
        }
    }
}