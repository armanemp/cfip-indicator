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
            0.00, // M1: precision/confirmation only
            0.50, // M5
            1.00, // M15: canonical decision/reference
            1.10, // M30
            1.35, // H1
            1.55, // H4
            1.75, // D1
            1.95  // W1
        };

        // One owner for the complete nine-level trend model:
        // 1-3 = WEAK, 4-6 = MEDIUM, 7-9 = STRONG.
        // The level is derived from signed multi-timeframe evidence, not
        // from a second HTF-only strength calculator.
        public static MtfTrendStrengthResult Evaluate(
            Frame[] frames,
            double[] weights,
            double livePrice)
        {
            if (frames == null ||
                frames.Length == 0 ||
                !NumericGuards.IsFinitePositive(livePrice))
                return CreateEmptyTrendStrengthResult();

            double signedEvidence = 0;
            double absoluteEvidence = 0;
            double htfSignedEvidence = 0;
            double htfAbsoluteEvidence = 0;

            for (int i = 0; i < frames.Length; i++)
            {
                Frame frame = frames[i];

                if (frame == null ||
                    !frame.NativeIndicatorsReady ||
                    frame.Atr <= 0 ||
                    frame.Quality <= 0)
                    continue;

                double weight =
                    weights != null && i < weights.Length
                        ? Math.Max(0, weights[i])
                        : i < DefaultWeights.Length
                            ? DefaultWeights[i]
                            : 0;

                if (weight <= 0)
                    continue;

                double quality =
                    ClampMtfTrend01(frame.Quality / 100.0);

                double bullScore =
                    ClampMtfTrend01(frame.BullScore / 100.0);

                double bearScore =
                    ClampMtfTrend01(frame.BearScore / 100.0);

                int structuralDirection =
                    ResolveStructuralDirection(frame);

                double pressure =
                    ClampMtfTrendSigned(
                        (livePrice - frame.EmaFast) /
                        Math.Max(frame.Atr, 1e-9));

                double pressureDirection =
                    structuralDirection != 0
                        ? structuralDirection * 0.75 +
                          pressure * 0.25
                        : pressure * 0.50;

                double directionalScore =
                    structuralDirection > 0
                        ? bullScore
                        : structuralDirection < 0
                            ? bearScore
                            : Math.Max(bullScore, bearScore);

                // Quality, directional score and live pressure all contribute.
                // A direction without evidence therefore cannot manufacture a
                // strong arrow merely by winning a percentage comparison.
                double magnitude =
                    ClampMtfTrend01(
                        quality * 0.40 +
                        directionalScore * 0.40 +
                        Math.Abs(pressureDirection) * 0.20);

                if (magnitude <= 0)
                    continue;

                double signedContribution =
                    weight *
                    magnitude *
                    ClampMtfTrendSigned(pressureDirection);

                signedEvidence += signedContribution;
                absoluteEvidence +=
                    weight *
                    magnitude;

                if (i >= 4)
                {
                    htfSignedEvidence += signedContribution;
                    htfAbsoluteEvidence +=
                        weight *
                        magnitude;
                }
            }

            if (absoluteEvidence <= 0)
                return CreateEmptyTrendStrengthResult();

            double normalizedStrength =
                Math.Min(
                    1.0,
                    Math.Abs(signedEvidence) /
                    Math.Max(1e-9, absoluteEvidence));

            int direction =
                ResolveMtfTrendStrengthDirection(
                    signedEvidence,
                    htfSignedEvidence,
                    htfAbsoluteEvidence);

            if (direction == 0)
                return new MtfTrendStrengthResult(
                    0,
                    0,
                    50,
                    50,
                    0);

            // 0.10 is the minimum meaningful directional dominance.
            // The remaining range is quantized into exactly nine levels.
            int level =
                normalizedStrength < 0.10
                    ? 0
                    : Math.Max(
                        1,
                        Math.Min(
                            9,
                            (int)Math.Ceiling(
                                normalizedStrength * 9.0)));

            int score =
                50 +
                (int)Math.Round(
                    normalizedStrength * 50.0);

            double normalizedHtf =
                htfAbsoluteEvidence <= 0
                    ? 0
                    : Math.Min(
                        1.0,
                        Math.Abs(htfSignedEvidence) /
                        Math.Max(
                            1e-9,
                            htfAbsoluteEvidence));

            int htfDirection =
                htfSignedEvidence > 0
                    ? 1
                    : htfSignedEvidence < 0
                        ? -1
                        : 0;

            int htfScore =
                htfAbsoluteEvidence <= 0
                    ? 50
                    : 50 +
                      (int)Math.Round(
                          normalizedHtf * 50.0);

            return new MtfTrendStrengthResult(
                direction,
                level,
                Math.Max(50, Math.Min(100, score)),
                Math.Max(50, Math.Min(100, htfScore)),
                htfDirection);
        }

        private static int ResolveStructuralDirection(Frame frame)
        {
            if (frame.Direction == 1 ||
                frame.Direction == -1)
                return frame.Direction;

            if (frame.TrendBull && !frame.TrendBear)
                return 1;

            if (frame.TrendBear && !frame.TrendBull)
                return -1;

            return 0;
        }

        private static int ResolveMtfTrendStrengthDirection(
            double signedEvidence,
            double htfSignedEvidence,
            double htfAbsoluteEvidence)
        {
            if (Math.Abs(signedEvidence) > 1e-9)
                return signedEvidence > 0 ? 1 : -1;

            if (htfAbsoluteEvidence > 0 &&
                Math.Abs(htfSignedEvidence) > 1e-9)
                return htfSignedEvidence > 0 ? 1 : -1;

            return 0;
        }

        private static double ClampMtfTrend01(double value)
        {
            if (double.IsNaN(value) ||
                double.IsInfinity(value))
                return 0;

            return Math.Max(
                0,
                Math.Min(
                    1,
                    value));
        }

        private static double ClampMtfTrendSigned(double value)
        {
            if (double.IsNaN(value) ||
                double.IsInfinity(value))
                return 0;

            return Math.Max(
                -1,
                Math.Min(
                    1,
                    value));
        }

        private static MtfTrendStrengthResult CreateEmptyTrendStrengthResult()
        {
            return new MtfTrendStrengthResult(
                0,
                0,
                0,
                0,
                0);
        }
    }
}