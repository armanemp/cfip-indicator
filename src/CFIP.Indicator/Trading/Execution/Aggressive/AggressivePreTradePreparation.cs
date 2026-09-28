// ============================================================================
// CFIP Indicator — AggressiveTradeExecution.cs
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
        private bool TryPrepareAggressiveTrade(
                                    int closedM5,
                                    out TradeType type,
                                    out double entry,
                                    out double atr,
                                    out double stop,
                                    out double target,
                                    out double stopPips,
                                    out double tpPips,
                                    out double volume)
                                {
                                    type = TradeType.Buy;
                                    entry = 0;
                                    atr = 0;
                                    stop = 0;
                                    target = 0;
                                    stopPips = 0;
                                    tpPips = 0;
                                    volume = 0;

                                    if (!AutoTradingEnabled ||
                                        !EnableAggressiveAutoEntry ||
                                        _lifecycleState ==
                                            LifecycleState.ExitRequested ||
                                        _plan != null ||
                                        _reaction == null ||
                                        !_reaction.EntryAllowed ||
                                        _reaction.Direction == 0)
                                        return false;
                                    string capacityReason;
                                    if (!ValidateConfiguredPositionCapacity(
                                            out capacityReason))
                                    {
                                        _autoExecutionBlockReason =
                                            "AGGRESSIVE • " + capacityReason;
                                        SetAutoTradingState(
                                            "BLOCKED",
                                            "AGGRESSIVE • " + capacityReason);
                                        return false;
                                    }
                        
                                    if (GetManagedPendingOrder() != null)
                                    {
                                        _autoExecutionBlockReason =
                                            "PENDING ORDER EXISTS";
                                        return false;
                                    }
                        
                                    if (DailyLossLimitHit(
                                            TimeInUtc))
                                        return false;
                                    if (OneOrderPerSignal &&
                                        _lastAutoM5 ==
                                        closedM5)
                                        return false;
                                    if (_reaction.Confidence <
                                        AggressiveMinimumConfidence ||
                                        _reaction.IndependentEvidence <
                                        AggressiveMinimumEvidence)
                                        return false;
                                    if (AggressiveRequireSmartAgreement &&
                                        (_decision == null ||
                                         _decision.Direction !=
                                         _reaction.Direction ||
                                         _decision.SmartQuality <
                                         AggressiveMinimumSmartQuality))
                                        return false;
                                    string aggressiveSuitabilityReason;
                        
                                    if (!PassesMarketSuitability(
                                            closedM5,
                                            _reaction.Direction,
                                            out aggressiveSuitabilityReason))
                                    {
                                        SetAutoTradingState(
                                            "BLOCKED",
                                            "AGGRESSIVE • " +
                                            aggressiveSuitabilityReason);
                                        return false;
                                    }
                        
                                    if (ManagedPositionCount() >=
                                        Math.Max(
                                            1,
                                            MaximumOpenPositions))
                                        return false;
                                    entry =
                                        NormalizePrice(
                                            _reaction.Direction == 1
                                                ? Symbol.Ask
                                                : Symbol.Bid);
                        
                                    atr =
                                        Atr(
                                            _m5Bars,
                                            closedM5);
                        
                                    if (atr <= 0)
                                    {
                                        _autoExecutionBlockReason =
                                            "AGGRESSIVE • ATR UNAVAILABLE";
                                        return false;
                                    }
                        
                                    string source;
                                    int quality;
                        
                                    stop =
                                        BuildStructuralStop(
                                            closedM5,
                                            _reaction.Direction,
                                            entry,
                                            atr,
                                            out source,
                                            out quality);
                        
                                    if (!IsFinitePositive(stop))
                                    {
                                        _autoExecutionBlockReason =
                                            "AGGRESSIVE • INVALID SL";
                                        return false;
                                    }
                        
                                    target =
                                        SelectStructuralAutoTarget(
                                            closedM5,
                                            _reaction.Direction,
                                            entry,
                                            stop,
                                            atr,
                                            AggressiveTpStage);
                        
                                    if (!IsAutoPlanValid(
                                            _reaction.Direction,
                                            entry,
                                            stop,
                                            target))
                                    {
                                        SetAutoTradingState(
                                            "BLOCKED",
                                            "NO VALID STRUCTURAL TARGET");
                                        return false;
                                    }
                        
                                    stopPips =
                                        Math.Abs(
                                            entry -
                                            stop) /
                                        Symbol.PipSize;
                        
                                    tpPips =
                                        Math.Abs(
                                            target -
                                            entry) /
                                        Symbol.PipSize;
                        
                                    double effectiveStopPips =
                                        stopPips;
                        
                                    if (IncludeSpreadInRiskSizing)
                                        effectiveStopPips +=
                                            Math.Max(
                                                0,
                                                (Symbol.Ask - Symbol.Bid) /
                                                Math.Max(
                                                    Symbol.PipSize,
                                                    1e-9));
                        
                                    volume =
                                        CalculateAggressiveVolume(
                                            effectiveStopPips);
                        
                                    volume =
                                        AdjustVolumeForMargin(
                                            _reaction.Direction == 1
                                                ? TradeType.Buy
                                                : TradeType.Sell,
                                            volume);
                        
                                    if (volume <
                                        Symbol.VolumeInUnitsMin)
                                    {
                                        _autoExecutionBlockReason =
                                            "AGGRESSIVE • VOLUME BELOW MINIMUM";
                                        return false;
                                    }

                                    type =
                                        _reaction.Direction == 1
                                            ? TradeType.Buy
                                            : TradeType.Sell;

                                    return true;
                                }
    }
}
