// ============================================================================
// CFIP Indicator — DecisionFilters.cs
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
        private bool PassesDecisionFilters(
                                    int chartIndex,
                                    int closedM5,
                                    DateTime reference,
                                    Decision d,
                                    out string reason)
                                {
                                    reason = "";
                        
                                    if (d == null ||
                                        d.Direction == 0)
                                    {
                                        reason = "NO DIRECTION";
                                        return false;
                                    }

                                    int adaptiveQualityThreshold;
                                    int adaptiveShareThreshold;
                                    int adaptiveEdgeThreshold;

                                    GetAdaptiveSmartThresholds(
                                        d.Regime,
                                        out adaptiveQualityThreshold,
                                        out adaptiveShareThreshold,
                                        out adaptiveEdgeThreshold);

                                    DecisionFilterResult thresholdResult =
                                        new DecisionThresholdFilterEvaluator().Evaluate(
                                            d,
                                            new DecisionThresholdFilterInput(
                                                d.Confidence,
                                                d.Edge,
                                                d.SmartQuality,
                                                d.TimeframeAgreement,
                                                d.IndependentEvidence,
                                                d.StructuralConfirmations,
                                                MinimumConfidence,
                                                adaptiveEdgeThreshold,
                                                adaptiveQualityThreshold,
                                                RequireHigherTfAgreement,
                                                MinimumTimeframeAgreement,
                                                MinimumIndependentEvidence,
                                                SmartMinimumIndependentEvidence,
                                                EnableSmartDecisionEngine,
                                                RequireStructuralConfirmation,
                                                MinimumStructuralConfirmations));

                                    if (!thresholdResult.Allowed)
                                    {
                                        reason = thresholdResult.Reason;
                                        return false;
                                    }

                                    if (RequireCoreAgreement &&
                                        _m5Frame != null &&
                                        _m15Frame != null)
                                    {
                                        bool aligned =
                                            d.Direction == 1
                                                ? _m5Frame.Direction == 1 &&
                                                  (_m15Frame.Direction == 1 ||
                                                   (_m15Frame.Direction == 0 &&
                                                    AllowM15NeutralPullback))
                                                : _m5Frame.Direction == -1 &&
                                                  (_m15Frame.Direction == -1 ||
                                                   (_m15Frame.Direction == 0 &&
                                                    AllowM15NeutralPullback));
                        
                                        if (!aligned)
                                        {
                                            reason = "CORE ALIGNMENT";
                                            return false;
                                        }
                                    }
                        
                                    if (UseM5Confirmation &&
                                        (_m5Frame == null ||
                                         _m5Frame.Direction !=
                                         d.Direction))
                                    {
                                        reason = "M5 CONFIRMATION";
                                        return false;
                                    }
                        
                                    if (M5OnlyConfirmedTrigger &&
                                        !ClosedBarTriggerReady(
                                            _m5Bars,
                                            closedM5,
                                            d.Direction))
                                    {
                                        bool directOverride =
                                            AllowDirectDisplacementOverride &&
                                            d.Confidence >=
                                            SmartStrongSetupQuality &&
                                            d.Edge >=
                                            DirectDisplacementOverrideScore &&
                                            _m5Frame != null &&
                                            (d.Direction == 1
                                                ? _m5Frame.DisplacementBull
                                                : _m5Frame.DisplacementBear);
                        
                                        bool strongOverride =
                                            AllowStrongTriggerOverride &&
                                            AllowStrongM5TriggerOverride &&
                                            d.Confidence >= 85 &&
                                            d.Edge >= 20;
                        
                                        if (!(directOverride ||
                                              strongOverride))
                                        {
                                            reason = "M5 TRIGGER";
                                            return false;
                                        }
                                    }
                        
                                    // ENHANCEMENT (day-trading / M5 trigger alignment): M1 Trigger
                                    // now defaults OFF. The actual trigger evaluation (candle
                                    // pattern score + precision entry model, gated by
                                    // M5OnlyConfirmedTrigger) already runs entirely on M5 bars —
                                    // M1 only ever added a +3 confluence bias and this veto. For a
                                    // stated M5-trigger, day-trading workflow that veto was mostly
                                    // reacting to 1-minute noise the M5 close hadn't confirmed yet.
                                    // Still available for anyone who wants the extra M1 filter —
                                    // just off by default now.
                                    if (UseM1Trigger &&
                                        _m1Frame != null &&
                                        _m1Frame.Direction != 0 &&
                                        _m1Frame.Direction !=
                                        d.Direction)
                                    {
                                        reason = "M1 MISALIGNMENT";
                                        return false;
                                    }
                        
                                    DecisionFilterResult smartConsensusResult =
                                        new DecisionSmartConsensusFilterEvaluator().Evaluate(
                                            new DecisionSmartConsensusFilterInput(
                                                EnableSmartDecisionEngine,
                                                RequireSmartConsensus,
                                                Math.Max(d.BuyShare, d.SellShare),
                                                SmartConsensusThreshold,
                                                adaptiveShareThreshold,
                                                AllowSmartSoftGate,
                                                d.SmartQuality,
                                                SmartStrongSetupQuality,
                                                d.Edge,
                                                SmartStrongSetupEdge,
                                                d.IndependentEvidence,
                                                SmartMinimumIndependentEvidence,
                                                d.TimeframeAgreement,
                                                SmartMinimumTimeframeAgreement));

                                    if (!smartConsensusResult.Allowed)
                                    {
                                        reason = smartConsensusResult.Reason;
                                        return false;
                                    }

                                    if (UseSmartEntryQualityFilter &&
                                        d.SmartQuality <
                                        Math.Max(
                                            SmartQualityThreshold,
                                            Math.Max(
                                                adaptiveQualityThreshold,
                                                EnableSmartDecisionEngine
                                                    ? SmartMinimumConsensusFloor()
                                                    : 0)))
                                    {
                                        reason = "SMART QUALITY";
                                        return false;
                                    }
                        
                                    if (UseStructuralSequenceGate &&
                                        StructuralSequence(
                                            _m5Bars,
                                            closedM5,
                                            d.Direction) <
                                        MinimumStructuralSequence)
                                    {
                                        reason = "STRUCTURAL SEQUENCE";
                                        return false;
                                    }
                        
                                    if (UseZoneConfluence &&
                                        RequireEntryLocationConfluence &&
                                        EntryLocationQuality(
                                            _m5Bars,
                                            closedM5,
                                            d.Direction) <
                                        MinimumEntryLocationQuality)
                                    {
                                        reason = "ENTRY LOCATION";
                                        return false;
                                    }
                        
                                    if (UseProxyExpectedValueGate &&
                                        ProxyExpectedValue(
                                            d.SmartQuality,
                                            Math.Max(
                                                1.0,
                                                SmartTargetMinimumRR)) <
                                        MinimumProxyExpectedValue)
                                    {
                                        reason = "EXPECTED VALUE";
                                        return false;
                                    }
                        
                                    if (UseRegimeNoTradeGuard &&
                                        NoTradeRegimeBlocked(
                                            d.Regime,
                                            d.SmartQuality))
                                    {
                                        reason = "REGIME NO-TRADE";
                                        return false;
                                    }
                        
                                    if (UseHistoricalChoppinessGuard &&
                                        _m5Frame != null &&
                                        _m15Frame != null &&
                                        _m5Frame.Choppy &&
                                        _m15Frame.Choppy &&
                                        d.SmartQuality <
                                        Math.Max(
                                            SmartRegimeQualityFloor + 5,
                                            NoTradeMinimumSmartQuality + 5))
                                    {
                                        reason = "CHOP";
                                        return false;
                                    }
                        
                                    if (RequireStableM5Direction &&
                                        !StableDirection(
                                            _m5Bars,
                                            closedM5,
                                            d.Direction,
                                            StableM5Bars))
                                    {
                                        reason = "M5 STABILITY";
                                        return false;
                                    }
                        
                                    int m15Closed =
                                        ClosedIndex(
                                            _m15Bars,
                                            reference);
                        
                                    if (RequireStableM15Direction &&
                                        m15Closed >=
                                        StableM15Bars + 5 &&
                                        !StableDirection(
                                            _m15Bars,
                                            m15Closed,
                                            d.Direction,
                                            StableM15Bars))
                                    {
                                        reason = "M15 STABILITY";
                                        return false;
                                    }
                        
                                    if (RequireRetestQuality &&
                                        d.RetestQuality <
                                        MinimumRetestQuality)
                                    {
                                        bool retestOverride =
                                            AllowStrongTriggerOverride &&
                                            d.Confidence >= 85 &&
                                            d.Edge >= 20 &&
                                            d.IndependentEvidence >=
                                            MinimumIndependentEvidence + 1;
                        
                                        if (!retestOverride)
                                        {
                                            reason = "RETEST QUALITY";
                                            return false;
                                        }
                                    }
                        
                                    if (!d.TriggerReady)
                                    {
                                        reason = "TRIGGER";
                                        return false;
                                    }
                        
                                    if (!SessionAllowed(
                                            TimeInUtc))
                                    {
                                        reason = "SESSION";
                                        return false;
                                    }
                        
                                    if (!FridayAllowed(
                                            TimeInUtc))
                                    {
                                        reason = "FRIDAY";
                                        return false;
                                    }
                        
                                    if (!SpreadAllowed(
                                            _m5Bars,
                                            closedM5))
                                    {
                                        reason = "SPREAD";
                                        return false;
                                    }
                        
                                    if ((UseVolatilityGuard ||
                                         UseVolatilityEventGuard) &&
                                        VolatilityBlocked(
                                            _m5Bars,
                                            closedM5))
                                    {
                                        reason = "VOLATILITY GUARD";
                                        return false;
                                    }
                        
                                    if (UseNewsEventGuard &&
                                        NewsBlocked(
                                            TimeInUtc,
                                            out reason))
                                        return false;
                        
                                    if (!CanAcceptConfirmedDirection(
                                            d.Direction,
                                            closedM5))
                                    {
                                        reason = "DIRECTION FLIP";
                                        return false;
                                    }
                        
                                    if (CooldownBlocked(
                                            closedM5))
                                    {
                                        reason = "COOLDOWN";
                                        return false;
                                    }
                        
                                    return true;
                                }
        

    }
}
