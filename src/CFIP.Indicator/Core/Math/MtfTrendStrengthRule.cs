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

        public static MtfTrendStrengthResult Evaluate(
            Frame[] frames,
            double[] weights,
            double livePrice)
        {
            if (frames == null ||
                frames.Length == 0 ||
                !NumericGuards.IsFinitePositive(livePrice))
                return new MtfTrendStrengthResult(0, 0, 0, 0, 0);

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
                    Math.Max(0, Math.Min(100, frame.Quality)) / 100.0;

                double pressure =
                    Math.Max(
                        -1.0,
                        Math.Min(
                            1.0,
                            (livePrice - frame.EmaFast) /
                            Math.Max(
                                frame.Atr,
                                1e-9)));

                double directional =
                    frame.Direction != 0
                        ? frame.Direction
                        : pressure;

                if (frame.Direction != 0)
                {
                    // Closed-frame direction remains the anchor. Intrabar EMA
                    // pressure can strengthen or soften it without flipping a
                    // strong structural frame on one tick.
                    directional =
                        frame.Direction * 0.70 +
                        pressure * 0.30;
                }

                double contribution =
                    weight *
                    quality *
                    Math.Max(
                        0.25,
                        Math.Abs(directional));

                if (directional > 0)
                    weightedBull += contribution;
                else if (directional < 0)
                    weightedBear += contribution;

                weightedTotal +=
                    weight *
                    quality;

                // H1+ is the explicit higher-timeframe trend authority for the
                // nine-level arrow, while M15/M30 provide calibration context.
                if (i >= 4)
                {
                    if (directional > 0)
                        htfBull += contribution;
                    else if (directional < 0)
                        htfBear += contribution;

                    htfTotal += weight * quality;
                }
            }

            if (weightedTotal <= 0)
                return new MtfTrendStrengthResult(0, 0, 0, 0, 0);

            double bullShare =
                100.0 * weightedBull /
                Math.Max(
                    1e-9,
                    weightedBull + weightedBear);

            double bearShare = 100.0 - bullShare;

            int direction =
                bullShare == bearShare
                    ? 0
                    : bullShare > bearShare
                        ? 1
                        : -1;

            double dominance =
                Math.Max(
                    bullShare,
                    bearShare);

            double normalizedHtf =
                htfTotal <= 0
                    ? 50.0
                    : 100.0 *
                      Math.Max(htfBull, htfBear) /
                      Math.Max(
                          1e-9,
                          htfBull + htfBear);

            int htfDirection =
                htfBull == htfBear
                    ? 0
                    : htfBull > htfBear
                        ? 1
                        : -1;

            // H1+ is the requested arrow authority. When HTF data exists, arrow
            // intensity is derived from HTF dominance; the full MTF score remains
            // available for context and diagnostics.
            double arrowDominance =
                htfTotal > 0
                    ? normalizedHtf
                    : dominance;

            int level =
                (int)Math.Floor(
                    (arrowDominance - 50.0) /
                    5.0);

            if (arrowDominance <= 50.0)
                level = 0;
            else
                level = Math.Max(1, Math.Min(9, level));

            int arrowDirection =
                htfTotal > 0 &&
                htfDirection != 0
                    ? htfDirection
                    : direction;

            int score =
                (int)Math.Round(dominance);

            int htfScore =
                (int)Math.Round(
                    Math.Max(
                        50.0,
                        Math.Min(
                            100.0,
                            normalizedHtf)));

            return new MtfTrendStrengthResult(
                arrowDirection,
                level,
                score,
                htfScore,
                htfDirection);
        }
    }
}