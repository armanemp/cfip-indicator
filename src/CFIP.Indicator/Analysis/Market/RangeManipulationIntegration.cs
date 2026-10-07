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
                    RangeSignalMinimumScoreInternal,
                    SessionStartUtc);

            EnrichRangeManipulationWithMarketContext(
                closedM5);
        }

        private void EnrichRangeManipulationWithMarketContext(
            int closedM5)
        {
            RangeManipulationSnapshot snapshot =
                _rangeManipulation;

            if (snapshot == null ||
                !snapshot.IsRange ||
                _m5Bars == null ||
                closedM5 < 0 ||
                closedM5 >= _m5Bars.Count ||
                _m5Frame == null)
                return;

            double width =
                snapshot.High -
                snapshot.Low;

            if (width <= 0 ||
                double.IsNaN(width) ||
                double.IsInfinity(width))
                return;

            double close =
                _m5Bars.ClosePrices[closedM5];

            double position =
                (close - snapshot.Low) /
                width;

            bool nearHigh =
                position >= 0.72;

            bool nearLow =
                position <= 0.28;

            if (!nearHigh &&
                !nearLow)
            {
                snapshot.WatchDirection = 0;
                snapshot.IsManipulationWatch = false;
                snapshot.WatchScore = 0;
                snapshot.BoundaryPressure = 0;
                snapshot.MomentumPressure = 0;
                snapshot.VolumePressure = 0;
                snapshot.WatchReason = "";
                return;
            }

            int compressionScore = 0;

            if (_m5Frame.Choppy)
                compressionScore += 10;

            if (_m5Frame.Choppiness >= 60)
                compressionScore += 8;

            if (_m5Frame.RangeEfficiency <= 0.45)
                compressionScore += 8;

            if (Math.Abs(_m5Frame.EmaSlopeAtr) <= 0.25)
                compressionScore += 8;

            if (_m5Frame.EmaSpreadAtr <= 0.80)
                compressionScore += 8;

            if (_m5Frame.AtrRatio > 0 &&
                _m5Frame.AtrRatio <= 1.15)
                compressionScore += 5;

            compressionScore =
                Math.Min(
                    40,
                    compressionScore);

            int highLoad = 0;
            int lowLoad = 0;
            int highMomentum = 0;
            int lowMomentum = 0;
            int highVolume = 0;
            int lowVolume = 0;

            if (nearHigh)
            {
                highLoad = 28;

                if (_m5Frame.TrendBull)
                {
                    highLoad += 9;
                    highMomentum += 30;
                }

                if (_m5Frame.MomentumBull)
                {
                    highLoad += 14;
                    highMomentum += 40;
                }

                if (_m5Frame.DisplacementBull)
                {
                    highLoad += 8;
                    highMomentum += 20;
                }

                if (_m5Frame.VolumeBull)
                {
                    highLoad += 12;
                    highVolume += 55;
                }

                if (_m5Frame.EqualHigh)
                    highLoad += 12;

                if (_m5Frame.LiquidityBear)
                    highLoad += 8;

                if (_m15Frame != null &&
                    (_m15Frame.TrendBear ||
                     _m15Frame.StructureBear ||
                     _m15Frame.MssBear ||
                     _m15Frame.ChochBear ||
                     _m15Frame.ObBear ||
                     _m15Frame.FvgBear ||
                     _m15Frame.FvgObBearConfluence))
                    highLoad += 10;
            }
            else
            {
                lowLoad = 28;

                if (_m5Frame.TrendBear)
                {
                    lowLoad += 9;
                    lowMomentum += 30;
                }

                if (_m5Frame.MomentumBear)
                {
                    lowLoad += 14;
                    lowMomentum += 40;
                }

                if (_m5Frame.DisplacementBear)
                {
                    lowLoad += 8;
                    lowMomentum += 20;
                }

                if (_m5Frame.VolumeBear)
                {
                    lowLoad += 12;
                    lowVolume += 55;
                }

                if (_m5Frame.EqualLow)
                    lowLoad += 12;

                if (_m5Frame.LiquidityBull)
                    lowLoad += 8;

                if (_m15Frame != null &&
                    (_m15Frame.TrendBull ||
                     _m15Frame.StructureBull ||
                     _m15Frame.MssBull ||
                     _m15Frame.ChochBull ||
                     _m15Frame.ObBull ||
                     _m15Frame.FvgBull ||
                     _m15Frame.FvgObBullConfluence))
                    lowLoad += 10;
            }

            int expectedDirection =
                highLoad >= 48
                    ? -1
                    : lowLoad >= 48
                        ? 1
                        : 0;

            int boundaryPressure =
                Math.Max(
                    highLoad,
                    lowLoad);

            int momentumPressure =
                Math.Max(
                    highMomentum,
                    lowMomentum);

            int volumePressure =
                Math.Max(
                    highVolume,
                    lowVolume);

            int watchScore =
                Math.Min(
                    100,
                    snapshot.Score +
                    compressionScore +
                    Math.Min(
                        20,
                        Math.Max(
                            0,
                            boundaryPressure - 28) / 2));

            bool eligibleWatch =
                !snapshot.IsManipulation &&
                !snapshot.IsConfirmedBreakout &&
                expectedDirection != 0 &&
                boundaryPressure >= 48 &&
                watchScore >= 64;

            snapshot.IsManipulationWatch =
                eligibleWatch;

            snapshot.WatchDirection =
                eligibleWatch
                    ? expectedDirection
                    : 0;

            snapshot.WatchScore =
                eligibleWatch
                    ? watchScore
                    : Math.Min(
                        100,
                        Math.Max(
                            0,
                            watchScore));

            snapshot.BoundaryPressure =
                Math.Min(
                    100,
                    boundaryPressure);

            snapshot.MomentumPressure =
                Math.Min(
                    100,
                    momentumPressure);

            snapshot.VolumePressure =
                Math.Min(
                    100,
                    volumePressure);

            if (!eligibleWatch)
            {
                snapshot.WatchReason = "";
                return;
            }

            snapshot.Direction =
                expectedDirection;

            string prefix =
                snapshot.State.StartsWith(
                    "OPENING_",
                    StringComparison.OrdinalIgnoreCase)
                    ? "OPENING_"
                    : "";

            snapshot.State =
                prefix +
                (expectedDirection > 0
                    ? "BULL_MANIPULATION_WATCH"
                    : "BEAR_MANIPULATION_WATCH");

            snapshot.WatchReason =
                (expectedDirection > 0
                    ? "LOW-SIDE SELL PRESSURE"
                    : "HIGH-SIDE BUY PRESSURE") +
                " | COMP " +
                compressionScore +
                " | BND " +
                Math.Min(
                    100,
                    boundaryPressure) +
                " | MOM " +
                Math.Min(
                    100,
                    momentumPressure) +
                " | VOL " +
                Math.Min(
                    100,
                    volumePressure) +
                " | SCORE " +
                watchScore;
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