// CFIP Indicator — LivePlanTargetEnrichment.cs
// Single-responsibility lifecycle module.

using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void EnrichLivePlanTargets(int closedM5)
                        {
                            if (_plan == null ||
                                !_plan.IsLivePosition ||
                                _m5Bars == null)
                                return;
                
                            int index =
                                Math.Max(
                                    1,
                                    Math.Min(
                                        closedM5,
                                        _m5Bars.Count - 2));
                
                            double atr =
                                Atr(
                                    _m5Bars,
                                    index);
                
                            if (atr <= 0)
                                return;
                
                            List<Level> levels =
                                BuildTargetLevels(
                                    index,
                                    _plan.Direction,
                                    _plan.Entry,
                                    atr);
                
                            List<Level> selected =
                                SelectTargets(
                                    levels,
                                    index,
                                    _plan.Entry,
                                    Math.Max(
                                        Symbol.PipSize,
                                        _plan.Risk),
                                    _plan.Direction,
                                    atr,
                                    _plan.Lane);
                
                            double market =
                                _plan.Direction == 1
                                    ? Symbol.Bid
                                    : Symbol.Ask;

                            double forwardDistance =
                                MinimumLiveTargetDistancePrice(_plan.Direction, atr);

                            double baseTarget =
                                _plan.Tp1;

                            double existingTp2 =
                                _plan.Tp2;

                            double existingTp3 =
                                _plan.Tp3;

                            double existingTp4 =
                                _plan.Tp4;

                            string existingTp2Source =
                                _plan.Tp2Source;
                            int existingTp2Quality =
                                _plan.Tp2Quality;

                            string existingTp3Source =
                                _plan.Tp3Source;
                            int existingTp3Quality =
                                _plan.Tp3Quality;

                            string existingTp4Source =
                                _plan.Tp4Source;
                            int existingTp4Quality =
                                _plan.Tp4Quality;

                            double candidateTp2 =
                                FindFurtherLiveTarget(
                                    selected,
                                    index,
                                    baseTarget,
                                    existingTp2,
                                    market,
                                    atr);

                            if (IsLiveTargetBrokerSafe(
                                    _plan.Direction,
                                    _plan.Entry,
                                    market,
                                    candidateTp2,
                                    atr) &&
                                LiveExitGeometryRule.ShouldAdvanceLiveTarget(
                                    _plan.Direction,
                                    existingTp2,
                                    candidateTp2,
                                    market,
                                    forwardDistance))
                                _plan.Tp2 =
                                    NormalizePrice(candidateTp2);

                            double base2 =
                                _plan.Tp2 > 0
                                    ? _plan.Tp2
                                    : baseTarget;

                            double candidateTp3 =
                                FindFurtherLiveTarget(
                                    selected,
                                    index,
                                    base2,
                                    existingTp3,
                                    market,
                                    atr);

                            if (IsLiveTargetBrokerSafe(
                                    _plan.Direction,
                                    _plan.Entry,
                                    market,
                                    candidateTp3,
                                    atr) &&
                                LiveExitGeometryRule.ShouldAdvanceLiveTarget(
                                    _plan.Direction,
                                    existingTp3,
                                    candidateTp3,
                                    market,
                                    forwardDistance))
                                _plan.Tp3 =
                                    NormalizePrice(candidateTp3);

                            double base3 =
                                _plan.Tp3 > 0
                                    ? _plan.Tp3
                                    : base2;

                            double candidateTp4 =
                                FindFurtherLiveTarget(
                                    selected,
                                    index,
                                    base3,
                                    existingTp4,
                                    market,
                                    atr);

                            if (IsLiveTargetBrokerSafe(
                                    _plan.Direction,
                                    _plan.Entry,
                                    market,
                                    candidateTp4,
                                    atr) &&
                                LiveExitGeometryRule.ShouldAdvanceLiveTarget(
                                    _plan.Direction,
                                    existingTp4,
                                    candidateTp4,
                                    market,
                                    forwardDistance))
                                _plan.Tp4 =
                                    NormalizePrice(candidateTp4);
                
                            ApplyResolvedTargetMeta(
                                selected,
                                1,
                                _plan.Tp2,
                                existingTp2,
                                existingTp2Source,
                                existingTp2Quality,
                                out _plan.Tp2Source,
                                out _plan.Tp2Quality);
                
                            ApplyResolvedTargetMeta(
                                selected,
                                2,
                                _plan.Tp3,
                                existingTp3,
                                existingTp3Source,
                                existingTp3Quality,
                                out _plan.Tp3Source,
                                out _plan.Tp3Quality);
                
                            ApplyResolvedTargetMeta(
                                selected,
                                3,
                                _plan.Tp4,
                                existingTp4,
                                existingTp4Source,
                                existingTp4Quality,
                                out _plan.Tp4Source,
                                out _plan.Tp4Quality);
                
                            _plan.HtfTargetCount =
                                CountHtfTargetsInPlan(
                                    _plan);
                
                            RecalculatePlanRR();
                        }
    }
}
