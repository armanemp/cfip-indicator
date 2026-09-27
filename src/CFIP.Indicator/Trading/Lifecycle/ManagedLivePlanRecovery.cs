// CFIP Indicator — ManagedLivePlanRecovery.cs
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
    }
}
