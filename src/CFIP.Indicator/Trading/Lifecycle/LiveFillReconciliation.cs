// CFIP Indicator — LiveFillReconciliation.cs
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
        private void ReconcileLivePlanToActualFill(
                                    Position position,
                                    int closedM5)
                                {
                                    if (_plan == null ||
                                        position == null ||
                                        _m5Bars == null ||
                                        closedM5 < 20)
                                        return;
                        
                                    int direction =
                                        position.TradeType == TradeType.Buy
                                            ? 1
                                            : -1;
                        
                                    double actualEntry =
                                        NormalizePrice(
                                            position.EntryPrice);
                        
                                    if (!IsFinitePositive(actualEntry))
                                        return;
                        
                                    double atr =
                                        Atr(
                                            _m5Bars,
                                            closedM5);
                        
                                    if (atr <= 0)
                                        return;
                        
                                    _plan.Entry =
                                        actualEntry;
                        
                                    // The broker may fill at a slightly different price than the
                                    // executable quote. Rebuild the complete structural ladder from
                                    // the actual fill so chart, plan and broker protection converge.
                                    if (RebuildSmartExecutionLevels(
                                            closedM5,
                                            direction,
                                            actualEntry,
                                            atr))
                                    {
                                        _plan.Entry =
                                            actualEntry;
                        
                                        return;
                                    }
                        
                                    double currentStop =
                                        _plan.Stop;
                        
                                    if (!IsValidStop(
                                            direction,
                                            actualEntry,
                                            currentStop))
                                    {
                                        string stopSource;
                                        int stopQuality;
                        
                                        double rebuiltStop =
                                            BuildStructuralStop(
                                                closedM5,
                                                direction,
                                                actualEntry,
                                                atr,
                                                out stopSource,
                                                out stopQuality);
                        
                                        if (IsFinitePositive(rebuiltStop) &&
                                            IsValidStop(
                                                direction,
                                                actualEntry,
                                                rebuiltStop))
                                        {
                                            currentStop =
                                                rebuiltStop;
                        
                                            _plan.StopSource =
                                                stopSource;
                        
                                            _plan.StopQuality =
                                                stopQuality;
                                        }
                                        else if (position.StopLoss.HasValue &&
                                                 IsValidStop(
                                                     direction,
                                                     actualEntry,
                                                     position.StopLoss.Value))
                                        {
                                            currentStop =
                                                NormalizePrice(
                                                    position.StopLoss.Value);
                        
                                            _plan.StopSource =
                                                "BROKER FILL PROTECTION";
                        
                                            _plan.StopQuality =
                                                70;
                                        }
                                    }
                        
                                    if (!IsValidStop(
                                            direction,
                                            actualEntry,
                                            currentStop))
                                        return;
                        
                                    _plan.Stop =
                                        NormalizePrice(
                                            currentStop);
                        
                                    _plan.Risk =
                                        Math.Max(
                                            Symbol.PipSize,
                                            Math.Abs(
                                                _plan.Entry -
                                                _plan.Stop));
                        
                                    List<Level> levels =
                                        BuildTargetLevels(
                                            closedM5,
                                            direction,
                                            _plan.Entry,
                                            atr);
                        
                                    List<Level> selected =
                                        SelectTargets(
                                            levels,
                                            closedM5,
                                            _plan.Entry,
                                            _plan.Risk,
                                            direction,
                                            atr);
                        
                                    double oldTp1 = _plan.Tp1;
                                    double oldTp2 = _plan.Tp2;
                                    double oldTp3 = _plan.Tp3;
                                    double oldTp4 = _plan.Tp4;
                        
                                    double tp1 =
                                        SelectTarget(
                                            selected,
                                            0,
                                            _plan.Entry,
                                            _plan.Risk,
                                            direction,
                                            Math.Max(
                                                FallbackTp1RR,
                                                MinimumRequiredRR()));
                        
                                    double tp2 =
                                        SelectTarget(
                                            selected,
                                            1,
                                            _plan.Entry,
                                            _plan.Risk,
                                            direction,
                                            Math.Max(
                                                FallbackTp2RR,
                                                Tp2MinimumRR));
                        
                                    double tp3 =
                                        SelectTarget(
                                            selected,
                                            2,
                                            _plan.Entry,
                                            _plan.Risk,
                                            direction,
                                            Math.Max(
                                                FallbackTp3RR,
                                                Tp3MinimumRR));
                        
                                    double tp4 =
                                        SelectTarget(
                                            selected,
                                            3,
                                            _plan.Entry,
                                            _plan.Risk,
                                            direction,
                                            Math.Max(
                                                FallbackTp4RR,
                                                Tp4MinimumRR));
                        
                                    _plan.Tp1 =
                                        IsValidTarget(
                                            direction,
                                            _plan.Entry,
                                            tp1)
                                            ? NormalizePrice(tp1)
                                            : oldTp1;
                        
                                    _plan.Tp2 =
                                        IsValidTarget(
                                            direction,
                                            _plan.Entry,
                                            tp2)
                                            ? NormalizePrice(tp2)
                                            : oldTp2;
                        
                                    _plan.Tp3 =
                                        IsValidTarget(
                                            direction,
                                            _plan.Entry,
                                            tp3)
                                            ? NormalizePrice(tp3)
                                            : oldTp3;
                        
                                    _plan.Tp4 =
                                        IsValidTarget(
                                            direction,
                                            _plan.Entry,
                                            tp4)
                                            ? NormalizePrice(tp4)
                                            : oldTp4;
                        
                                    ApplyTargetMeta(
                                        levels,
                                        _plan.Tp1,
                                        atr,
                                        out _plan.Tp1Source,
                                        out _plan.Tp1Quality);
                        
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
                        
                                    _runtimeTpStageIndex =
                                        -1;
                        
                                    _runtimeTpStagePlanCreatedM5 =
                                        -1;
                        
                                    RecalculatePlanRR();
                                }
    }
}
