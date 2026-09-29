// ============================================================================
// CFIP Indicator — OutcomeTelemetryEngine.cs
// ============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void RegisterOutcome(
                            int direction,
                            bool win)
                        {
                            if (!EnableOutcomeTelemetry)
                                return;

                            if (!_directionSamples.ContainsKey(
                                    direction))
                                _directionSamples[direction] = 0;

                            if (!_directionWins.ContainsKey(
                                    direction))
                                _directionWins[direction] = 0;

                            _directionSamples[direction]++;

                            if (win)
                                _directionWins[direction]++;
                        }

        private void RegisterCalibratedOutcome(
                            Plan plan,
                            bool win)
                        {
                            if (!EnableOutcomeTelemetry ||
                                plan == null ||
                                !plan.CalibrationEligible ||
                                (plan.CalibrationDirection != 1 &&
                                 plan.CalibrationDirection != -1))
                                return;

                            ConfidenceCalibrationKey key =
                                new ConfidenceCalibrationKey(
                                    plan.CalibrationDirection,
                                    plan.CalibrationLane,
                                    plan.CalibrationRegime,
                                    EmpiricalConfidenceCalibrator.ConfidenceBucket(
                                        plan.CalibrationConfidence));

                            if (!_calibrationSamples.ContainsKey(key))
                                _calibrationSamples[key] = 0;

                            if (!_calibrationWins.ContainsKey(key))
                                _calibrationWins[key] = 0;

                            _calibrationSamples[key]++;

                            if (win)
                                _calibrationWins[key]++;
                        }
        
        private string CalibrationText()
                        {
                            int total =
                                _wins +
                                _losses;
                
                            return
                                total <= 0
                                    ? "W0/L0"
                                    : (100.0 *
                                       _wins /
                                       total)
                                      .ToString("F0") +
                                      "%";
                        }
        
        private void MonitorOutcome(
                            int closedM5)
                        {
                            if (!EnableOutcomeTelemetry ||
                                _plan == null ||
                                !_plan.IsLivePosition ||
                                OutcomeMaximumM5Bars <= 0)
                                return;
                
                            if (_outcomeTelemetryTimedOut ||
                                closedM5 -
                                _plan.CreatedM5 <
                                OutcomeMaximumM5Bars)
                                return;
                
                            // Telemetry timeout is an observation boundary, not a position
                            // lifecycle boundary. Keep broker ownership and live protection
                            // active until the position is actually closed.
                            _outcomeTelemetryTimedOut = true;
                
                            SendUnifiedAlert(
                                "OUTCOME-TIMEOUT|" +
                                _plan.PositionId,
                                "CFIP OUTCOME WINDOW ELAPSED | POSITION #" +
                                _plan.PositionId +
                                " remains under live management",
                                _plan.Direction,
                                false);
                        }}
}
