using System;

namespace cAlgo
{
    internal sealed class MtfTrendStrengthResult
    {
        public int Direction { get; }
        public int Level { get; }
        public int Score { get; }
        public int HigherTimeframeScore { get; }
        public int HigherTimeframeDirection { get; }

        public MtfTrendStrengthResult(
            int direction,
            int level,
            int score,
            int higherTimeframeScore,
            int higherTimeframeDirection)
        {
            Direction = direction;
            Level = level;
            Score = score;
            HigherTimeframeScore = higherTimeframeScore;
            HigherTimeframeDirection = higherTimeframeDirection;
        }
    }

    internal static class MtfTrendStrengthRule
    {
        private static readonly double[] DefaultWeights =
        {
            0.00, // M1: trigger/precision, not HTF trend authority
            0.50, // M5
            1.00, // M15
            1.10, // M30
            1.35, // H1
            1.55, // H4
            1.75, // D1
            1.95  // W1
        };

        private const double ScoreNormalizationMaximum = 70.0;
        private const double LevelStartScore = 55.0;
        private const double LevelStepScore = 5.0;

        public static MtfTrendStrengthResult Evaluate(
            Frame[] frames,
            double[] weights,
            double livePrice)
        {
            return Evaluate(
                frames,
                weights,
                livePrice,
                0);
        }

        public static MtfTrendStrengthResult Evaluate(
            Frame[] frames,
            double[] weights,
            double livePrice,
            int preferredDirection)
        {
            if (frames == null ||
                frames.Length == 0 ||
                !NumericGuards.IsFinitePositive(livePrice))
                return new MtfTrendStrengthResult(0, 0, 0, 0, 0);

            preferredDirection =
                preferredDirection == 1 || preferredDirection == -1
                    ? preferredDirection
                    : 0;

            double weightedBull = 0;
            double weightedBear = 0;
            double weightedTotal = 0;

            double htfBull = 0;
            double htfBear = 0;
            double htfTotal = 0;

            for (int i = 0; i < frames.Length; i++)
            {
                Frame frame = frames[i];

                if (frame == null ||
                    !frame.NativeIndicatorsReady ||
                    frame.Atr <= 0)
                    continue;

                double weight =
                    weights != null && i < weights.Length
                        ? Math.Max(0, weights[i])
                        : i < DefaultWeights.Length
                            ? DefaultWeights[i]
                            : 0;

                if (weight <= 0)
                    continue;

                double bullStrength =
                    ResolveFrameSideStrength(
                        frame,
                        1,
                        livePrice);

                double bearStrength =
                    ResolveFrameSideStrength(
                        frame,
                        -1,
                        livePrice);

                weightedBull +=
                    weight *
                    bullStrength;

                weightedBear +=
                    weight *
                    bearStrength;

                weightedTotal += weight;

                if (i >= 4)
                {
                    htfBull +=
                        weight *
                        bullStrength;

                    htfBear +=
                        weight *
                        bearStrength;

                    htfTotal += weight;
                }
            }

            if (weightedTotal <= 0)
                return new MtfTrendStrengthResult(0, 0, 0, 0, 0);

            double bullAverage =
                NumericGuards.ClampDouble(
                    weightedBull / weightedTotal,
                    0,
                    100);

            double bearAverage =
                NumericGuards.ClampDouble(
                    weightedBear / weightedTotal,
                    0,
                    100);

            int overallDirection =
                ResolveDominantDirection(
                    bullAverage,
                    bearAverage);

            int arrowDirection =
                preferredDirection != 0
                    ? preferredDirection
                    : overallDirection;

            double selectedStrength =
                arrowDirection == 1
                    ? bullAverage
                    : arrowDirection == -1
                        ? bearAverage
                        : 0;

            double selectedDominance =
                ResolveDominance(
                    arrowDirection == 1
                        ? bullAverage
                        : bearAverage,
                    arrowDirection == 1
                        ? bearAverage
                        : bullAverage);

            int score =
                ResolveCompositeScore(
                    selectedStrength,
                    selectedDominance);

            int level =
                ResolveNineLevel(score);

            double htfBullAverage =
                htfTotal <= 0
                    ? 0
                    : NumericGuards.ClampDouble(
                        htfBull / htfTotal,
                        0,
                        100);

            double htfBearAverage =
                htfTotal <= 0
                    ? 0
                    : NumericGuards.ClampDouble(
                        htfBear / htfTotal,
                        0,
                        100);

            int htfDirection =
                ResolveDominantDirection(
                    htfBullAverage,
                    htfBearAverage);

            int higherScoreDirection =
                preferredDirection != 0
                    ? preferredDirection
                    : htfDirection;

            int htfScore =
                ResolveCompositeScore(
                    higherScoreDirection == 1
                        ? htfBullAverage
                        : htfScoreDirection(higherScoreDirection, htfBearAverage, htfBullAverage),
                    ResolveDominance(
                        higherScoreDirection == 1
                            ? htfBullAverage
                            : htfBearAverage,
                        higherScoreDirection == 1
                            ? htfBearAverage
                            : htfBullAverage));

            return new MtfTrendStrengthResult(
                arrowDirection,
                level,
                score,
                NumericGuards.ClampInt(
                    htfScore,
                    0,
                    100),
                htfDirection);
        }

        private static double htfScoreDirection(
            int direction,
            double primary,
            double opposite)
        {
            return direction == -1
                ? primary
                : 0;
        }

        private static int ResolveDominantDirection(
            double bull,
            double bear)
        {
            if (bull <= 0 &&
                bear <= 0)
                return 0;

            if (Math.Abs(bull - bear) < 0.5)
                return 0;

            return bull > bear
                ? 1
                : -1;
        }

        private static double ResolveDominance(
            double primary,
            double opposite)
        {
            double total =
                primary +
                opposite;

            if (total <= 0)
                return 50.0;

            return NumericGuards.ClampDouble(
                100.0 *
                primary /
                total,
                50.0,
                100.0);
        }

        private static int ResolveCompositeScore(
            double strength,
            double dominance)
        {
            return NumericGuards.ClampInt(
                (int)Math.Round(
                    strength * 0.65 +
                    dominance * 0.35),
                0,
                100);
        }

        private static int ResolveNineLevel(
            int score)
        {
            if (score < LevelStartScore)
                return 0;

            return NumericGuards.ClampInt(
                (int)Math.Ceiling(
                    (score - 50.0) /
                    LevelStepScore),
                1,
                9);
        }

        private static double ResolveFrameSideStrength(
            Frame frame,
            int direction,
            double livePrice)
        {
            if (frame == null ||
                (direction != 1 && direction != -1) ||
                !frame.NativeIndicatorsReady ||
                frame.Atr <= 0)
                return 0;

            int sideScore =
                direction == 1
                    ? frame.BullScore
                    : frame.BearScore;

            int oppositeScore =
                direction == 1
                    ? frame.BearScore
                    : frame.BullScore;

            double quality =
                NumericGuards.ClampDouble(
                    frame.Quality,
                    0,
                    100);

            double normalizedScore =
                NumericGuards.ClampDouble(
                    sideScore /
                    ScoreNormalizationMaximum *
                    100.0,
                    0,
                    100);

            double trendQuality =
                ResolveTrendQuality(
                    frame,
                    direction);

            double adxQuality =
                ResolveRangeQuality(
                    frame.Adx,
                    15.0,
                    35.0);

            double spreadQuality =
                ResolveRangeQuality(
                    Math.Abs(frame.EmaSpreadAtr),
                    0.10,
                    0.80);

            double slopeQuality =
                ResolveRangeQuality(
                    Math.Abs(frame.EmaSlopeAtr),
                    0.02,
                    0.15);

            double structuralQuality =
                ResolveStructuralQuality(
                    frame,
                    direction);

            double locationQuality =
                ResolveLocationQuality(
                    frame,
                    direction);

            double evidenceQuality =
                NumericGuards.ClampDouble(
                    frame.IndicatorIndependentEvidenceGroupCount /
                    3.0 *
                    100.0,
                    0,
                    100);

            double livePressureQuality =
                ResolveLivePressureQuality(
                    frame,
                    direction,
                    livePrice);

            double raw =
                normalizedScore * 0.24 +
                quality * 0.22 +
                trendQuality * 0.14 +
                adxQuality * 0.10 +
                spreadQuality * 0.07 +
                slopeQuality * 0.05 +
                structuralQuality * 0.06 +
                locationQuality * 0.05 +
                evidenceQuality * 0.03 +
                livePressureQuality * 0.04;

            double alignment =
                ResolveFrameAlignment(
                    frame,
                    direction,
                    sideScore,
                    oppositeScore);

            double conflictPenalty =
                NumericGuards.ClampDouble(
                    Math.Max(
                        0,
                        frame.IndicatorConflict - 30) *
                    0.35,
                    0,
                    18);

            return NumericGuards.ClampDouble(
                raw *
                alignment -
                conflictPenalty,
                0,
                100);
        }

        private static double ResolveFrameAlignment(
            Frame frame,
            int direction,
            int sideScore,
            int oppositeScore)
        {
            if (frame.Direction == direction)
                return 1.0;

            if (frame.Direction == -direction)
                return 0.35;

            if (sideScore > oppositeScore)
                return 0.72;

            if (sideScore < oppositeScore)
                return 0.45;

            return 0.55;
        }

        private static double ResolveTrendQuality(
            Frame frame,
            int direction)
        {
            bool trend =
                direction == 1
                    ? frame.TrendBull
                    : frame.TrendBear;

            bool momentum =
                direction == 1
                    ? frame.MomentumBull
                    : frame.MomentumBear;

            if (trend && momentum)
                return 100;

            if (trend)
                return 88;

            if (momentum)
                return 68;

            if (direction == frame.Direction)
                return 62;

            return 35;
        }

        private static double ResolveStructuralQuality(
            Frame frame,
            int direction)
        {
            bool structure =
                direction == 1
                    ? frame.StructureBull
                    : frame.StructureBear;

            bool mss =
                direction == 1
                    ? frame.MssBull
                    : frame.MssBear;

            bool choch =
                direction == 1
                    ? frame.ChochBull
                    : frame.ChochBear;

            bool displacement =
                direction == 1
                    ? frame.DisplacementBull
                    : frame.DisplacementBear;

            if (mss || choch)
                return 100;

            if (structure)
                return 82;

            if (displacement)
                return 62;

            return 30;
        }

        private static double ResolveLocationQuality(
            Frame frame,
            int direction)
        {
            bool fvg =
                direction == 1
                    ? frame.FvgBull
                    : frame.FvgBear;

            bool ob =
                direction == 1
                    ? frame.ObBull
                    : frame.ObBear;

            bool fvgOb =
                direction == 1
                    ? frame.FvgObBullConfluence
                    : frame.FvgObBearConfluence;

            int fvgQuality =
                direction == 1
                    ? frame.FvgBullQuality
                    : frame.FvgBearQuality;

            int obQuality =
                direction == 1
                    ? frame.ObBullQuality
                    : frame.ObBearQuality;

            double location =
                Math.Max(
                    fvgQuality,
                    obQuality);

            if (fvgOb)
                return 100;

            if (fvg || ob)
                return NumericGuards.ClampDouble(
                    location,
                    45,
                    95);

            return 20;
        }

        private static double ResolveLivePressureQuality(
            Frame frame,
            int direction,
            double livePrice)
        {
            if (!NumericGuards.IsFinitePositive(frame.EmaFast) ||
                !NumericGuards.IsFinitePositive(frame.Atr) ||
                !NumericGuards.IsFinitePositive(livePrice))
                return 50;

            double signedPressure =
                (livePrice - frame.EmaFast) /
                Math.Max(
                    frame.Atr,
                    1e-9);

            signedPressure *= direction;

            if (signedPressure <= 0)
                return 25;

            return NumericGuards.ClampDouble(
                50.0 +
                signedPressure * 50.0,
                50.0,
                100.0);
        }

        private static double ResolveRangeQuality(
            double value,
            double minimum,
            double maximum)
        {
            if (double.IsNaN(value) ||
                double.IsInfinity(value))
                return 0;

            if (maximum <= minimum)
                return 0;

            return NumericGuards.ClampDouble(
                (value - minimum) /
                (maximum - minimum) *
                100.0,
                0,
                100);
        }
    }
}
