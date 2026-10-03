using System;

namespace cAlgo
{
    internal static class HtfTrendArrowStrengthRule
    {
        // Nine deterministic presentation levels:
        // 1-3 weak, 4-6 medium, 7-9 strong.
        // H1 contributes the base trend strength, H4 adds confirmation,
        // D1 adds higher-order confirmation and W1 supplies the final bias.
        public static int ResolveStrength(
            Frame h1,
            Frame h4,
            Frame d1,
            Frame w1,
            int direction)
        {
            if (direction != 1 && direction != -1)
                return 0;

            return Math.Max(
                0,
                Math.Min(
                    9,
                    FrameStrength(h1, direction, 3) +
                    FrameStrength(h4, direction, 3) +
                    FrameStrength(d1, direction, 2) +
                    FrameStrength(w1, direction, 1)));
        }

        private static int FrameStrength(
            Frame frame,
            int direction,
            int maximum)
        {
            if (frame == null ||
                !frame.NativeIndicatorsReady ||
                maximum <= 0)
                return 0;

            bool directionalBias =
                frame.Direction == direction ||
                (direction == 1
                    ? frame.TrendBull
                    : frame.TrendBear);

            if (!directionalBias)
                return 0;

            int score =
                direction == 1
                    ? frame.BullScore
                    : frame.BearScore;

            int quality =
                Math.Max(
                    0,
                    Math.Min(
                        100,
                        frame.Quality));

            double adx =
                double.IsNaN(frame.Adx) || double.IsInfinity(frame.Adx)
                    ? 0
                    : frame.Adx;

            double spread =
                double.IsNaN(frame.EmaSpreadAtr) || double.IsInfinity(frame.EmaSpreadAtr)
                    ? 0
                    : Math.Abs(frame.EmaSpreadAtr);

            double slope =
                double.IsNaN(frame.EmaSlopeAtr) || double.IsInfinity(frame.EmaSlopeAtr)
                    ? 0
                    : Math.Abs(frame.EmaSlopeAtr);

            int strength = 1;

            if (score >= 55 ||
                quality >= 55 ||
                (adx >= 20 && spread >= 0.20))
                strength = 2;

            if (score >= 70 ||
                quality >= 70 ||
                (adx >= 25 && spread >= 0.35 && slope >= 0.05))
                strength = 3;

            return Math.Min(maximum, strength);
        }
    }
}
