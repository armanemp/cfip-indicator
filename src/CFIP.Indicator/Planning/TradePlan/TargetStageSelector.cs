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
                                            double alternateRR,
                                            OpportunityLane lane)
                                        {
                                            if (selected != null &&
                                                position < selected.Count &&
                                                selected[position] != null)
                                                return selected[position].Price;
                                
                                            bool requireHtf =
                                                RequiresHtfRewardForTargetStage(
                                                    position,
                                                    lane);
                                
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
                                
                                            double spread =
                                                IncludeSpreadInRiskSizing
                                                    ? Math.Max(
                                                        0,
                                                        Symbol.Ask - Symbol.Bid)
                                                    : 0;

                                            double synthetic =
                                                RiskRewardMathRule.TargetFromRR(
                                                    direction,
                                                    entry,
                                                    risk,
                                                    minimumRR,
                                                    spread);

                                            if (!IsFinitePositive(synthetic))
                                                return 0;

                                            double previous =
                                                FindPreviousSelectedTargetPrice(
                                                    selected,
                                                    position,
                                                    entry);

                                            if (!IsProgressiveTarget(
                                                    direction,
                                                    previous,
                                                    synthetic))
                                                return 0;

                                            return NormalizePrice(synthetic);
                                        }
    }
}
