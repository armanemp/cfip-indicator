// CFIP Indicator — TargetStageSelector.cs
// Single-responsibility planning module.

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
private double SelectTarget(
                                            List<Level> selected,
                                            int position,
                                            double entry,
                                            double risk,
                                            int direction,
                                            double alternateRR)
                                        {
                                            if (selected != null &&
                                                position < selected.Count &&
                                                selected[position] != null)
                                                return selected[position].Price;
                                
                                            bool requireHtf =
                                                position == 0
                                                    ? RequireHtfRewardForTp1
                                                    : RequireHtfRewardForTp2Plus;
                                
                                            double minimumRR =
                                                Math.Max(
                                                    0.50,
                                                    alternateRR);
                                
                                            double maximumRR =
                                                Math.Max(
                                                    minimumRR,
                                                    MaximumRewardRR);
                                
                                            if (!AllowSyntheticTargetFallback ||
                                                requireHtf ||
                                                minimumRR > maximumRR)
                                                return 0;
                                
                                            return NormalizePrice(
                                                direction == 1
                                                    ? entry +
                                                      risk *
                                                      minimumRR
                                                    : entry -
                                                      risk *
                                                      minimumRR);
                                        }
    }
}
