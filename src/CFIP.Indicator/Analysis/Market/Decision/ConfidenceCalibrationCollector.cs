using cAlgo.API;

using System;
using System.Linq;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private int ConfidenceCalibrationAdjustment(int direction)
        {
            if (!UseEmpiricalCalibration ||
                !EnableConfidenceCalibration ||
                !EnableOutcomeTelemetry ||
                direction == 0 ||
                !_directionSamples.ContainsKey(direction))
                return 0;

            int totalSamples = _directionSamples.Values.Sum();

            return new EmpiricalConfidenceCalibrator()
                .CalculateAdjustment(
                    true,
                    true,
                    totalSamples,
                    _directionSamples[direction],
                    _directionWins.ContainsKey(direction)
                        ? _directionWins[direction]
                        : 0,
                    Math.Max(1, CalibrationMinimumSamples),
                    CalibrationDirectionalMinimumSamples,
                    CalibrationMaxConfidenceAdjustment);
        }
    }
}