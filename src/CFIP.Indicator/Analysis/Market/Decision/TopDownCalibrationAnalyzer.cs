using cAlgo.API;

using System;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private TopDownCalibrationSnapshot EvaluateTopDownCalibration(
            Decision decision)
        {
            int strongThreshold =
                Math.Max(
                    MinimumTimeframeAgreement,
                    SmartMinimumTimeframeAgreement);

            return TopDownCalibrationRule.Evaluate(
                new[]
                {
                    _h1Frame == null ? 0 : _h1Frame.Direction,
                    _h4Frame == null ? 0 : _h4Frame.Direction,
                    _d1Frame == null ? 0 : _d1Frame.Direction,
                    SmartWeeklyContext &&
                    _w1Frame != null
                        ? _w1Frame.Direction
                        : 0
                },
                new[]
                {
                    _h1Frame == null ? 0 : _h1Frame.Quality,
                    _h4Frame == null ? 0 : _h4Frame.Quality,
                    _d1Frame == null ? 0 : _d1Frame.Quality,
                    SmartWeeklyContext &&
                    _w1Frame != null
                        ? _w1Frame.Quality
                        : 0
                },
                new[]
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
                    _m30Frame == null ? 0 : _m30Frame.Direction,
                    _m15Frame == null ? 0 : _m15Frame.Direction
                },
                new[]
                {
                    _m30Frame == null ? 0 : _m30Frame.Quality,
                    _m15Frame == null ? 0 : _m15Frame.Quality
                },
                new[]
                {
                    Math.Max(0, M30Weight),
                    Math.Max(0, M15Weight)
                },
                _m5Frame == null ? 0 : _m5Frame.Direction,
                _m5Frame == null ? 0 : _m5Frame.Quality,
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