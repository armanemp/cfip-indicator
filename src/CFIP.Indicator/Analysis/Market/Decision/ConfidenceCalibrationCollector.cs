using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private int ConfidenceCalibrationAdjustment(
            int direction)
        {
            // Phase 9.3 closes the legacy direction-only adjustment path.
            // Calibration is applied only after lane/regime/confidence context
            // is known, so it cannot contaminate the pre-context decision score.
            return 0;
        }

        private EmpiricalCalibrationSnapshot GetEmpiricalCalibrationSnapshot(
            int direction,
            OpportunityLane lane,
            string regime,
            int confidence)
        {
            if (!UseEmpiricalCalibration ||
                !EnableConfidenceCalibration ||
                !EnableOutcomeTelemetry ||
                direction == 0)
                return EmpiricalCalibrationSnapshot.None(
                    EmpiricalConfidenceCalibrator.ConfidenceBucket(
                        confidence));

            return new EmpiricalConfidenceCalibrator()
                .CalculateContextual(
                    true,
                    true,
                    direction,
                    lane,
                    regime,
                    confidence,
                    _calibrationSamples,
                    _calibrationWins,
                    Math.Max(
                        1,
                        CalibrationMinimumSamples),
                    Math.Max(
                        2,
                        CalibrationDirectionalMinimumSamples),
                    CalibrationMaxConfidenceAdjustment);
        }

        private void ApplyEmpiricalCalibration(
            Decision decision,
            OpportunityLane tacticalLane)
        {
            if (decision == null)
                return;

            decision.BaseConfidence =
                decision.Confidence;

            OpportunityLane lane =
                decision.TopDownEligible &&
                string.Equals(
                    decision.TopDownStage,
                    "ENTRY CALIBRATED",
                    StringComparison.OrdinalIgnoreCase)
                    ? OpportunityLane.Strategic
                    : decision.TacticalOpportunityAllowed
                        ? decision.TacticalOpportunityLane
                        : tacticalLane;

            EmpiricalCalibrationSnapshot snapshot =
                GetEmpiricalCalibrationSnapshot(
                    decision.Direction,
                    lane,
                    decision.Regime,
                    decision.BaseConfidence);

            decision.EmpiricalCalibrationBucket =
                snapshot.ConfidenceBucket;
            decision.EmpiricalCalibrationSamples =
                snapshot.Samples;
            decision.EmpiricalCalibrationWins =
                snapshot.Wins;
            decision.EmpiricalCalibrationObservedWinRate =
                snapshot.ObservedWinRate;
            decision.EmpiricalCalibrationAdjustment =
                snapshot.Adjustment;
            decision.EmpiricalCalibrationSource =
                snapshot.Source;

            decision.Confidence =
                NumericGuards.ClampInt(
                    decision.BaseConfidence +
                    (snapshot.Available
                        ? snapshot.Adjustment
                        : 0),
                    0,
                    100);

            decision.CalibratedConfidence =
                decision.Confidence;
        }
    }
}