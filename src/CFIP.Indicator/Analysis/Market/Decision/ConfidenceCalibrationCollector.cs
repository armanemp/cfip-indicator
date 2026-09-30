using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private int ConfidenceCalibrationAdjustment(
            int direction)
        {
            // Phase 9.3 closes the pre-context direction-only adjustment path.
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

            EmpiricalConfidenceCalibrator calibrator =
                new EmpiricalConfidenceCalibrator();

            int minimumDirectionalSamples =
                Math.Max(
                    2,
                    CalibrationDirectionalMinimumSamples);

            if (_outcomeHistory != null &&
                _outcomeHistory.Count >=
                    minimumDirectionalSamples)
            {
                EmpiricalCalibrationSnapshot recent =
                    calibrator.CalculateRecentContextual(
                        true,
                        true,
                        direction,
                        lane,
                        regime,
                        confidence,
                        _outcomeHistory,
                        128,
                        Math.Max(
                            1,
                            CalibrationMinimumSamples),
                        minimumDirectionalSamples,
                        CalibrationMaxConfidenceAdjustment);

                if (recent.Available)
                    return recent;
            }

            // When the recent bounded window is too sparse, use the
            // persistent archive aggregate as the long-term evidence baseline.
            // This keeps old 90-day files useful without loading every outcome
            // into the hot-path recent list.
            if (_archiveLearningOutcomeCount >=
                Math.Max(
                    1,
                    CalibrationMinimumSamples))
            {
                EmpiricalCalibrationSnapshot archived =
                    calibrator.CalculateContextual(
                        true,
                        true,
                        direction,
                        lane,
                        regime,
                        confidence,
                        _archiveCalibrationSamples,
                        _archiveCalibrationWins,
                        Math.Max(
                            1,
                            CalibrationMinimumSamples),
                        minimumDirectionalSamples,
                        CalibrationMaxConfidenceAdjustment);

                if (archived.Available)
                    return new EmpiricalCalibrationSnapshot(
                        true,
                        archived.Adjustment,
                        archived.Samples,
                        archived.Wins,
                        archived.ConfidenceBucket,
                        archived.ObservedWinRate,
                        archived.Source + "-ARCHIVE");
            }

            return calibrator.CalculateContextual(
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
                minimumDirectionalSamples,
                CalibrationMaxConfidenceAdjustment);
        }

        private void BindPlanCalibrationContext(
            Plan plan,
            Decision decision,
            int direction,
            OpportunityLane lane)
        {
            if (plan == null ||
                decision == null ||
                !EnableOutcomeTelemetry ||
                !UseEmpiricalCalibration ||
                !EnableConfidenceCalibration ||
                (direction != 1 && direction != -1))
                return;

            plan.CalibrationEligible = true;
            plan.CalibrationDirection = direction;
            plan.CalibrationLane = lane;
            plan.CalibrationRegime =
                string.IsNullOrWhiteSpace(decision.Regime)
                    ? "UNKNOWN"
                    : decision.Regime.Trim().ToUpperInvariant();
            plan.CalibrationConfidence =
                NumericGuards.ClampInt(
                    decision.BaseConfidence,
                    0,
                    100);
            plan.CalibrationBucket =
                EmpiricalConfidenceCalibrator.ConfidenceBucket(
                    plan.CalibrationConfidence);
        }

        private void CopyPlanCalibrationContext(
            Plan source,
            Plan target)
        {
            if (source == null ||
                target == null ||
                !source.CalibrationEligible)
                return;

            target.CalibrationEligible =
                source.CalibrationEligible;
            target.CalibrationDirection =
                source.CalibrationDirection;
            target.CalibrationLane =
                source.CalibrationLane;
            target.CalibrationRegime =
                source.CalibrationRegime;
            target.CalibrationConfidence =
                source.CalibrationConfidence;
            target.CalibrationBucket =
                source.CalibrationBucket;
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
            decision.EmpiricalCalibrationAverageRealizedR =
                snapshot.AverageRealizedR;
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