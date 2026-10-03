using System;
using cAlgo.API;

namespace cAlgo
{
    internal static class VolumeProfileAnalyzer
    {
        public static VolumeProfileSnapshot BuildSnapshot(
            Bars bars,
            int closedIndex,
            int lookbackBars,
            int bins,
            int valueAreaPercent)
        {
            if (bars == null ||
                closedIndex < 0 ||
                closedIndex >= bars.Count ||
                lookbackBars < 8 ||
                bins < 12 ||
                valueAreaPercent <= 0 ||
                valueAreaPercent > 100)
                return VolumeProfileSnapshot.Empty;

            int start =
                Math.Max(
                    0,
                    closedIndex -
                    lookbackBars +
                    1);

            if (closedIndex - start + 1 < 8)
                return VolumeProfileSnapshot.Empty;

            double low = double.MaxValue;
            double high = double.MinValue;

            for (int i = start; i <= closedIndex; i++)
            {
                double barLow = bars.LowPrices[i];
                double barHigh = bars.HighPrices[i];

                if (!IsVolumeProfileFinitePositive(barLow) ||
                    !IsVolumeProfileFinitePositive(barHigh) ||
                    barHigh < barLow)
                    continue;

                low = Math.Min(low, barLow);
                high = Math.Max(high, barHigh);
            }

            if (!IsVolumeProfileFinitePositive(low) ||
                !IsVolumeProfileFinitePositive(high) ||
                high <= low)
                return VolumeProfileSnapshot.Empty;

            bins =
                Math.Max(
                    12,
                    Math.Min(
                        96,
                        bins));

            double binSize =
                (high - low) /
                bins;

            if (!IsVolumeProfileFinitePositive(binSize))
                return VolumeProfileSnapshot.Empty;

            double[] volumeByBin =
                new double[bins];

            double totalVolume = 0;

            for (int i = start; i <= closedIndex; i++)
            {
                double barLow = bars.LowPrices[i];
                double barHigh = bars.HighPrices[i];
                double volume =
                    Math.Max(
                        0,
                        bars.TickVolumes[i]);

                if (!IsVolumeProfileFinitePositive(volume) ||
                    !IsVolumeProfileFinitePositive(barLow) ||
                    !IsVolumeProfileFinitePositive(barHigh) ||
                    barHigh < barLow)
                    continue;

                totalVolume += volume;

                double barRange =
                    Math.Max(
                        binSize * 0.01,
                        barHigh - barLow);

                int firstBin =
                    PriceToBin(
                        barLow,
                        low,
                        binSize,
                        bins);

                int lastBin =
                    PriceToBin(
                        barHigh,
                        low,
                        binSize,
                        bins);

                if (firstBin == lastBin)
                {
                    volumeByBin[firstBin] += volume;
                    continue;
                }

                for (int b = firstBin;
                     b <= lastBin;
                     b++)
                {
                    double binLow =
                        low +
                        b * binSize;

                    double binHigh =
                        b == bins - 1
                            ? high
                            : binLow + binSize;

                    double overlap =
                        Math.Min(barHigh, binHigh) -
                        Math.Max(barLow, binLow);

                    if (overlap <= 0)
                        continue;

                    volumeByBin[b] +=
                        volume *
                        Math.Min(
                            1,
                            Math.Max(
                                0,
                                overlap / barRange));
                }
            }

            if (!IsVolumeProfileFinitePositive(totalVolume))
                return VolumeProfileSnapshot.Empty;

            int pocIndex = 0;
            double pocVolume = 0;

            for (int i = 0; i < bins; i++)
            {
                if (volumeByBin[i] > pocVolume)
                {
                    pocVolume = volumeByBin[i];
                    pocIndex = i;
                }
            }

            if (!IsVolumeProfileFinitePositive(pocVolume))
                return VolumeProfileSnapshot.Empty;

            double targetVolume =
                totalVolume *
                Math.Min(
                    1,
                    valueAreaPercent / 100.0);

            double valueAreaVolume =
                pocVolume;

            int lowerIndex = pocIndex;
            int upperIndex = pocIndex;

            while (valueAreaVolume < targetVolume &&
                   (lowerIndex > 0 ||
                    upperIndex < bins - 1))
            {
                double lowerVolume =
                    lowerIndex > 0
                        ? volumeByBin[lowerIndex - 1]
                        : -1;

                double upperVolume =
                    upperIndex < bins - 1
                        ? volumeByBin[upperIndex + 1]
                        : -1;

                if (upperVolume >= lowerVolume)
                {
                    if (upperIndex >= bins - 1)
                        break;

                    upperIndex++;
                    valueAreaVolume +=
                        Math.Max(
                            0,
                            upperVolume);
                }
                else
                {
                    if (lowerIndex <= 0)
                        break;

                    lowerIndex--;
                    valueAreaVolume +=
                        Math.Max(
                            0,
                            lowerVolume);
                }
            }

            double poc =
                low +
                (pocIndex + 0.5) *
                binSize;

            double val =
                low +
                lowerIndex *
                binSize;

            double vah =
                low +
                (upperIndex + 1) *
                binSize;

            return new VolumeProfileSnapshot(
                true,
                closedIndex,
                bins,
                low,
                high,
                binSize,
                poc,
                val,
                vah,
                totalVolume,
                pocVolume);
        }

        private static int PriceToBin(
            double price,
            double low,
            double binSize,
            int bins)
        {
            int index =
                (int)Math.Floor(
                    (price - low) /
                    binSize);

            return Math.Max(
                0,
                Math.Min(
                    bins - 1,
                    index));
        }

        private static bool IsVolumeProfileFinitePositive(double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value) &&
                value > 0;
        }
    }
}
