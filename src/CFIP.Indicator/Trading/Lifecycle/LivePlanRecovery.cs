// ============================================================================
// CFIP Indicator — LivePlanRecovery.cs
// ============================================================================

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
        
        private double FindFurtherLiveTarget(
                            List<Level> selected,
                            int index,
                            double previous,
                            double atr)
                        {
                            if (selected == null ||
                                !IsFinitePositive(previous) ||
                                _plan == null)
                                return 0;
                
                            double best = 0;
                            double bestScore = double.MinValue;
                
                            foreach (Level level in selected)
                            {
                                if (level == null ||
                                    !IsFinitePositive(level.Price) ||
                                    level.Score < SmartTargetQuality)
                                    continue;
                
                                bool farther =
                                    _plan.Direction == 1
                                        ? level.Price > previous + Symbol.PipSize
                                        : level.Price < previous - Symbol.PipSize;
                
                                if (!farther)
                                    continue;
                
                                double rr =
                                    Math.Abs(
                                        level.Price -
                                        _plan.Entry) /
                                    Math.Max(
                                        Symbol.PipSize,
                                        _plan.Risk);
                
                                if (rr >
                                    Math.Max(
                                        0,
                                        MaximumRewardRR))
                                    continue;
                
                                if (RejectTargetObstacle &&
                                    HasTargetObstacle(
                                        _m5Bars,
                                        index,
                                        _plan.Direction,
                                        _plan.Entry,
                                        level.Price,
                                        atr))
                                    continue;
                
                                double score =
                                    level.Score +
                                    (IsHtfTimeframe(level.Timeframe)
                                        ? HtfRewardBonus
                                        : 0);
                
                                if (score > bestScore)
                                {
                                    bestScore = score;
                                    best = NormalizePrice(level.Price);
                                }
                            }
                
                            return best;
                        }
        
        private Plan CreateManagedPlanFromExecution(
                            int direction,
                            double entry,
                            double stop,
                            double target,
                            int createdM5,
                            double volume,
                            ExecutionMode entryMode =
                                ExecutionMode.BreakoutMarket)
                        {
                            double risk =
                                Math.Abs(
                                    entry -
                                    stop);
                
                            return new Plan
                            {
                                Direction = direction,
                                EntryMode = entryMode,
                                Entry = NormalizePrice(entry),
                                IdealEntry = NormalizePrice(entry),
                                Stop = NormalizePrice(stop),
                                Tp1 = NormalizePrice(target),
                                Tp2 = 0,
                                Tp3 = 0,
                                Tp4 = 0,
                                Risk = Math.Max(
                                    Symbol.PipSize,
                                    risk),
                                Tp1RR =
                                    risk > 0
                                        ? Math.Abs(
                                            target - entry) /
                                          risk
                                        : 0,
                                StopSource = "LIVE / STRUCTURAL",
                                StopQuality = 100,
                                Tp1Source = "LIVE / ADAPTIVE",
                                Tp1Quality = 100,
                                CreatedM5 = createdM5,
                                OriginalVolume = volume,
                                IsLivePosition = true
                            };
                        }
        
        private void RecoverManagedLivePlan(int closedM5)
                        {
                            if (_plan != null &&
                                _plan.IsLivePosition)
                                return;
                
                            foreach (Position position in Positions)
                            {
                                if (!IsManagedPosition(position))
                                    continue;
                
                                int direction =
                                    position.TradeType == TradeType.Buy
                                        ? 1
                                        : -1;
                
                                double entry =
                                    position.EntryPrice;
                
                                double atr =
                                    _m5Bars == null
                                        ? 0
                                        : Atr(
                                            _m5Bars,
                                            Math.Max(
                                                1,
                                                closedM5));
                
                                if (!IsFinitePositive(atr))
                                {
                                    atr =
                                        Math.Max(
                                            Symbol.PipSize * 20,
                                            Math.Abs(
                                                Symbol.Ask -
                                                Symbol.Bid) *
                                        10);
                                }
                
                                double stop =
                                    position.StopLoss.HasValue &&
                                    IsValidStop(
                                        direction,
                                        entry,
                                        position.StopLoss.Value)
                                        ? NormalizePrice(
                                            position.StopLoss.Value)
                                        : 0;
                
                                double target =
                                    position.TakeProfit.HasValue &&
                                    IsValidTarget(
                                        direction,
                                        entry,
                                        position.TakeProfit.Value)
                                        ? NormalizePrice(
                                            position.TakeProfit.Value)
                                        : 0;
                
                                bool protectionMissing =
                                    !IsFinitePositive(stop) ||
                                    !IsFinitePositive(target);
                
                                if (!IsFinitePositive(stop))
                                {
                                    string stopSource;
                                    int stopQuality;
                
                                    stop =
                                        BuildStructuralStop(
                                            Math.Max(1, closedM5),
                                            direction,
                                            entry,
                                            atr,
                                            out stopSource,
                                            out stopQuality);
                
                                    if (!IsValidStop(
                                            direction,
                                            entry,
                                            stop))
                                    {
                                        double fallbackRisk =
                                            atr *
                                            Math.Max(
                                                0.10,
                                                FallbackSlAtr);
                
                                        stop =
                                            direction == 1
                                                ? entry - fallbackRisk
                                                : entry + fallbackRisk;
                
                                        stop =
                                            NormalizePrice(stop);
                                    }
                                }
                
                                if (!IsFinitePositive(target))
                                {
                                    target =
                                        SelectStructuralAutoTarget(
                                            Math.Max(1, closedM5),
                                            direction,
                                            entry,
                                            stop,
                                            atr,
                                            EffectiveAutoTpStage());
                
                                    if (!IsValidTarget(
                                            direction,
                                            entry,
                                            target))
                                    {
                                        double risk =
                                            Math.Max(
                                                Symbol.PipSize,
                                                Math.Abs(
                                                    entry -
                                                    stop));
                
                                        double distance =
                                            risk *
                                            Math.Max(
                                                1.0,
                                                MinimumRequiredRR());
                
                                        target =
                                            direction == 1
                                                ? entry + distance
                                                : entry - distance;
                
                                        target =
                                            NormalizePrice(target);
                                    }
                                }
                
                                if (!IsExecutionPlanConsistent(
                                        direction,
                                        entry,
                                        stop,
                                        target))
                                {
                                    _brokerProtectionRecoveryRequired = true;
                
                                    SetLifecycleState(
                                        LifecycleState.RecoveryRequired,
                                        "STARTUP RECOVERY FAILED");
                                    continue;
                                }
                
                                _plan =
                                    CreateManagedPlanFromExecution(
                                        direction,
                                        entry,
                                        stop,
                                        target,
                                        Math.Max(1, closedM5),
                                        position.VolumeInUnits);
                
                                _plan.PositionId =
                                    position.Id;
                
                                _activeBrokerStop =
                                    position.StopLoss.HasValue
                                        ? NormalizePrice(
                                            position.StopLoss.Value)
                                        : 0;
                
                                _activeBrokerTarget =
                                    position.TakeProfit.HasValue
                                        ? NormalizePrice(
                                            position.TakeProfit.Value)
                                        : 0;
                
                                _brokerProtectionRecoveryRequired =
                                    protectionMissing;
                
                                SetLifecycleState(
                                    protectionMissing
                                        ? LifecycleState.RecoveryRequired
                                        : LifecycleState.LivePosition,
                                    protectionMissing
                                        ? "STARTUP RECOVERY • BROKER PROTECTION MISSING"
                                        : "STARTUP RECOVERY • LIVE");
                
                                EnrichLivePlanTargets(closedM5);
                
                                if (AutoProtectBrokerPositions ||
                                    AutoBrokerProtection)
                                {
                                    bool protectionOk =
                                        EnsureBrokerProtectionForPosition(
                                            position,
                                            stop,
                                            target,
                                            "STARTUP RECOVERY",
                                            direction);
                
                                    if (protectionOk)
                                    {
                                        _activeBrokerStop = stop;
                                        _activeBrokerTarget = target;
                                        _brokerProtectionRecoveryRequired = false;
                
                                        SetLifecycleState(
                                            LifecycleState.LivePosition,
                                            "STARTUP RECOVERY • PROTECTED");
                                    }
                                }
                
                                _lastMarket =
                                    direction == 1
                                        ? Symbol.Bid
                                        : Symbol.Ask;
                
                                _peakPrice =
                                    _lastMarket;
                
                                break;
                            }
                        }
        
        private void CheckAutoTradingDisabledReminder(int closedM5)
                        {
                            if (_decision == null ||
                                _decision.Direction == 0 ||
                                !_decision.EntryAllowed ||
                                _lastAutoTradingReminderM5 == closedM5)
                                return;
                
                            int confidenceFloor =
                                Math.Max(
                                    MinimumAutoConfidence,
                                    HighConfidenceThreshold);
                
                            if (_decision.Confidence < confidenceFloor ||
                                _decision.SmartQuality < MinimumAutoSmartQuality ||
                                _decision.IndependentEvidence < MinimumIndependentEvidence)
                                return;
                
                            _lastAutoTradingReminderM5 =
                                closedM5;
                
                            SetAutoTradingState(
                                "OFF",
                                "HIGH-CONFIDENCE SIGNAL READY");
                
                            ShowPopup(
                                "CFIP SMART\n" +
                                (_decision.Direction == 1 ? "BUY" : "SELL") +
                                " setup is confirmed while Auto Trading is OFF.\n" +
                                "Review ENTRY / SL / TP before taking any manual action.");
                
                            SendUnifiedAlert(
                                "AUTOOFF|" + closedM5,
                                "CFIP AUTO TRADING OFF | " +
                                (_decision.Direction == 1 ? "BUY" : "SELL") +
                                " SIGNAL READY | CONF " +
                                _decision.Confidence +
                                " | SMART " +
                                _decision.SmartQuality,
                                _decision.Direction,
                                true);
                        }
    }
}
