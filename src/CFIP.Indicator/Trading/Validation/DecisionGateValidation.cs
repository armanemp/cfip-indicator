// ============================================================================
// CFIP Indicator — DecisionGateValidation.cs
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
        private void EnsureSignalPlan(
                                    int closedM5,
                                    DecisionPolicyMode policy)
                                {
                                    if (_plan != null ||
                                        _decision == null ||
                                        _decision.Direction == 0)
                                        return;
                        
                                    if (!ShouldCreatePlan(
                                            closedM5,
                                            policy))
                                        return;
                        
                                    Plan plan =
                                        BuildPlan(
                                            closedM5,
                                            _decision.Direction);
                        
                                    if (plan == null)
                                    {
                                        if (AutoTradingEnabled)
                                        {
                                            SetAutoTradingState(
                                                "BLOCKED",
                                                "PLAN BUILD / STRUCTURAL REWARD GATE");
                                        }
                        
                                        return;
                                    }
                        
                                    _lastAutoPlanAttemptM5 =
                                        closedM5;
                        
                                    ActivatePlan(plan);
                                }
        
        private bool IsHardDecisionBlockReason(
                                    string reason)
                                {
                                    if (string.IsNullOrWhiteSpace(reason))
                                        return false;
                        
                                    switch (reason.Trim().ToUpperInvariant())
                                    {
                                        case "SESSION":
                                        case "FRIDAY":
                                        case "SPREAD":
                                        case "VOLATILITY GUARD":
                                        case "REGIME NO-TRADE":
                                        case "NEWS":
                                        case "COOLDOWN":
                                        case "DIRECTION FLIP":
                                        case "CHOP":
                                        case "M1 MISALIGNMENT":
                                            return true;
                        
                                        default:
                                            return false;
                                    }
                                }
        
        private bool IsLiveExecutionGateReason(
                                    string reason)
                                {
                                    if (string.IsNullOrWhiteSpace(reason))
                                        return false;
                        
                                    switch (reason.Trim().ToUpperInvariant())
                                    {
                                        case "TRIGGER":
                                        case "M5 TRIGGER":
                                        case "ENTRY LOCATION":
                                            return true;
                        
                                        default:
                                            return false;
                                    }
                                }
        
        private bool ShouldCreatePlan(
                                    int closedM5,
                                    DecisionPolicyMode policy)
                                {
                                    if (BlockNewSignalWhileActive &&
                                        _plan != null)
                                        return false;
                        
                                    if (_decision == null ||
                                        _decision.Direction == 0)
                                        return false;
                        
                                    if (policy ==
                                            DecisionPolicyMode.Confirmed)
                                    {
                                        if (!_decision.EntryAllowed)
                                            return false;
                                    }
                                    else
                                    {
                                        if (!_decision.EntryAllowed)
                                        {
                                            bool softEligible =
                                                !IsHardDecisionBlockReason(
                                                    _decision.BlockReason) &&
                                                _decision.Confidence >=
                                                    MinimumAutoConfidence &&
                                                _decision.SmartQuality >=
                                                    MinimumAutoSmartQuality;
                        
                                            if (!softEligible)
                                                return false;
                                        }
                                    }
                        
                                    if (BlockSameBarReentryAfterExit &&
                                        _lastExitM5 == closedM5)
                                        return false;
                        
                                    if (_lastSignalM5 >= 0 &&
                                        closedM5 - _lastSignalM5 <
                                        Math.Max(
                                            CooldownBars,
                                            Math.Max(
                                                CooldownM5Bars,
                                                ExitReentryCooldownM5)))
                                        return false;
                        
                                    return _lastSignalM5 != closedM5;
                                }
        
        private void GetAdaptiveSmartThresholds(
                                    string regime,
                                    out int qualityThreshold,
                                    out int shareThreshold,
                                    out int edgeThreshold)
                                {
                                    qualityThreshold =
                                        Math.Max(
                                            40,
                                            Math.Min(
                                                95,
                                                MinimumSmartQuality));
                        
                                    shareThreshold =
                                        Math.Max(
                                            50,
                                            Math.Min(
                                                90,
                                                MinimumSmartDirectionShare));
                        
                                    edgeThreshold =
                                        Math.Max(
                                            4,
                                            Math.Min(
                                                30,
                                                MinimumEdge));
                        
                                    if (!AdaptiveSmartThresholds)
                                        return;
                        
                                    int b =
                                        Math.Max(
                                            0,
                                            SmartRegimeBuffer);
                        
                                    switch (
                                        regime ??
                                        "UNKNOWN")
                                    {
                                        case "TREND":
                                        case "EXPANSION":
                                            qualityThreshold -= b;
                                            shareThreshold -= Math.Max(1, b / 3);
                                            edgeThreshold -= Math.Max(1, b / 3);
                                            break;
                        
                                        case "REVERSAL":
                                            qualityThreshold -= Math.Max(1, b / 2);
                                            break;
                        
                                        case "RANGE":
                                            qualityThreshold += Math.Max(1, b / 2);
                                            shareThreshold += Math.Max(1, b / 3);
                                            edgeThreshold += Math.Max(1, b / 3);
                                            break;
                        
                                        case "COMPRESSION":
                                            qualityThreshold += b;
                                            shareThreshold += Math.Max(1, b / 2);
                                            edgeThreshold += Math.Max(1, b / 2);
                                            break;
                                    }
                        
                                    qualityThreshold =
                                        Math.Max(
                                            40,
                                            Math.Min(
                                                95,
                                                qualityThreshold));
                        
                                    shareThreshold =
                                        Math.Max(
                                            50,
                                            Math.Min(
                                                90,
                                                shareThreshold));
                        
                                    edgeThreshold =
                                        Math.Max(
                                            4,
                                            Math.Min(
                                                30,
                                                edgeThreshold));
                                }
        
        private int SmartMinimumConsensusFloor()
                                {
                                    return Math.Max(
                                        40,
                                        SmartConsensusThreshold - 12);
                                }
    }
}
