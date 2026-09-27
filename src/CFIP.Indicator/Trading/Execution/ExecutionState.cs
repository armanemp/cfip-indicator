// ============================================================================
// CFIP Indicator — ExecutionState.cs
// One responsibility per module. Behavioral parity with v73 is preserved.
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
        private void SetAutoTradingState(
                            string state,
                            string reason)
                        {
                            _autoTradingState =
                                string.IsNullOrWhiteSpace(state)
                                    ? "WAIT"
                                    : state.Trim();
                
                            _autoTradingReason =
                                string.IsNullOrWhiteSpace(reason)
                                    ? ""
                                    : reason.Trim();
                        }
        
        private string AutoTradingPanelLine()
                        {
                            if (!AutoTradingEnabled)
                                return
                                    "AUTO TRADING  •  OFF  •  MANUAL REVIEW" +
                                    (AutomaticOrdersEnabled
                                        ? "  •  ORDERS ON"
                                        : "  •  ORDERS OFF");
                
                            string state =
                                string.IsNullOrWhiteSpace(_autoTradingState)
                                    ? "ARMED"
                                    : _autoTradingState;
                
                            return
                                "AUTO TRADING  •  ON  •  " +
                                state +
                                "  •  " +
                                (HasTradingPermission() ? "PERM OK" : "PERM OFF") +
                                "  •  " +
                                (AutomaticOrdersEnabled ? "ORDERS ON" : "ORDERS OFF") +
                                "  •  SMART EXEC " +
                                _marketSuitabilityScore +
                                "/100";
                        }
        
        private Color AutoTradingPanelColor()
                        {
                            if (!AutoTradingEnabled)
                                return PanelMutedTextColor;
                
                            if (string.Equals(
                                    _autoTradingState,
                                    "EXECUTED",
                                    StringComparison.OrdinalIgnoreCase))
                                return TpLineColor;
                
                            if (string.Equals(
                                    _autoTradingState,
                                    "BLOCKED",
                                    StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(
                                    _autoTradingState,
                                    "ERROR",
                                    StringComparison.OrdinalIgnoreCase))
                                return PanelWarningColor;
                
                            return PanelAccentColor;
                        }
        
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
        
        // ============================================================
                
                        private void SetAutoTradingRuntimeState(bool enabled, string reason)
                        {
                            _autoTradingEnabledRuntime = enabled;
                            _autoExecutionBlockReason =
                                string.IsNullOrWhiteSpace(reason)
                                    ? (enabled ? "NOT EVALUATED" : "DISABLED")
                                    : reason;
                            SyncQuickExecutionControls();
                        }
        
        private void SetAutomaticOrdersRuntimeState(bool enabled, string reason)
                        {
                            _automaticOrdersEnabledRuntime = enabled;
                            _autoOrdersBlockReason =
                                string.IsNullOrWhiteSpace(reason)
                                    ? (enabled ? "NOT EVALUATED" : "DISABLED")
                                    : reason;
                            SyncQuickExecutionControls();
                        }
        
        private bool RequestLivePlanExit(
                            int closedM5,
                            string reason)
                        {
                            if (_plan == null ||
                                !_plan.IsLivePosition)
                                return false;
                
                            Position position =
                                GetManagedLivePositionForPlan();
                
                            if (position == null)
                            {
                                _plan = null;
                                _activeBrokerStop = 0;
                                _activeBrokerTarget = 0;
                                _executionModel = null;
                
                                SetLifecycleState(
                                    LifecycleState.Closed,
                                    reason +
                                    " • POSITION ALREADY CLOSED");
                
                                RemovePlanObjects();
                                return true;
                            }
                
                            SetLifecycleState(
                                LifecycleState.ExitRequested,
                                reason);
                
                            if (!TryClosePosition(
                                    position,
                                    reason))
                            {
                                SetLifecycleState(
                                    LifecycleState.RecoveryRequired,
                                    reason +
                                    " • EXIT REJECTED");
                
                                _autoExecutionBlockReason =
                                    reason +
                                    " • EXIT REJECTED";
                
                                return false;
                            }
                
                            _lastExitM5 =
                                Math.Max(
                                    _lastExitM5,
                                    closedM5);
                
                            return true;
                        }
        
        private void SetLifecycleState(
                            LifecycleState state,
                            string reason)
                        {
                            _lifecycleState = state;
                            _lifecycleReason =
                                string.IsNullOrWhiteSpace(reason)
                                    ? state.ToString().ToUpperInvariant()
                                    : reason;
                        }
        
        private Position GetManagedPositionById(long positionId)
                        {
                            if (positionId <= 0)
                                return null;
                
                            foreach (Position position in Positions)
                            {
                                if (position != null &&
                                    position.Id == positionId &&
                                    IsManagedPosition(position))
                                    return position;
                            }
                
                            return null;
                        }
        
        private double AutoTarget(
                            Plan plan,
                            TargetStage stage)
                        {
                            if (stage ==
                                    TargetStage.TP4 &&
                                plan.Tp4 > 0)
                                return plan.Tp4;
                
                            if (stage ==
                                    TargetStage.TP3 &&
                                plan.Tp3 > 0)
                                return plan.Tp3;
                
                            if (stage ==
                                    TargetStage.TP2 &&
                                plan.Tp2 > 0)
                                return plan.Tp2;
                
                            return plan.Tp1;
                        }
        
        private TargetStage EffectiveAutoTpStage()
                        {
                            if (!EnableDynamicTpAdvance ||
                                _plan == null)
                                return AutoTpStage;
                
                            int baseStage =
                                (int)AutoTpStage;
                
                            // A new plan resets the ratchet back to the configured base
                            // stage — this is a per-trade advance, not a permanent state.
                            if (_runtimeTpStagePlanCreatedM5 !=
                                _plan.CreatedM5)
                            {
                                _runtimeTpStagePlanCreatedM5 =
                                    _plan.CreatedM5;
                
                                _runtimeTpStageIndex =
                                    baseStage;
                            }
                
                            if (_runtimeTpStageIndex <
                                baseStage)
                                _runtimeTpStageIndex =
                                    baseStage;
                
                            double risk =
                                Math.Max(
                                    Symbol.PipSize,
                                    _plan.Risk);
                
                            while (_runtimeTpStageIndex < 3)
                            {
                                double currentTarget =
                                    AutoTarget(
                                        _plan,
                                        (TargetStage)
                                        _runtimeTpStageIndex);
                
                                double nextTarget =
                                    AutoTarget(
                                        _plan,
                                        (TargetStage)
                                        (_runtimeTpStageIndex +
                                         1));
                
                                // AutoTarget() falls back to Tp1 for a stage with no valid
                                // price, so confirm the "next" stage is a genuinely farther
                                // level before treating it as something to advance to.
                                bool nextIsFarther =
                                    _plan.Direction == 1
                                        ? nextTarget >
                                          currentTarget
                                        : nextTarget <
                                          currentTarget;
                
                                if (!nextIsFarther)
                                    break;
                
                                double distanceToCurrent =
                                    Math.Abs(
                                        currentTarget -
                                        _plan.Entry);
                
                                if (distanceToCurrent <=
                                    risk * 0.1)
                                    break;
                
                                double covered =
                                    _plan.Direction == 1
                                        ? _lastMarket -
                                          _plan.Entry
                                        : _plan.Entry -
                                          _lastMarket;
                
                                double progressPercent =
                                    covered /
                                    distanceToCurrent *
                                    100.0;
                
                                if (progressPercent <
                                    Math.Max(
                                        50,
                                        TpAdvanceProximityPercent))
                                    break;
                
                                _runtimeTpStageIndex++;
                            }
                
                            return
                                (TargetStage)
                                _runtimeTpStageIndex;
                        }
        
        private void SynchronizeLiveBrokerState()
                        {
                            _activeBrokerStop = 0;
                            _activeBrokerTarget = 0;
                
                            if (_plan == null ||
                                !_plan.IsLivePosition)
                                return;
                
                            Position position =
                                GetManagedLivePositionForPlan();
                
                            if (position == null)
                                return;
                
                            int direction =
                                position.TradeType == TradeType.Buy
                                    ? 1
                                    : -1;
                
                            if (IsFinitePositive(position.EntryPrice))
                                _plan.Entry =
                                    NormalizePrice(position.EntryPrice);
                
                            if (position.StopLoss.HasValue &&
                                IsFinitePositive(position.StopLoss.Value) &&
                                IsValidStop(
                                    direction,
                                    position.EntryPrice,
                                    position.StopLoss.Value))
                            {
                                _activeBrokerStop =
                                    NormalizePrice(position.StopLoss.Value);
                            }
                
                            if (position.TakeProfit.HasValue &&
                                IsFinitePositive(position.TakeProfit.Value))
                            {
                                _activeBrokerTarget =
                                    NormalizePrice(position.TakeProfit.Value);
                            }
                        }
        
        private double GetActiveBrokerStopPrice()
                        {
                            if (_plan != null &&
                                _plan.IsLivePosition &&
                                IsFinitePositive(_activeBrokerStop))
                                return _activeBrokerStop;
                
                            return _plan == null
                                ? 0
                                : _plan.Stop;
                        }
        
        private double GetActiveBrokerTargetPrice()
                        {
                            return
                                _plan != null &&
                                _plan.IsLivePosition &&
                                IsFinitePositive(_activeBrokerTarget)
                                    ? _activeBrokerTarget
                                    : 0;
                        }
        
        private string BrokerTargetStageText(double target)
                        {
                            if (!IsFinitePositive(target) ||
                                _plan == null)
                                return "CUSTOM / NONE";
                
                            if (SamePrice(target, _plan.Tp1))
                                return "TP1";
                            if (SamePrice(target, _plan.Tp2))
                                return "TP2";
                            if (SamePrice(target, _plan.Tp3))
                                return "TP3";
                            if (SamePrice(target, _plan.Tp4))
                                return "TP4";
                
                            return "CUSTOM";
                        }
        
        private string NormalizeLabel()
                        {
                            return
                                string.IsNullOrWhiteSpace(
                                    AutoTradeLabel)
                                    ? "CFIP-SMART66"
                                    : AutoTradeLabel.Trim();
                        }
        
        private void CloseAllPositions()
                        {
                            bool allClosedOrAbsent = true;
                
                            foreach (Position position in Positions)
                            {
                                if (!IsManagedPosition(position))
                                    continue;
                
                                if (!TryClosePosition(
                                        position,
                                        "END OF DAY"))
                                    allClosedOrAbsent = false;
                            }
                
                            if (GetManagedPosition() == null)
                            {
                                _plan = null;
                                _executionModel = null;
                                _activeBrokerStop = 0;
                                _activeBrokerTarget = 0;
                
                                SetLifecycleState(
                                    LifecycleState.Closed,
                                    "END OF DAY • CLOSED");
                
                                RemovePlanObjects();
                            }
                            else if (!allClosedOrAbsent)
                            {
                                SetLifecycleState(
                                    LifecycleState.RecoveryRequired,
                                    "END OF DAY • CLOSE REJECTED");
                            }
                            else
                            {
                                SetLifecycleState(
                                    LifecycleState.ExitRequested,
                                    "END OF DAY • EXIT REQUESTED");
                            }
                        }
        
        private void CancelAllOrders()
                        {
                            bool allCancelledOrAbsent = true;
                
                            foreach (PendingOrder order in PendingOrders)
                            {
                                if (!IsManagedPendingOrder(order))
                                    continue;
                
                                if (!TryCancelPendingOrder(
                                        order,
                                        "PENDING CIRCUIT BREAKER"))
                                    allCancelledOrAbsent = false;
                            }
                
                            if (GetManagedPendingOrder() == null)
                            {
                                RemoveManagedPendingOrderObjects();
                            }
                            else if (!allCancelledOrAbsent)
                            {
                                SetLifecycleState(
                                    LifecycleState.RecoveryRequired,
                                    "PENDING CANCEL REJECTED");
                            }
                        }
    }
}
