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

                double market =
                    direction == 1
                        ? Symbol.Bid
                        : Symbol.Ask;

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
                    IsValidManagedStop(
                        direction,
                        entry,
                        market,
                        position.StopLoss.Value)
                        ? NormalizePrice(
                            position.StopLoss.Value)
                        : 0;

                double minimumForwardDistance =
                    MinimumLiveTargetDistancePrice(
                        direction,
                        atr);

                double target =
                    position.TakeProfit.HasValue &&
                    IsLiveTargetBrokerSafe(
                        direction,
                        entry,
                        market,
                        position.TakeProfit.Value,
                        atr)
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
                        StructuralStopGeometrySnapshot fallbackGeometry =
                            StructuralStopGeometryRule.EvaluateFallback(
                                direction,
                                entry,
                                atr,
                                Math.Max(0.10, FallbackSlAtr),
                                Symbol.TickSize,
                                Symbol.Digits);

                        stop =
                            fallbackGeometry.IsValid
                                ? fallbackGeometry.Stop
                                : 0;

                        if (IsFinitePositive(stop))
                        {
                            double risk =
                                RiskRewardMathRule.RiskFromLevels(
                                    entry,
                                    stop,
                                    Symbol.PipSize);

                            double riskAtr =
                                risk /
                                Math.Max(
                                    Symbol.PipSize,
                                    atr);

                            double spread =
                                Math.Max(
                                    0,
                                    Symbol.Ask - Symbol.Bid);

                            if (!StructuralStopRiskRule.IsWithinPlanningRiskEnvelope(
                                    riskAtr,
                                    atr,
                                    MinimumSlAtr,
                                    MaximumSlAtr,
                                    MaximumStructuralStopAtr,
                                    spread,
                                    Symbol.PipSize,
                                    MaximumSpreadToStopRiskRatio) ||
                                !IsValidStop(
                                    direction,
                                    entry,
                                    stop))
                                stop = 0;
                        }
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

                    if (!IsLiveTargetBrokerSafe(
                            direction,
                            entry,
                            market,
                            target,
                            atr))
                        target = 0;

                    if (!IsLiveTargetBrokerSafe(
                            direction,
                            entry,
                            market,
                            target,
                            atr))
                    {
                        double risk =
                            RiskRewardMathRule.RiskFromLevels(
                                entry,
                                stop,
                                Symbol.PipSize);

                        double fallbackRR =
                            Math.Max(
                                1.0,
                                MinimumRequiredRR());

                        double maximumRR =
                            Math.Max(
                                1.0,
                                MaximumRewardRR);

                        if (maximumRR < fallbackRR)
                        {
                            target = 0;
                        }
                        else
                        {
                            fallbackRR =
                                Math.Min(
                                    fallbackRR,
                                    maximumRR);

                            double distance =
                                risk *
                                fallbackRR;

                            target =
                                direction == 1
                                    ? entry + distance
                                    : entry - distance;

                            target =
                                NormalizePrice(target);

                            if (!IsLiveTargetBrokerSafe(
                                    direction,
                                    entry,
                                    market,
                                    target,
                                    atr))
                                target = 0;
                        }
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

                int recoveryCreatedM5 =
                    _m5Bars == null
                        ? Math.Max(1, closedM5)
                        : ClosedIndex(
                            _m5Bars,
                            position.EntryTime);

                if (recoveryCreatedM5 < 1 ||
                    (_m5Bars != null &&
                     recoveryCreatedM5 >= _m5Bars.Count))
                    recoveryCreatedM5 =
                        Math.Max(
                            1,
                            Math.Min(
                                closedM5,
                                _m5Bars == null
                                    ? closedM5
                                    : _m5Bars.Count - 2));

                _plan =
                    CreateManagedPlanFromExecution(
                        direction,
                        entry,
                        stop,
                        target,
                        recoveryCreatedM5,
                        position.VolumeInUnits);

                if (_plan == null)
                {
                    _brokerProtectionRecoveryRequired = true;
                    SetLifecycleState(
                        LifecycleState.RecoveryRequired,
                        "STARTUP RECOVERY • PLAN NUMERIC INVALID");
                    continue;
                }

                _plan.PositionId =
                    position.Id;

                _pendingProtectedStopCandidate =
                    0;

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

                double protectionTarget =
                    AutoTarget(
                        _plan,
                        EffectiveAutoTpStage());

                if (!LiveExitGeometryRule.ShouldAdvanceLiveTarget(
                        direction,
                        0,
                        protectionTarget,
                        market,
                        minimumForwardDistance))
                {
                    protectionTarget =
                        FurthestForwardPlanTarget(
                            market,
                            minimumForwardDistance);
                }

                if (AutoProtectBrokerPositions ||
                    AutoBrokerProtection)
                {
                    bool protectionOk =
                        EnsureBrokerProtectionForPosition(
                            position,
                            stop,
                            protectionTarget,
                            "STARTUP RECOVERY",
                            direction);

                    if (protectionOk)
                    {
                        _activeBrokerStop =
                            position.StopLoss.HasValue
                                ? NormalizePrice(position.StopLoss.Value)
                                : 0;

                        AdoptServerSideTakeProfitLadder(position);

                        if (!_serverSideTakeProfitLadderActive)
                        {
                            _activeBrokerTarget =
                                position.TakeProfit.HasValue &&
                                IsFinitePositive(position.TakeProfit.Value)
                                    ? NormalizePrice(position.TakeProfit.Value)
                                    : 0;
                        }

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

                double reconstructedPeak;
                int peakStartM5 =
                    Math.Max(
                        1,
                        recoveryCreatedM5);

                int peakEndM5 =
                    _m5Bars == null
                        ? closedM5
                        : Math.Max(
                            peakStartM5,
                            Math.Min(
                                closedM5,
                                _m5Bars.Count - 2));

                if (_m5Bars != null &&
                    PeakPriceReconstructionRule.TryResolve(
                        direction,
                        entry,
                        _lastMarket,
                        peakStartM5,
                        peakEndM5,
                        i => _m5Bars.HighPrices[i],
                        i => _m5Bars.LowPrices[i],
                        out reconstructedPeak))
                {
                    _peakPrice =
                        NormalizePrice(
                            reconstructedPeak);
                }
                else
                {
                    _peakPrice =
                        _lastMarket;
                }

                break;
            }
        }
    }
}