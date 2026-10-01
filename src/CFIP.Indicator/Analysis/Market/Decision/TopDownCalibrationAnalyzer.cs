using cAlgo.API;

using System;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private TopDownCalibrationSnapshot EvaluateTopDownCalibration(
            Decision decision)
        {
            if (_marketStateSnapshot == null)
                throw new InvalidOperationException(
                    "Top-down calibration market-state snapshot is missing.");

            int strongThreshold =
                Math.Max(
                    MinimumTimeframeAgreement,
                    SmartMinimumTimeframeAgreement);

            return TopDownCalibrationRule.Evaluate(
                new[]
                {
                    _marketStateSnapshot.H1.Direction,
                    _marketStateSnapshot.H4.Direction,
                    _marketStateSnapshot.D1.Direction,
                    SmartWeeklyContext
                        ? _marketStateSnapshot.W1.Direction
                        : 0
                },
                new[]
                {
                    _marketStateSnapshot.H1.Quality,
                    _marketStateSnapshot.H4.Quality,
                    _marketStateSnapshot.D1.Quality,
                    SmartWeeklyContext
                        ? _marketStateSnapshot.W1.Quality
                        : 0
                },
                new double[]
                {
                    Math.Max(0, H1Weight),
                    Math.Max(0, H4Weight),
                    Math.Max(0, D1Weight),
                    SmartWeeklyContext
                        ? Math.Max(0, W1Weight)
                        : 0
                },
                new[]
                {
                    _marketStateSnapshot.M30.Direction,
                    _marketStateSnapshot.M15.Direction
                },
                new[]
                {
                    _marketStateSnapshot.M30.Quality,
                    _marketStateSnapshot.M15.Quality
                },
                new double[]
                {
                    Math.Max(0, M30Weight),
                    Math.Max(0, M15Weight)
                },
                _marketStateSnapshot.M5.Direction,
                _marketStateSnapshot.M5.Quality,
                strongThreshold,
                decision == null ? 0 : decision.Direction);
        }

        private DecisionFilterResult EvaluateDecisionTopDownGate(
            Decision decision)
        {
            if (!RequireHigherTfAgreement ||
                decision == null ||
                decision.Direction == 0)
                return new DecisionFilterResult(
                    true,
                    string.Empty);

            if (!decision.TopDownEligible ||
                !string.Equals(
                    decision.TopDownStage,
                    "ENTRY CALIBRATED",
                    StringComparison.OrdinalIgnoreCase))
            {
                if (decision.TacticalOpportunityAllowed)
                    return new DecisionFilterResult(
                        true,
                        string.Empty);

                string reason =
                    string.Equals(
                        decision.TopDownStage,
                        "MIDFRAME CONFLICT",
                        StringComparison.OrdinalIgnoreCase)
                        ? "TOP-DOWN MIDFRAME"
                        : string.Equals(
                            decision.TopDownStage,
                            "ENTRY CONFLICT",
                            StringComparison.OrdinalIgnoreCase)
                            ? "TOP-DOWN ENTRY"
                            : "TOP-DOWN HTF";

                return new DecisionFilterResult(
                    false,
                    reason);
            }

            return new DecisionFilterResult(
                true,
                string.Empty);
        }
    }
}