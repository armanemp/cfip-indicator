namespace cAlgo
{
    internal static class ExecutionPanelPresentationIdentityRule
    {
        public static string Compose(
            string autoTradingState,
            string autoTradingReason,
            string executionTelemetryPath,
            string executionTelemetryState,
            string activeExecutionScenarioId,
            int marketSuitabilityScore,
            string marketSuitabilityState,
            string marketSuitabilityReason,
            string breakEvenDiagnostic)
        {
            return string.Join(
                "|",
                Normalize(autoTradingState),
                Normalize(autoTradingReason),
                Normalize(executionTelemetryPath),
                Normalize(executionTelemetryState),
                Normalize(activeExecutionScenarioId),
                marketSuitabilityScore.ToString(System.Globalization.CultureInfo.InvariantCulture),
                Normalize(marketSuitabilityState),
                Normalize(marketSuitabilityReason),
                Normalize(breakEvenDiagnostic));
        }

        private static string Normalize(string value)
        {
            return value ?? string.Empty;
        }
    }
}
