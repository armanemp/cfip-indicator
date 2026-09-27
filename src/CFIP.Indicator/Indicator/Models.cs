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
        // ============================================================
        
                private sealed class Frame
                {
                    public Bars Bars;
                    public int Index;
                    public double Atr;
                    public double Rsi;
                    public double Adx;
                    public double EmaFast;
                    public double EmaSlow;
                    public int Direction;
                    public int BullScore;
                    public int BearScore;
                    public int Evidence;
                    public int Quality;
                    public bool StructureBull;
                    public bool StructureBear;
                    public bool MssBull;
                    public bool MssBear;
                    public bool ChochBull;
                    public bool ChochBear;
                    public bool DisplacementBull;
                    public bool DisplacementBear;
                    public bool LiquidityBull;
                    public bool LiquidityBear;
                    public bool FvgBull;
                    public bool FvgBear;
                    public bool ObBull;
                    public bool ObBear;
                    public bool TrendBull;
                    public bool TrendBear;
                    public bool MomentumBull;
                    public bool MomentumBear;
                    public bool RejectionBull;
                    public bool RejectionBear;
                    public bool EqualHigh;
                    public bool EqualLow;
                    public bool VolumeBull;
                    public bool VolumeBear;
                    public bool MacdBull;
                    public bool MacdBear;
                    public bool VwapBull;
                    public bool VwapBear;
                    public bool VolatilityBull;
                    public bool VolatilityBear;
                    public bool Choppy;
                }
        
                private sealed class Level
                {
                    public double Price;
                    public double Score;
                    public string Kind;
                    public string Timeframe;
                    public int Age;
                    public int Hits;
                }
        
                private sealed class Zone
                {
                    public double Low;
                    public double High;
                    public int Direction;
                    public string Kind;
                    public int Age;
                    public int Quality;
                }
        
                private sealed class ExecutionIntent
            {
                public int Direction;
                public DecisionPolicyMode Policy;
                public ExecutionIntentKind Kind;
                public double RequestedEntry;
                public double Trigger;
                public double ZoneLow;
                public double ZoneHigh;
                public double Stop;
                public double Target;
                public double StopPips;
                public double TargetPips;
                public double Volume;
                public int CreatedM5;
                public string Source;
            }
        
            private sealed class ExecutionModel
                {
                    public int Direction;
                    public ExecutionMode Mode;
                    public double IdealEntry;
                    public double ActualEntry;
                    public double ZoneLow;
                    public double ZoneHigh;
                    public double Trigger;
                    public double Invalidation;
                    public int Quality;
                    public bool Ready;
                    public string Source;
                }
        
                    private ExecutionIntent BuildExecutionIntent(
                    int direction,
                    DecisionPolicyMode policy,
                    ExecutionIntentKind kind,
                    double entry,
                    double trigger,
                    double zoneLow,
                    double zoneHigh,
                    double stop,
                    double target,
                    double volume,
                    int closedM5,
                    string source)
                {
                    entry = NormalizePrice(entry);
                    stop = NormalizePrice(stop);
                    target = NormalizePrice(target);
        
                    return new ExecutionIntent
                    {
                        Direction = direction,
                        Policy = policy,
                        Kind = kind,
                        RequestedEntry = entry,
                        Trigger = NormalizePrice(trigger),
                        ZoneLow = NormalizePrice(zoneLow),
                        ZoneHigh = NormalizePrice(zoneHigh),
                        Stop = stop,
                        Target = target,
                        StopPips =
                            IsFinitePositive(entry) &&
                            IsFinitePositive(stop)
                                ? Math.Abs(entry - stop) /
                                  Symbol.PipSize
                                : 0,
                        TargetPips =
                            IsFinitePositive(entry) &&
                            IsFinitePositive(target)
                                ? Math.Abs(target - entry) /
                                  Symbol.PipSize
                                : 0,
                        Volume = volume,
                        CreatedM5 = closedM5,
                        Source = source
                    };
                }
        
                private bool ValidateExecutionIntent(
                    ExecutionIntent intent,
                    double liveQuote,
                    out string reason)
                {
                    reason = "OK";
        
                    if (intent == null ||
                        (intent.Direction != 1 &&
                         intent.Direction != -1))
                    {
                        reason = "INVALID EXECUTION INTENT";
                        return false;
                    }
        
                    if (!IsExecutionPlanConsistent(
                            intent.Direction,
                            intent.RequestedEntry,
                            intent.Stop,
                            intent.Target))
                    {
                        reason = "INCONSISTENT EXECUTION INTENT";
                        return false;
                    }
        
                    if (!IsFinitePositive(intent.Volume) ||
                        intent.StopPips <= 0 ||
                        intent.TargetPips <= 0)
                    {
                        reason = "INCOMPLETE EXECUTION INTENT";
                        return false;
                    }
        
                    if (intent.Kind ==
                        ExecutionIntentKind.Market)
                    {
                        if (!IsFinitePositive(liveQuote))
                        {
                            reason = "INVALID MARKET QUOTE";
                            return false;
                        }
                    }
                    else if (intent.Kind ==
                                ExecutionIntentKind.Stop)
                    {
                        if (!SamePrice(
                                intent.RequestedEntry,
                                intent.Trigger) ||
                            !IsValidPendingEntry(
                                intent.Direction,
                                intent.RequestedEntry,
                                true))
                        {
                            reason = "INVALID STOP INTENT";
                            return false;
                        }
                    }
                    else if (intent.Kind ==
                                ExecutionIntentKind.Limit)
                    {
                        if (!IsValidPendingEntry(
                                intent.Direction,
                                intent.RequestedEntry,
                                false))
                        {
                            reason = "INVALID LIMIT INTENT";
                            return false;
                        }
                    }
        
                    return true;
                }
        
                private bool ValidateActualMarketFill(
                    ExecutionIntent intent,
                    double actualFill,
                    double atr,
                    out string reason)
                {
                    reason = "OK";
        
                    if (intent == null ||
                        !IsFinitePositive(actualFill) ||
                        atr <= 0)
                    {
                        reason = "INVALID ACTUAL FILL";
                        return false;
                    }
        
                    if (Math.Abs(
                            actualFill -
                            intent.RequestedEntry) >
                        atr *
                        Math.Max(
                            0.10,
                            MaximumEntryExtensionAtr))
                    {
                        reason = "BROKER FILL FAR FROM INTENT";
                        return false;
                    }
        
                    return true;
                }
        
            private sealed class Prediction
                {
                    public int Direction;
                    public ExecutionMode Mode;
                    public int Confidence;
                    public double Entry;
                    public double StopLoss;
                    public double ZoneLow;
                    public double ZoneHigh;
                    public double Trigger;
                    public double Target;
                    public double Target1;
                    public double Target2;
                    public double Target3;
                    public double Target4;
                    public string Reason;
                }
        
                private sealed class Decision
                {
                    public int Direction;
                    public int Confidence;
                    public int Edge;
                    public int SmartQuality;
                    public int TimeframeAgreement;
                    public int IndependentEvidence;
                    public int StructuralConfirmations;
                    public int RetestQuality;
                    public int BuyShare;
                    public int SellShare;
                    public string Regime;
                    public int RegimeQuality;
                    public bool TriggerReady;
                    public bool EntryAllowed;
                    public string BlockReason;
                    public string Reason;
                }
        
                private sealed class Plan
                {
                    public int Direction;
                    public ExecutionMode EntryMode;
                    public double Entry;
                    public double IdealEntry;
                    public double EntryZoneLow;
                    public double EntryZoneHigh;
                    public double EntryTrigger;
                    public double EntryInvalidation;
                    public int EntryQuality;
                    public string EntrySource;
                    public double Stop;
                    public double Tp1;
                    public double Tp2;
                    public double Tp3;
                    public double Tp4;
                    public double Risk;
                    public double Tp1RR;
                    public double Tp2RR;
                    public double Tp3RR;
                    public double Tp4RR;
                    public int StopQuality;
                    public int Tp1Quality;
                    public int Tp2Quality;
                    public int Tp3Quality;
                    public int Tp4Quality;
                    public string StopSource;
                    public string Tp1Source;
                    public string Tp2Source;
                    public string Tp3Source;
                    public string Tp4Source;
                    public int HtfTargetCount;
                    public int CreatedM5;
                    public double OriginalVolume;
                    public bool IsLivePosition;
                    public long PositionId;
                }
        
                private sealed class Native
                {
                    public Bars Bars;
                    public ExponentialMovingAverage Fast;
                    public ExponentialMovingAverage Slow;
                    public AverageTrueRange Atr;
                    public RelativeStrengthIndex Rsi;
                    public DirectionalMovementSystem Dms;
                    public ExponentialMovingAverage MacdFast;
                    public ExponentialMovingAverage MacdSlow;
                }
        
                // ============================================================
    }
}
