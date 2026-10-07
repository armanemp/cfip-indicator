using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private const int RangeLookbackM5Internal = 36;
        private const int RangeMinimumTouchesInternal = 2;
        private const double RangeMinWidthAtrInternal = 0.35;
        private const double RangeMaxWidthAtrInternal = 3.0;
        private const double RangeSweepAtrInternal = 0.05;
        private const double RangeBreakoutAtrInternal = 0.10;
        private const double RangeRetestAtrInternal = 0.12;
        private const int RangeSignalMinimumScoreInternal = 72;

        private void UpdateRangeManipulation(
            int closedM5)
        {
            if (_m5Bars == null ||
                closedM5 < 30)
            {
                _rangeManipulation =
                    RangeManipulationSnapshot.Empty;
                return;
            }

            _rangeManipulation =
                _rangeManipulationAnalyzer.Evaluate(
                    _m5Bars,
                    closedM5,
                    RangeLookbackM5Internal,
                    RangeMinimumTouchesInternal,
                    RangeMaxWidthAtrInternal,
                    RangeMinWidthAtrInternal,
                    RangeSweepAtrInternal,
                    Symbol.PipSize,
                    RangeBreakoutAtrInternal,
                    RangeRetestAtrInternal,
                    RangeSignalMinimumScoreInternal);
        }

        private void ApplyRangeIntelligenceToDecision(
            Decision decision,
            int closedM5)
        {
            if (decision == null ||
                _rangeManipulation == null ||
                !_rangeManipulation.IsRange)
                return;

            bool isRangeRegime =
                string.Equals(
                    decision.Regime,
                    "RANGE",
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    decision.Regime,
                    "COMPRESSION",
                    StringComparison.OrdinalIgnoreCase);

            if (!isRangeRegime)
                return;

            int rangeDirection =
                _rangeManipulation.Direction;

            bool strong =
                _rangeManipulation.Score >=
                RangeSignalMinimumScoreInternal;

            bool confirmedBreakout =
                _rangeManipulation.IsConfirmedBreakout &&
                rangeDirection != 0;

            bool recentSweep =
                _rangeManipulation.IsManipulation &&
                _rangeManipulation.SweepIndex >= 0 &&
                closedM5 -
                _rangeManipulation.SweepIndex <= 12;

            if (!strong &&
                !confirmedBreakout)
                return;

            // A confirmed range breakout is allowed to become the tactical
            // direction. A mere sweep is only a directional setup until the
            // decision engine confirms it.
            if (confirmedBreakout)
            {
                decision.Direction =
                    rangeDirection;

                decision.Confidence =
                    Math.Max(
                        decision.Confidence,
                        Math.Min(
                            92,
                            _rangeManipulation.Score));

                decision.SmartQuality =
                    Math.Max(
                        decision.SmartQuality,
                        Math.Min(
                            92,
                            _rangeManipulation.Score));

                decision.Edge =
                    Math.Max(
                        decision.Edge,
                        18);

                decision.Reason =
                    AppendReason(
                        decision.Reason,
                        "RANGE BREAKOUT " +
                        (rangeDirection > 0 ? "BUY" : "SELL") +
                        " " +
                        _rangeManipulation.Score);
                return;
            }

            if (recentSweep &&
                rangeDirection != 0)
            {
                // If the core engine is already aligned, strengthen it.
                if (decision.Direction == rangeDirection)
                {
                    decision.Confidence =
                        Math.Max(
                            decision.Confidence,
                            Math.Min(
                                88,
                                _rangeManipulation.Score));

                    decision.SmartQuality =
                        Math.Max(
                            decision.SmartQuality,
                            Math.Min(
                                88,
                                _rangeManipulation.Score));

                    decision.Reason =
                        AppendReason(
                            decision.Reason,
                            "RANGE SWEEP RECLAIM ALIGNED");
                }
                else if (decision.Direction == 0)
                {
                    // A sweep alone is not allowed to manufacture an immediate
                    // trade. It creates a directional watch state.
                    decision.Reason =
                        AppendReason(
                            decision.Reason,
                            "RANGE SWEEP " +
                            (rangeDirection > 0 ? "BUY" : "SELL") +
                            " WATCH");
                }
                else
                {
                    // Contradictory evidence is deliberately treated as NO TRADE
                    // rather than flipping an already-owned decision.
                    decision.EntryAllowed = false;
                    decision.ActionableNow = false;
                    decision.BlockReason =
                        "RANGE MANIPULATION CONFLICT";
                    decision.ActionabilityReason =
                        decision.BlockReason;
                    decision.Reason =
                        AppendReason(
                            decision.Reason,
                            "RANGE CONFLICT • NO TRADE");
                }
            }
        }

        private static string AppendReason(
            string current,
            string addition)
        {
            if (string.IsNullOrWhiteSpace(current))
                return addition;

            return current +
                   " | " +
                   addition;
        }
    }
}