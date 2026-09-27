// ============================================================================
// CFIP Indicator — ExecutionState.cs
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
                                            ? "CFIP-SMART"
                                            : AutoTradeLabel.Trim();
                                }
    }
}
