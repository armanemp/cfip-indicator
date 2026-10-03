using System;

namespace cAlgo
{
    public partial class CFIPIndicator
    {
        private void ApplyOpportunityIntelligenceRanking(
            TradeOpportunityCandidate candidate,
            OpportunityLane lane,
            int direction,
            int quality)
        {
            if (candidate == null ||
                direction == 0)
                return;

            EmpiricalCalibrationSnapshot historical =
                GetEmpiricalCalibrationSnapshot(
                    direction,
                    lane,
                    _decision == null
                        ? "UNKNOWN"
                        : _decision.Regime,
                    Math.Max(
                        0,
                        Math.Min(
                            100,
                            quality)));

            int forecastAlignment = 0;

            if (_prediction != null &&
                _prediction.Direction != 0)
            {
                int forecastConfidence =
                    Math.Max(
                        0,
                        Math.Min(
                            100,
                            _prediction.Confidence));

                forecastAlignment =
                    _prediction.Direction == direction
                        ? forecastConfidence
                        : -forecastConfidence;
            }

            int historicalSupport = 0;

            if (historical.Available &&
                historical.Samples >= 3)
            {
                double winEdge =
                    (historical.ObservedWinRate - 0.50) * 100.0;

                double realizedREdge =
                    Math.Max(
                        -3.0,
                        Math.Min(
                            3.0,
                            historical.AverageRealizedR)) * 6.0;

                historicalSupport =
                    (int)Math.Round(
                        winEdge + realizedREdge);

                historicalSupport =
                    Math.Max(
                        -20,
                        Math.Min(
                            20,
                            historicalSupport));
            }

            candidate.ForecastAlignmentScore =
                forecastAlignment;
            candidate.HistoricalSupportScore =
                historicalSupport;
            candidate.HistoricalCalibrationSamples =
                Math.Max(
                    0,
                    historical.Samples);
            candidate.HistoricalObservedWinRate =
                historical.Available
                    ? historical.ObservedWinRate
                    : 0;
            candidate.HistoricalAverageRealizedR =
                historical.Available
                    ? historical.AverageRealizedR
                    : 0;

            candidate.ExecutionPriorityScore =
                TradeOpportunityQualityRule.CalculateExecutionPriorityScore(
                    candidate);
        }
    }
}
