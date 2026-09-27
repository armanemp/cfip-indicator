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
                
                            double average = 0;
                            int count = 0;
                            int first =
                                Math.Max(
                                    0,
                                    index - 20);
                
                            for (int i = first;
                                 i < index;
                                 i++)
                            {
                                average +=
                                    Math.Max(
                                        0,
                                        bars.TickVolumes[i]);
                
                                count++;
                            }
                
                            if (count == 0 ||
                                average <= 0)
                                return false;
                
                            average /= count;
                
                            bool directional =
                                direction == 1
                                    ? bars.ClosePrices[index] >
                                      bars.OpenPrices[index]
                                    : bars.ClosePrices[index] <
                                      bars.OpenPrices[index];
                
                            return
                                directional &&
                                bars.TickVolumes[index] >=
                                average *
                                Math.Max(
                                    1.0,
                                    VolumeExpansionRatio);
                        }
    }
}
