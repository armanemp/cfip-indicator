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
                
                            double baseTarget = _plan.Tp1;
                
                            _plan.Tp2 =
                                FindFurtherLiveTarget(
                                    selected,
                                    index,
                                    baseTarget,
                                    atr);
                
                            double base2 =
                                _plan.Tp2 > 0
                                    ? _plan.Tp2
                                    : baseTarget;
                
                            _plan.Tp3 =
                                FindFurtherLiveTarget(
                                    selected,
                                    index,
                                    base2,
                                    atr);
                
                            double base3 =
                                _plan.Tp3 > 0
                                    ? _plan.Tp3
                                    : base2;
                
                            _plan.Tp4 =
                                FindFurtherLiveTarget(
                                    selected,
                                    index,
                                    base3,
                                    atr);
                
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
