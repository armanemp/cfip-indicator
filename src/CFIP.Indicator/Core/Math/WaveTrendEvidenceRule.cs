using System;

namespace cAlgo
{
    internal readonly struct WaveTrendEvidenceResult
    {
        public int Direction { get; }
        public int Quality { get; }
        public bool Bull { get; }
        public bool Bear { get; }

        public WaveTrendEvidenceResult(
            int direction,
            int quality,
            bool bull,
            bool bear)
        {
            Direction = direction;
            Quality = Math.Max(0, Math.Min(100, quality));
            Bull = bull;
            Bear = bear;
        }
    }

    internal static class WaveTrendEvidenceRule
    {
        internal static int NormalizeMinimumQuality(
            int minimumQuality)
        {
            return Math.Max(40, Math.Min(100, minimumQuality));
        }

        internal static bool MeetsMinimumQuality(
            int quality,
            int minimumQuality)
        {
            return quality >= NormalizeMinimumQuality(minimumQuality);
        }

        internal static WaveTrendEvidenceResult Evaluate(
            WaveTrendSnapshot snapshot,
            int minimumQuality)
        {
            if (!snapshot.Valid)
                return new WaveTrendEvidenceResult(0, 0, false, false);

            bool bull =
                snapshot.BullCross ||
                (snapshot.Wave > snapshot.Signal &&
                 snapshot.Rising);

            bool bear =
                snapshot.BearCross ||
                (snapshot.Wave < snapshot.Signal &&
                 snapshot.Falling);

            if (snapshot.Oversold &&
                snapshot.Rising &&
                snapshot.Wave > snapshot.PreviousWave)
                bull = true;

            if (snapshot.Overbought &&
                snapshot.Falling &&
                snapshot.Wave < snapshot.PreviousWave)
                bear = true;

            double separation =
                Math.Abs(
                    snapshot.Wave -
                    snapshot.Signal);

            double slope =
                Math.Abs(
                    snapshot.WaveDelta);

            double zoneStrength =
                snapshot.Oversold ||
                snapshot.Overbought
                    ? 22
                    : 0;

            double crossStrength =
                snapshot.BullCross ||
                snapshot.BearCross
                    ? 30
                    : 0;

            int quality =
                (int)Math.Round(
                    Math.Min(
                        100,
                        separation * 1.25 +
                        slope * 1.75 +
                        zoneStrength +
                        crossStrength +
                        (snapshot.AboveZero ||
                         snapshot.BelowZero
                            ? 10
                            : 0)));

            bool strongBull =
                bull &&
                MeetsMinimumQuality(
                    quality,
                    minimumQuality);

            bool strongBear =
                bear &&
                quality >=
                Math.Max(
                    40,
                    minimumQuality);

            int direction =
                strongBull == strongBear
                    ? 0
                    : strongBull
                        ? 1
                        : -1;

            return new WaveTrendEvidenceResult(
                direction,
                quality,
                strongBull,
                strongBear);
        }
    }
}
