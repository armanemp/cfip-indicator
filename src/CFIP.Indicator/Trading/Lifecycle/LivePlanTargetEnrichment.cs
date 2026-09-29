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
                                    atr);
                
                            double market =
                                _plan.Direction == 1
                                    ? Symbol.Bid
                                    : Symbol.Ask;

                            double forwardDistance =
                                Math.Max(
                                    Symbol.PipSize,
                                    atr *
                                    Math.Max(
                                        0.05,
                                        MinimumTpSpacingAtr));

                            double baseTarget =
                                _plan.Tp1;

                            double existingTp2 =
                                _plan.Tp2;

                            double existingTp3 =
                                _plan.Tp3;

                            double existingTp4 =
                                _plan.Tp4;

                            double candidateTp2 =
                                FindFurtherLiveTarget(
                                    selected,
                                    index,
                                    baseTarget,
                                    existingTp2,
                                    market,
                                    atr);

                            if (LiveExitGeometryRule.ShouldAdvanceTarget(
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

                            if (LiveExitGeometryRule.ShouldAdvanceTarget(
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

                            if (LiveExitGeometryRule.ShouldAdvanceTarget(
                                    _plan.Direction,
                                    existingTp4,
                                    candidateTp4,
                                    market,
                                    forwardDistance))
                                _plan.Tp4 =
                                    NormalizePrice(candidateTp4);
                
                            ApplyTargetMeta(
                                levels,
                                _plan.Tp2,
                                atr,
                                out _plan.Tp2Source,
                                out _plan.Tp2Quality);
                
                            ApplyTargetMeta(
                                levels,
                                _plan.Tp3,
                                atr,
                                out _plan.Tp3Source,
                                out _plan.Tp3Quality);
                
                            ApplyTargetMeta(
                                levels,
                                _plan.Tp4,
                                atr,
                                out _plan.Tp4Source,
                                out _plan.Tp4Quality);
                
                            _plan.HtfTargetCount =
                                CountHtfTargetsInPlan(
                                    _plan);
                
                            RecalculatePlanRR();
                        }
    }
}
