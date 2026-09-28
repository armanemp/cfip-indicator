// CFIP Indicator — VolumeExpansionAnalyzer.cs
// Single-responsibility analysis module.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool HasVolumeExpansion(
                            Bars bars,
                            int index,
                            int direction)
                        {
                            if (!UseVolumeExpansion ||
                                bars == null ||
                                index < 25)
                                return false;

                            double averageVolume = 0;
                            double averageRange = 0;
                            int count = 0;
                            int first =
                                Math.Max(
                                    0,
                                    index - 20);

                            for (int i = first;
                                 i < index;
                                 i++)
                            {
                                averageVolume +=
                                    Math.Max(
                                        0,
                                        bars.TickVolumes[i]);

                                averageRange +=
                                    Math.Max(
                                        0,
                                        bars.HighPrices[i] -
                                        bars.LowPrices[i]);

                                count++;
                            }

                            if (count == 0)
                                return false;

                            averageVolume /= count;
                            averageRange /= count;

                            if (averageVolume <= 0 ||
                                averageRange <= 0)
                                return false;

                            double currentRange =
                                Math.Max(
                                    Symbol.PipSize,
                                    bars.HighPrices[index] -
                                    bars.LowPrices[index]);

                            double currentBody =
                                Math.Abs(
                                    bars.ClosePrices[index] -
                                    bars.OpenPrices[index]);

                            double bodyShare =
                                currentBody /
                                currentRange;

                            bool directional =
                                direction == 1
                                    ? bars.ClosePrices[index] >
                                      bars.OpenPrices[index]
                                    : bars.ClosePrices[index] <
                                      bars.OpenPrices[index];

                            double closeLocation =
                                direction == 1
                                    ? (bars.ClosePrices[index] -
                                       bars.LowPrices[index]) /
                                      currentRange
                                    : (bars.HighPrices[index] -
                                       bars.ClosePrices[index]) /
                                      currentRange;

                            bool priceResult =
                                currentRange >=
                                    averageRange * 0.90 &&
                                bodyShare >= 0.45 &&
                                (direction == 1
                                    ? closeLocation >= 0.65
                                    : closeLocation >= 0.65);

                            return
                                directional &&
                                priceResult &&
                                bars.TickVolumes[index] >=
                                averageVolume *
                                Math.Max(
                                    1.0,
                                    VolumeExpansionRatio);
                        }
    }
}
