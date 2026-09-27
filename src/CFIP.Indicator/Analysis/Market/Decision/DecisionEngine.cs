// ============================================================================
// CFIP Indicator — DecisionEngine.cs
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
        // DECISION
                        
                                // Automated decision safety invariant: all automated decision inputs
                                // come from fully closed bars at a single M5 UTC reference. The chart
                                // confluence index is also closed at that same reference.
                                private Decision BuildDecision(
                                    int chartIndex,
                                    int closedM5,
                                    DateTime reference)
                                {
                                    Decision d =
                                        new Decision();
                        
                                    double buy = 0;
                                    double sell = 0;
                                    int evidence = 0;
                        
                                    AddFrame(_m5Frame, M5Weight, ref buy, ref sell, ref evidence);
                                    AddFrame(_m15Frame, M15Weight, ref buy, ref sell, ref evidence);
                                    AddFrame(_m30Frame, M30Weight, ref buy, ref sell, ref evidence);
                                    AddFrame(_h1Frame, H1Weight, ref buy, ref sell, ref evidence);
                                    AddFrame(_h4Frame, H4Weight, ref buy, ref sell, ref evidence);
                                    AddFrame(_d1Frame, D1Weight, ref buy, ref sell, ref evidence);
                        
                                    if (SmartWeeklyContext)
                                        AddFrame(_w1Frame, W1Weight, ref buy, ref sell, ref evidence);
                        
                                    if (UseAdvancedConfluence)
                                    {
                                        int closedChartIndex =
                                            MapM5ToClosedChart(
                                                closedM5,
                                                chartIndex);
                        
                                        buy +=
                                            LiveBias(
                                                closedChartIndex,
                                                1);
                        
                                        sell +=
                                            LiveBias(
                                                closedChartIndex,
                                                -1);
                                    }
                        
                                    if (UsePremiumDiscount)
                                    {
                                        int pd =
                                            PremiumDiscountBias(
                                                _m5Bars,
                                                closedM5);
                        
                                        if (pd == 1)
                                            buy += 6;
                                        else if (pd == -1)
                                            sell += 6;
                                    }
                        
                                    if (UseM1Trigger &&
                                        _m1Frame != null)
                                    {
                                        if (_m1Frame.Direction == 1)
                                            buy += 3;
                                        else if (_m1Frame.Direction == -1)
                                            sell += 3;
                                    }
                        
                                    string regime =
                                        DetectRegime(
                                            _m5Bars,
                                            closedM5);
                        
                                    if (AdaptiveRegimeWeighting &&
                                        _m5Frame != null)
                                    {
                                        if (regime == "EXPANSION")
                                        {
                                            if (_m5Frame.DisplacementBull)
                                                buy += 4;
                        
                                            if (_m5Frame.DisplacementBear)
                                                sell += 4;
                        
                                            if (_m5Frame.VolumeBull)
                                                buy += 2;
                        
                                            if (_m5Frame.VolumeBear)
                                                sell += 2;
                                        }
                                        else if (regime == "RANGE" ||
                                                 regime == "TRANSITION")
                                        {
                                            if (_m5Frame.LiquidityBull)
                                                buy += 3;
                        
                                            if (_m5Frame.LiquidityBear)
                                                sell += 3;
                        
                                            if (_m5Frame.FvgBull)
                                                buy += 2;
                        
                                            if (_m5Frame.FvgBear)
                                                sell += 2;
                        
                                            if (_m5Frame.ObBull)
                                                buy += 2;
                        
                                            if (_m5Frame.ObBear)
                                                sell += 2;
                                        }
                                        else if (regime == "COMPRESSION")
                                        {
                                            buy *= 0.95;
                                            sell *= 0.95;
                                        }
                                    }
                        
                                    if (UseHistoricalChoppinessGuard &&
                                        _m5Frame != null &&
                                        _m15Frame != null &&
                                        _m5Frame.Choppy &&
                                        _m15Frame.Choppy)
                                    {
                                        buy *= 0.90;
                                        sell *= 0.90;
                                    }
                        
                                    double temperature =
                                        Math.Max(
                                            1.0,
                                            SmartScoreTemperature);
                        
                                    double centered =
                                        (buy - sell) /
                                        temperature;
                        
                                    double expBuy =
                                        Math.Exp(
                                            Clamp(
                                                centered,
                                                -12,
                                                12));
                        
                                    double expSell =
                                        Math.Exp(
                                            Clamp(
                                                -centered,
                                                -12,
                                                12));
                        
                                    double total =
                                        Math.Max(
                                            1e-9,
                                            expBuy + expSell);
                        
                                    int buyShare =
                                        ClampInt(
                                            (int)Math.Round(
                                                100.0 *
                                                expBuy /
                                                total),
                                            0,
                                            100);
                        
                                    int sellShare =
                                        100 -
                                        buyShare;
                        
                                    d.BuyShare = buyShare;
                                    d.SellShare = sellShare;
                        
                                    int strongestShare =
                                        Math.Max(
                                            buyShare,
                                            sellShare);
                        
                                    d.Direction =
                                        strongestShare >=
                                        Math.Max(
                                            50,
                                            MinimumSmartDirectionShare)
                                            ? (buyShare >= sellShare
                                                ? 1
                                                : -1)
                                            : 0;
                        
                                    d.Edge =
                                        Math.Abs(
                                            buyShare -
                                            sellShare);
                        
                                    d.TimeframeAgreement =
                                        TimeframeAgreement(
                                            d.Direction,
                                            reference);
                        
                                    d.IndependentEvidence =
                                        IndependentEvidence(
                                            d.Direction);
                        
                                    d.StructuralConfirmations =
                                        StructuralConfirmations(
                                            d.Direction);
                        
                                    d.Regime = regime;
                        
                                    d.RegimeQuality =
                                        RegimeQuality(
                                            regime,
                                            _m5Bars,
                                            closedM5);
                        
                                    d.SmartQuality =
                                        ClampInt(
                                            (int)Math.Round(
                                                strongestShare * 0.28 +
                                                d.TimeframeAgreement * 0.23 +
                                                Math.Min(
                                                    100,
                                                    d.IndependentEvidence * 10) * 0.20 +
                                                Math.Min(
                                                    100,
                                                    d.StructuralConfirmations * 16) * 0.17 +
                                                d.RegimeQuality * 0.12),
                                            0,
                                            100);
                        
                                    if (d.Direction == 0)
                                    {
                                        d.Confidence =
                                            strongestShare;
                        
                                        d.TriggerReady = false;
                                        d.EntryAllowed = false;
                                        d.BlockReason =
                                            "SMART CONSENSUS";
                        
                                        d.Reason =
                                            "NEUTRAL | BUY " +
                                            buyShare +
                                            " | SELL " +
                                            sellShare;
                        
                                        return d;
                                    }
                        
                                    d.Confidence =
                                        CalibratedConfidence(
                                            ClampInt(
                                                (int)Math.Round(
                                                    strongestShare * 0.45 +
                                                    d.TimeframeAgreement * 0.25 +
                                                    d.SmartQuality * 0.30),
                                                0,
                                                100),
                                            d.Direction);
                        
                                    if (HigherTfPenalty > 0)
                                    {
                                        bool h1Against =
                                            _h1Frame != null &&
                                            _h1Frame.Direction != 0 &&
                                            _h1Frame.Direction !=
                                            d.Direction;
                        
                                        bool h4Against =
                                            _h4Frame != null &&
                                            _h4Frame.Direction != 0 &&
                                            _h4Frame.Direction !=
                                            d.Direction;
                        
                                        // ENHANCEMENT (signal accuracy): a D1-opposed trade is the
                                        // single riskiest disagreement a professional would flag —
                                        // daily structure was previously never checked here, so a
                                        // setup could pass with H1/H4 aligned while fighting the
                                        // daily trend outright. D1 opposition now applies the same
                                        // penalty, plus an extra half-penalty since it is the more
                                        // consequential disagreement of the three.
                                        bool d1Against =
                                            _d1Frame != null &&
                                            _d1Frame.Direction != 0 &&
                                            _d1Frame.Direction !=
                                            d.Direction;
                        
                                        if (h1Against ||
                                            h4Against ||
                                            d1Against)
                                        {
                                            int penalty =
                                                HigherTfPenalty +
                                                (d1Against
                                                    ? HigherTfPenalty / 2
                                                    : 0);
                        
                                            d.Confidence =
                                                ClampInt(
                                                    d.Confidence -
                                                    penalty,
                                                    0,
                                                    100);
                                        }
                                    }
                        
                                    d.RetestQuality =
                                        RetestQuality(
                                            _m5Bars,
                                            closedM5,
                                            d.Direction);
                        
                                    d.TriggerReady =
                                        ClosedBarTriggerReady(
                                            _m5Bars,
                                            closedM5,
                                            d.Direction);
                        
                                    d.EntryAllowed =
                                        PassesDecisionFilters(
                                            chartIndex,
                                            closedM5,
                                            reference,
                                            d,
                                            out d.BlockReason);
                        
                                    d.Reason =
                                        BuildReason(
                                            d,
                                            buyShare,
                                            sellShare);
                        
                                    return d;
                                }
    }
}
