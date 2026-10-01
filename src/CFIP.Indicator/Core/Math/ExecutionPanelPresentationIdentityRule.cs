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
                NormalizePanelIdentityField(autoTradingState),
                NormalizePanelIdentityField(autoTradingReason),
                NormalizePanelIdentityField(executionTelemetryPath),
                NormalizePanelIdentityField(executionTelemetryState),
                NormalizePanelIdentityField(activeExecutionScenarioId),
                marketSuitabilityScore.ToString(System.Globalization.CultureInfo.InvariantCulture),
                NormalizePanelIdentityField(marketSuitabilityState),
                NormalizePanelIdentityField(marketSuitabilityReason),
                NormalizePanelIdentityField(breakEvenDiagnostic));
        }

        private static string NormalizePanelIdentityField(string value)
        {
            return value ?? string.Empty;
        }
    }
}
