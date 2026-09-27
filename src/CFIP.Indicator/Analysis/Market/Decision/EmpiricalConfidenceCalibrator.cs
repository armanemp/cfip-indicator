using System;

namespace cAlgo
{
    internal sealed class EmpiricalConfidenceCalibrator
    {
        public int CalculateAdjustment(
            bool calibrationEnabled,
            bool telemetryEnabled,
            int totalSamples,
            int directionalSamples,
            int directionalWins,
            int minimumSamples,
            int minimumDirectionalSamples,
            int maximumAdjustment)
        {
            if (!calibrationEnabled ||
                !telemetryEnabled ||
                totalSamples < Math.Max(1, minimumSamples) ||
                directionalSamples < minimumDirectionalSamples)
                return 0;

            double rate =
                directionalSamples <= 0
                    ? 0.5
                    : (double)directionalWins / directionalSamples;

            return NumericGuards.ClampInt(
                (int)Math.Round(
                    (rate - 0.5) *
                    2.0 *
                    maximumAdjustment),
                -maximumAdjustment,
                maximumAdjustment);
        }
    }
}