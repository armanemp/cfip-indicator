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
        private Frame AnalyzeFrame(
                    Bars bars,
                    int index)
                {
                    Frame f =
                        new Frame
                        {
                            Bars = bars,
                            Index = index
                        };
        
                    if (bars == null ||
                        index < 30 ||
                        index >= bars.Count)
                        return f;
        
                    f.Atr = Atr(bars, index);
                    f.Rsi = Rsi(bars, index);
                    f.Adx = Adx(bars, index);
                    f.EmaFast = Ema(bars, index, true);
                    f.EmaSlow = Ema(bars, index, false);
        
                    f.StructureBull =
                        UseInternalStructure &&
                        BullStructure(
                            bars,
                            index,
                            f.Atr);
        
                    f.StructureBear =
                        UseInternalStructure &&
                        BearStructure(
                            bars,
                            index,
                            f.Atr);
        
                    f.MssBull =
                        UseMssChoch &&
                        BullMss(
                            bars,
                            index,
                            f.Atr);
        
                    f.MssBear =
                        UseMssChoch &&
                        BearMss(
                            bars,
                            index,
                            f.Atr);
        
                    f.ChochBull =
                        UseMssChoch &&
                        BullChoch(
                            bars,
                            index);
        
                    f.ChochBear =
                        UseMssChoch &&
                        BearChoch(
                            bars,
                            index);
        
                    f.DisplacementBull =
                        UseDisplacement &&
                        BullDisplacement(
                            bars,
                            index,
                            f.Atr);
        
                    f.DisplacementBear =
                        UseDisplacement &&
                        BearDisplacement(
                            bars,
                            index,
                            f.Atr);
        
                    f.LiquidityBull =
                        UseLiquiditySweep &&
                        BullLiquiditySweep(
                            bars,
                            index,
                            f.Atr);
        
                    f.LiquidityBear =
                        UseLiquiditySweep &&
                        BearLiquiditySweep(
                            bars,
                            index,
                            f.Atr);
        
                    f.FvgBull =
                        UseFvg &&
                        FindNearestFvg(
                            bars,
                            index,
                            1,
                            f.Atr) != null;
        
                    f.FvgBear =
                        UseFvg &&
                        FindNearestFvg(
                            bars,
                            index,
                            -1,
                            f.Atr) != null;
        
                    f.ObBull =
                        UseOrderBlock &&
                        FindNearestOrderBlock(
                            bars,
                            index,
                            1,
                            f.Atr) != null;
        
                    f.ObBear =
                        UseOrderBlock &&
                        FindNearestOrderBlock(
                            bars,
                            index,
                            -1,
                            f.Atr) != null;
        
                    f.TrendBull =
                        f.EmaFast > f.EmaSlow &&
                        bars.ClosePrices[index] >
                        f.EmaFast;
        
                    f.TrendBear =
                        f.EmaFast < f.EmaSlow &&
                        bars.ClosePrices[index] <
                        f.EmaFast;
        
                    f.MomentumBull =
                        Momentum(
                            bars,
                            index,
                            1,
                            f.Atr);
        
                    f.MomentumBear =
                        Momentum(
                            bars,
                            index,
                            -1,
                            f.Atr);
        
                    f.RejectionBull =
                        Rejection(
                            bars,
                            index,
                            1);
        
                    f.RejectionBear =
                        Rejection(
                            bars,
                            index,
                            -1);
        
                    f.VolumeBull =
                        HasVolumeExpansion(
                            bars,
                            index,
                            1);
        
                    f.VolumeBear =
                        HasVolumeExpansion(
                            bars,
                            index,
                            -1);
        
                    f.MacdBull =
                        HasMacdBias(
                            bars,
                            index,
                            1);
        
                    f.MacdBear =
                        HasMacdBias(
                            bars,
                            index,
                            -1);
        
                    f.VwapBull =
                        HasVwapBias(
                            bars,
                            index,
                            1);
        
                    f.VwapBear =
                        HasVwapBias(
                            bars,
                            index,
                            -1);
        
                    f.VolatilityBull =
                        HasHealthyVolatility(
                            bars,
                            index,
                            1);
        
                    f.VolatilityBear =
                        HasHealthyVolatility(
                            bars,
                            index,
                            -1);
        
                    f.Choppy =
                        UseHistoricalChoppinessGuard &&
                        f.Adx < AdxMinimum &&
                        Math.Abs(
                            f.EmaFast -
                            f.EmaSlow) <
                        f.Atr * 0.35;
        
                    f.EqualHigh =
                        UseEqualHighLow &&
                        FindEqualHigh(
                            bars,
                            index,
                            bars.ClosePrices[index],
                            f.Atr) > 0;
        
                    f.EqualLow =
                        UseEqualHighLow &&
                        FindEqualLow(
                            bars,
                            index,
                            bars.ClosePrices[index],
                            f.Atr) > 0;
        
                    int bull = 0;
                    int bear = 0;
                    int evidence = 0;
        
                    AddScore(f.StructureBull, 16, ref bull, ref evidence);
                    AddScore(f.StructureBear, 16, ref bear, ref evidence);
                    AddScore(f.MssBull, 12, ref bull, ref evidence);
                    AddScore(f.MssBear, 12, ref bear, ref evidence);
                    AddScore(f.ChochBull, 9, ref bull, ref evidence);
                    AddScore(f.ChochBear, 9, ref bear, ref evidence);
                    AddScore(f.DisplacementBull, 10, ref bull, ref evidence);
                    AddScore(f.DisplacementBear, 10, ref bear, ref evidence);
                    AddScore(f.LiquidityBull, 10, ref bull, ref evidence);
                    AddScore(f.LiquidityBear, 10, ref bear, ref evidence);
                    AddScore(f.FvgBull, 8, ref bull, ref evidence);
                    AddScore(f.FvgBear, 8, ref bear, ref evidence);
                    AddScore(f.ObBull, 9, ref bull, ref evidence);
                    AddScore(f.ObBear, 9, ref bear, ref evidence);
                    AddScore(f.TrendBull, 10, ref bull, ref evidence);
                    AddScore(f.TrendBear, 10, ref bear, ref evidence);
                    AddScore(f.MomentumBull, 8, ref bull, ref evidence);
                    AddScore(f.MomentumBear, 8, ref bear, ref evidence);
                    AddScore(f.RejectionBull, 6, ref bull, ref evidence);
                    AddScore(f.RejectionBear, 6, ref bear, ref evidence);
                    AddScore(
                        f.VolumeBull,
                        3,
                        ref bull,
                        ref evidence,
                        UseVolumeExpansionEvidence);
                    AddScore(
                        f.VolumeBear,
                        3,
                        ref bear,
                        ref evidence,
                        UseVolumeExpansionEvidence);
                    AddScore(
                        f.MacdBull,
                        3,
                        ref bull,
                        ref evidence,
                        UseMacdEvidence);
                    AddScore(
                        f.MacdBear,
                        3,
                        ref bear,
                        ref evidence,
                        UseMacdEvidence);
                    AddScore(
                        f.VwapBull,
                        2,
                        ref bull,
                        ref evidence,
                        UseVwapEvidence);
                    AddScore(
                        f.VwapBear,
                        2,
                        ref bear,
                        ref evidence,
                        UseVwapEvidence);
                    AddScore(
                        f.VolatilityBull,
                        2,
                        ref bull,
                        ref evidence,
                        UseHealthyVolatilityEvidence);
                    AddScore(
                        f.VolatilityBear,
                        2,
                        ref bear,
                        ref evidence,
                        UseHealthyVolatilityEvidence);
                    AddScore(f.EqualLow, 5, ref bull, ref evidence);
                    AddScore(f.EqualHigh, 5, ref bear, ref evidence);
        
                    if (f.Rsi > 50)
                        bull += 3;
                    else if (f.Rsi < 50)
                        bear += 3;
        
                    if (f.Adx >= AdxMinimum)
                    {
                        double dmi =
                            DmiBias(
                                bars,
                                index);
        
                        if (dmi > 0)
                            bull += 4;
                        else if (dmi < 0)
                            bear += 4;
                    }
        
                    if (UseEmaSlope &&
                        index > 2)
                    {
                        double previous =
                            Ema(
                                bars,
                                index - 2,
                                true);
        
                        if (f.EmaFast > previous)
                            bull += 3;
                        else if (f.EmaFast < previous)
                            bear += 3;
                    }
        
                    if (AvoidRsiExhaustion)
                    {
                        if (f.Rsi >= 75)
                            bull = Math.Max(
                                0,
                                bull - 5);
        
                        if (f.Rsi <= 25)
                            bear = Math.Max(
                                0,
                                bear - 5);
                    }
        
                    f.BullScore = bull;
                    f.BearScore = bear;
                    f.Evidence = evidence;
        
                    if (bull >= 35 &&
                        bull >= bear + 8)
                        f.Direction = 1;
                    else if (bear >= 35 &&
                             bear >= bull + 8)
                        f.Direction = -1;
        
                    double total =
                        Math.Max(
                            1,
                            bull + bear);
        
                    double strongest =
                        100.0 *
                        Math.Max(
                            bull,
                            bear) /
                        total;
        
                    f.Quality =
                        ClampInt(
                            (int)Math.Round(
                                strongest * 0.50 +
                                Math.Min(
                                    100,
                                    f.Adx * 1.5) * 0.15 +
                                Math.Min(
                                    100,
                                    evidence * 5) * 0.25 +
                                (f.Choppy ? 0 : 10) * 0.10),
                            0,
                            100);
        
                    return f;
                }
        
                private void AddScore(
                    bool condition,
                    int score,
                    ref int total,
                    ref int evidence)
                {
                    if (!condition)
                        return;
        
                    total += score;
                    evidence++;
                }
        
                private void AddScore(
                    bool condition,
                    int score,
                    ref int total,
                    ref int evidence,
                    bool countAsEvidence)
                {
                    if (!condition)
                        return;
        
                    total += score;
        
                    if (countAsEvidence)
                        evidence++;
                }
        
                private bool HasVolumeExpansion(
                    Bars bars,
                    int index,
                    int direction)
                {
                    if (!UseVolumeExpansion ||
                        bars == null ||
                        index < 25)
                        return false;
        
                    double average = 0;
                    int count = 0;
                    int first =
                        Math.Max(
                            0,
                            index - 20);
        
                    for (int i = first;
                         i < index;
                         i++)
                    {
                        average +=
                            Math.Max(
                                0,
                                bars.TickVolumes[i]);
        
                        count++;
                    }
        
                    if (count == 0 ||
                        average <= 0)
                        return false;
        
                    average /= count;
        
                    bool directional =
                        direction == 1
                            ? bars.ClosePrices[index] >
                              bars.OpenPrices[index]
                            : bars.ClosePrices[index] <
                              bars.OpenPrices[index];
        
                    return
                        directional &&
                        bars.TickVolumes[index] >=
                        average *
                        Math.Max(
                            1.0,
                            VolumeExpansionRatio);
                }
        
                private bool HasMacdBias(
                    Bars bars,
                    int index,
                    int direction)
                {
                    if (!UseMacdBias ||
                        bars == null ||
                        index < 35)
                        return false;
        
                    Native set =
                        GetNative(bars);
        
                    if (set == null ||
                        set.MacdFast == null ||
                        set.MacdSlow == null ||
                        index >= set.MacdFast.Result.Count ||
                        index >= set.MacdSlow.Result.Count)
                        return false;
        
                    double histogram =
                        set.MacdFast.Result[index] -
                        set.MacdSlow.Result[index];
        
                    int previousIndex =
                        Math.Max(
                            0,
                            index - 2);
        
                    double previous =
                        set.MacdFast.Result[previousIndex] -
                        set.MacdSlow.Result[previousIndex];
        
                    if (double.IsNaN(histogram) ||
                        double.IsInfinity(histogram) ||
                        double.IsNaN(previous) ||
                        double.IsInfinity(previous))
                        return false;
        
                    return
                        direction == 1
                            ? histogram > 0 &&
                              histogram >= previous
                            : histogram < 0 &&
                              histogram <= previous;
                }
        
                private bool HasVwapBias(
                    Bars bars,
                    int index,
                    int direction)
                {
                    if (!UseVwapBias ||
                        bars == null ||
                        index < 20)
                        return false;
        
                    int first =
                        Math.Max(
                            0,
                            index -
                            Math.Max(
                                10,
                                VwapLookbackBars - 1));
        
                    double priceVolume = 0;
                    double volume = 0;
        
                    for (int i = first;
                         i <= index;
                         i++)
                    {
                        double typical =
                            (bars.HighPrices[i] +
                             bars.LowPrices[i] +
                             bars.ClosePrices[i]) /
                            3.0;
        
                        double v =
                            Math.Max(
                                1.0,
                                bars.TickVolumes[i]);
        
                        priceVolume +=
                            typical *
                            v;
        
                        volume +=
                            v;
                    }
        
                    if (volume <= 0)
                        return false;
        
                    double vwap =
                        priceVolume /
                        volume;
        
                    return
                        direction == 1
                            ? bars.ClosePrices[index] >
                              vwap
                            : bars.ClosePrices[index] <
                              vwap;
                }
        
                private bool HasHealthyVolatility(
                    Bars bars,
                    int index,
                    int direction)
                {
                    if (!UseHealthyVolatility ||
                        bars == null ||
                        index < 30)
                        return false;
        
                    double atr =
                        Atr(
                            bars,
                            index);
        
                    double oldAtr =
                        Atr(
                            bars,
                            Math.Max(
                                5,
                                index - 10));
        
                    if (atr <= 0 ||
                        oldAtr <= 0)
                        return false;
        
                    double body =
                        Math.Abs(
                            bars.ClosePrices[index] -
                            bars.OpenPrices[index]);
        
                    bool directional =
                        direction == 1
                            ? bars.ClosePrices[index] >
                              bars.OpenPrices[index]
                            : bars.ClosePrices[index] <
                              bars.OpenPrices[index];
        
                    double minRatio =
                        Math.Max(
                            0.50,
                            HealthyAtrMinimumRatio);
        
                    double maxRatio =
                        Math.Max(
                            minRatio,
                            HealthyAtrMaximumRatio);
        
                    return
                        directional &&
                        body >=
                        atr *
                        MinimumTriggerBodyAtr &&
                        atr >=
                        oldAtr *
                        minRatio &&
                        atr <=
                        oldAtr *
                        maxRatio;
                }
        
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
        
                private void AddFrame(
                    Frame frame,
                    double weight,
                    ref double buy,
                    ref double sell,
                    ref int evidence)
                {
                    if (frame == null || frame.Quality <= 0 || weight <= 0)
                        return;
        
                    double scale =
                        weight / 10.0;
        
                    buy += frame.BullScore * scale;
                    sell += frame.BearScore * scale;
        
                    if (frame.Direction != 0)
                        evidence++;
                }
        
                                private int TimeframeAgreement(
                    int direction,
                    DateTime reference)
                {
                    if (direction == 0)
                        return 0;
        
                    Frame[] frames =
                    {
                        _m5Frame,
                        _m15Frame,
                        _m30Frame,
                        _h1Frame,
                        _h4Frame,
                        _d1Frame,
                        _w1Frame
                    };
        
                    Bars[] bars =
                    {
                        _m5Bars,
                        _m15Bars,
                        _m30Bars,
                        _h1Bars,
                        _h4Bars,
                        _d1Bars,
                        _w1Bars
                    };
        
                    double[] weights =
                    {
                        Math.Max(0, M5Weight),
                        Math.Max(0, M15Weight),
                        Math.Max(0, M30Weight),
                        Math.Max(0, H1Weight),
                        Math.Max(0, H4Weight),
                        Math.Max(0, D1Weight),
                        Math.Max(0, W1Weight)
                    };
        
                    bool[] enabled =
                    {
                        true,
                        true,
                        M30Weight > 0,
                        H1Weight > 0,
                        H4Weight > 0,
                        D1Weight > 0,
                        SmartWeeklyContext &&
                        W1Weight > 0
                    };
        
                    double totalWeight = 0;
                    double alignedWeight = 0;
        
                    for (int i = 0;
                         i < frames.Length;
                         i++)
                    {
                        if (!enabled[i] ||
                            weights[i] <= 0 ||
                            frames[i] == null ||
                            bars[i] == null ||
                            frames[i].Quality <= 0)
                            continue;
        
                        int closedIndex =
                            ClosedIndex(
                                bars[i],
                                reference);
        
                        if (closedIndex < 0 ||
                            frames[i].Index != closedIndex)
                            continue;
        
                        // A neutral timeframe is not evidence against the selected
                        // direction; it contributes no alignment weight.
                        if (frames[i].Direction == 0)
                            continue;
        
                        totalWeight +=
                            weights[i];
        
                        if (frames[i].Direction == direction)
                            alignedWeight +=
                                weights[i];
                    }
        
                    return
                        totalWeight <= 0
                            ? 0
                            : ClampInt(
                                (int)Math.Round(
                                    100.0 *
                                    alignedWeight /
                                    totalWeight),
                                0,
                                100);
                }
        
                private int IndependentEvidence(
                    int direction)
                {
                    if (_m5Frame == null ||
                        direction == 0)
                        return 0;
        
                    int count = 0;
        
                    if (direction == 1)
                    {
                        if (_m5Frame.StructureBull) count++;
                        if (_m5Frame.LiquidityBull) count++;
                        if (_m5Frame.FvgBull) count++;
                        if (_m5Frame.ObBull) count++;
                        if (_m5Frame.DisplacementBull) count++;
                        if (_m5Frame.MomentumBull) count++;
                        if (_m5Frame.VolumeBull) count++;
                        if (_m5Frame.MacdBull) count++;
                        if (_m5Frame.VwapBull) count++;
                        if (_m5Frame.VolatilityBull) count++;
                    }
                    else
                    {
                        if (_m5Frame.StructureBear) count++;
                        if (_m5Frame.LiquidityBear) count++;
                        if (_m5Frame.FvgBear) count++;
                        if (_m5Frame.ObBear) count++;
                        if (_m5Frame.DisplacementBear) count++;
                        if (_m5Frame.MomentumBear) count++;
                        if (_m5Frame.VolumeBear) count++;
                        if (_m5Frame.MacdBear) count++;
                        if (_m5Frame.VwapBear) count++;
                        if (_m5Frame.VolatilityBear) count++;
                    }
        
                    return count;
                }
        
                private int StructuralConfirmations(
                    int direction)
                {
                    if (_m5Frame == null ||
                        direction == 0)
                        return 0;
        
                    int count = 0;
        
                    if (direction == 1)
                    {
                        if (_m5Frame.StructureBull) count++;
                        if (_m5Frame.MssBull ||
                            _m5Frame.ChochBull) count++;
                        if (_m5Frame.DisplacementBull) count++;
                        if (_m15Frame != null &&
                            _m15Frame.StructureBull) count++;
                        if (_h1Frame != null &&
                            _h1Frame.StructureBull) count++;
                        if (_h4Frame != null &&
                            _h4Frame.StructureBull) count++;
                    }
                    else
                    {
                        if (_m5Frame.StructureBear) count++;
                        if (_m5Frame.MssBear ||
                            _m5Frame.ChochBear) count++;
                        if (_m5Frame.DisplacementBear) count++;
                        if (_m15Frame != null &&
                            _m15Frame.StructureBear) count++;
                        if (_h1Frame != null &&
                            _h1Frame.StructureBear) count++;
                        if (_h4Frame != null &&
                            _h4Frame.StructureBear) count++;
                    }
        
                    return count;
                }
        
                private bool CanAcceptConfirmedDirection(
                    int direction,
                    int closedM5)
                {
                    if (direction == 0)
                        return false;
        
                    if (!PreventRapidDirectionFlip)
                        return true;
        
                    if (_lastConfirmedDirection == 0 ||
                        _lastConfirmedM5 < 0)
                        return true;
        
                    int elapsed =
                        closedM5 -
                        _lastConfirmedM5;
        
                    if (elapsed < 0)
                        return false;
        
                    if (direction ==
                        _lastConfirmedDirection)
                    {
                        if (_plan != null)
                            return false;
        
                        if (_lastExitM5 >= 0 &&
                            closedM5 -
                            _lastExitM5 <
                            Math.Max(
                                0,
                                ExitReentryCooldownM5))
                            return false;
        
                        return true;
                    }
        
                    if (elapsed <
                        Math.Max(
                            1,
                            OppositeSignalCooldownM5))
                        return false;
        
                    if (_plan != null &&
                        !AllowOppositeWhileActive)
                        return false;
        
                    if (RequireM15ReversalForOpposite)
                    {
                        if (_m15Frame == null ||
                            _m15Frame.Direction !=
                            direction)
                            return false;
        
                        bool structural =
                            direction == 1
                                ? (_m15Frame.MssBull ||
                                   _m15Frame.ChochBull)
                                : (_m15Frame.MssBear ||
                                   _m15Frame.ChochBear);
        
                        bool force =
                            direction == 1
                                ? (_m15Frame.DisplacementBull &&
                                   _m15Frame.LiquidityBull)
                                : (_m15Frame.DisplacementBear &&
                                   _m15Frame.LiquidityBear);
        
                        if (!(RequireReversalForce
                                ? structural && force
                                : structural || force))
                            return false;
        
                        int m15Index =
                            ClosedIndex(
                                _m15Bars,
                                _m5Bars.OpenTimes[closedM5]);
        
                        if (m15Index >= 0 &&
                            !StableDirection(
                                _m15Bars,
                                m15Index,
                                direction,
                                Math.Max(
                                    1,
                                    SmartFlipConfirmationBars)))
                            return false;
                    }
        
                    int m5Evidence = 0;
        
                    if (_m5Frame != null)
                    {
                        if (direction == 1)
                        {
                            if (_m5Frame.MssBull) m5Evidence++;
                            if (_m5Frame.ChochBull) m5Evidence++;
                            if (_m5Frame.DisplacementBull) m5Evidence++;
                            if (_m5Frame.LiquidityBull) m5Evidence++;
                        }
                        else
                        {
                            if (_m5Frame.MssBear) m5Evidence++;
                            if (_m5Frame.ChochBear) m5Evidence++;
                            if (_m5Frame.DisplacementBear) m5Evidence++;
                            if (_m5Frame.LiquidityBear) m5Evidence++;
                        }
                    }
        
                    return
                        m5Evidence >=
                        Math.Max(
                            1,
                            MinimumOppositeM5Structure);
                }
        
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
        
                    if (d.Confidence < MinimumConfidence)
                    {
                        reason = "CONFIDENCE";
                        return false;
                    }
        
                    if (d.Edge < adaptiveEdgeThreshold)
                    {
                        reason = "EDGE";
                        return false;
                    }
        
                    if (d.SmartQuality < adaptiveQualityThreshold)
                    {
                        reason = "SMART QUALITY";
                        return false;
                    }
        
                    if (RequireHigherTfAgreement &&
                        d.TimeframeAgreement <
                        MinimumTimeframeAgreement)
                    {
                        reason = "MTF AGREEMENT";
                        return false;
                    }
        
                    if (d.IndependentEvidence <
                        Math.Max(
                            MinimumIndependentEvidence,
                            EnableSmartDecisionEngine
                                ? SmartMinimumIndependentEvidence
                                : 0))
                    {
                        reason = "INDEPENDENT EVIDENCE";
                        return false;
                    }
        
                    if (RequireStructuralConfirmation &&
                        d.StructuralConfirmations <
                        MinimumStructuralConfirmations)
                    {
                        reason = "STRUCTURE";
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
        
                    if (EnableSmartDecisionEngine)
                    {
                        int strongest =
                            Math.Max(
                                d.BuyShare,
                                d.SellShare);
        
                        if (RequireSmartConsensus &&
                            strongest <
                            Math.Max(
                                SmartConsensusThreshold,
                                adaptiveShareThreshold))
                        {
                            bool soft =
                                AllowSmartSoftGate &&
                                d.SmartQuality >=
                                SmartStrongSetupQuality &&
                                d.Edge >=
                                SmartStrongSetupEdge &&
                                d.IndependentEvidence >=
                                SmartMinimumIndependentEvidence + 1;
        
                            if (!soft)
                            {
                                reason = "SMART CONSENSUS";
                                return false;
                            }
                        }
        
                        if (d.TimeframeAgreement <
                            SmartMinimumTimeframeAgreement)
                        {
                            reason = "SMART MTF";
                            return false;
                        }
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
        
                private bool RestrictionAlertEnabled(
                    string reason)
                {
                    if (string.IsNullOrWhiteSpace(
                            reason))
                        return AlertOnEntryRestriction;
        
                    switch (reason)
                    {
                        case "NEWS BLACKOUT":
                            return AlertOnNewsEventGuard ||
                                   AlertOnNewsEvent ||
                                   AlertOnEntryRestriction;
        
                        case "SESSION":
                            return AlertOnSessionBlock ||
                                   AlertOnEntryRestriction;
        
                        case "SPREAD":
                            return AlertOnSpreadBlock ||
                                   AlertOnEntryRestriction;
        
                        case "FRIDAY":
                            return AlertOnFridayBlock ||
                                   AlertOnEntryRestriction;
        
                        case "REGIME NO-TRADE":
                        case "CHOP":
                            return AlertOnRegimeNoTrade ||
                                   AlertOnEntryRestriction;
        
                        case "COOLDOWN":
                        case "DIRECTION FLIP":
                            return AlertOnCooldownBlock ||
                                   AlertOnEntryRestriction;
        
                        default:
                            return AlertOnEntryRestriction;
                    }
                }
        
                private string BuildReason(
                    Decision d,
                    int buyShare,
                    int sellShare)
                {
                    return
                        (d.Direction == 1
                            ? "BUY"
                            : "SELL") +
                        " | CONF " +
                        d.Confidence +
                        " | EDGE " +
                        d.Edge +
                        " | SMART " +
                        d.SmartQuality +
                        " | MTF " +
                        d.TimeframeAgreement +
                        " | EVID " +
                        d.IndependentEvidence +
                        " | STRUCT " +
                        d.StructuralConfirmations +
                        " | RETEST " +
                        d.RetestQuality +
                        " | REGIME " +
                        d.Regime +
                        " | " +
                        buyShare +
                        "/" +
                        sellShare +
                        (string.IsNullOrWhiteSpace(d.BlockReason)
                            ? ""
                            : " | BLOCK " +
                              d.BlockReason);
                }
        
                private int PremiumDiscountBias(
                    Bars bars,
                    int index)
                {
                    if (!UsePremiumDiscount ||
                        bars == null ||
                        index < 10)
                        return 0;
        
                    double high =
                        Highest(
                            bars,
                            Math.Max(
                                0,
                                index -
                                StructureLookback),
                            index);
        
                    double low =
                        Lowest(
                            bars,
                            Math.Max(
                                0,
                                index -
                                StructureLookback),
                            index);
        
                    if (high <= low)
                        return 0;
        
                    double midpoint =
                        (high + low) * 0.5;
        
                    if (bars.ClosePrices[index] <
                        midpoint)
                        return 1;
        
                    if (bars.ClosePrices[index] >
                        midpoint)
                        return -1;
        
                    return 0;
                }
        
                private double LiveBias(
                    int chartIndex,
                    int direction)
                {
                    if (Bars == null ||
                        Bars.Count < 10)
                        return 0;
        
                    int i =
                        Math.Max(
                            1,
                            Math.Min(
                                chartIndex,
                                Bars.Count - 1));
        
                    double fast =
                        Ema(
                            Bars,
                            i,
                            true);
        
                    double slow =
                        Ema(
                            Bars,
                            i,
                            false);
        
                    if (direction == 1 &&
                        Bars.ClosePrices[i] > fast &&
                        fast > slow)
                        return 5;
        
                    if (direction == -1 &&
                        Bars.ClosePrices[i] < fast &&
                        fast < slow)
                        return 5;
        
                    return 0;
                }
    }
}
