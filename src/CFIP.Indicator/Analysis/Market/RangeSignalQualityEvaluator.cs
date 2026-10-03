using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private RangeSignalQualityResult EvaluateRangeSignalQuality(
            int index,
            int direction,
            int confidence,
            int smartQuality,
            int edge,
            int independentEvidence,
            int structuralConfirmations)
        {
            MarketRegimeSnapshot regime =
                GetActiveM5Regime(index);

            if (regime == null)
                return new RangeSignalQualityResult(
                    true,
                    string.Empty);

            if (!string.Equals(
                    regime.Regime,
                    "RANGE",
                    StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(
                    regime.Regime,
                    "COMPRESSION",
                    StringComparison.OrdinalIgnoreCase))
                return new RangeSignalQualityResult(
                    true,
                    string.Empty);

            if (_m5Bars == null ||
                _m5Frame == null ||
                index < 5 ||
                index >= _m5Bars.Count)
                return new RangeSignalQualityResult(
                    false,
                    "RANGE NO-TRADE • DATA");

            int lookback =
                Math.Max(
                    10,
                    Math.Min(
                        40,
                        RegimeLookbackBars));

            int first =
                Math.Max(
                    0,
                    index -
                    lookback +
                    1);

            double rangeHigh =
                _m5Bars.HighPrices[first];
            double rangeLow =
                _m5Bars.LowPrices[first];

            int priorLast =
                Math.Max(
                    first,
                    index - 1);

            double priorRangeHigh =
                _m5Bars.HighPrices[first];
            double priorRangeLow =
                _m5Bars.LowPrices[first];

            for (int i = first + 1;
                 i <= index;
                 i++)
            {
                rangeHigh =
                    Math.Max(
                        rangeHigh,
                        _m5Bars.HighPrices[i]);
                rangeLow =
                    Math.Min(
                        rangeLow,
                        _m5Bars.LowPrices[i]);
            }

            for (int i = first + 1;
                 i <= priorLast;
                 i++)
            {
                priorRangeHigh =
                    Math.Max(
                        priorRangeHigh,
                        _m5Bars.HighPrices[i]);
                priorRangeLow =
                    Math.Min(
                        priorRangeLow,
                        _m5Bars.LowPrices[i]);
            }

            double width =
                rangeHigh - rangeLow;

            double rangePosition =
                width > 0
                    ? (_m5Bars.ClosePrices[index] -
                       rangeLow) /
                      width
                    : 0.50;

            bool liquidity =
                direction == 1
                    ? _m5Frame.LiquidityBull
                    : direction == -1 &&
                      _m5Frame.LiquidityBear;

            bool structure =
                direction == 1
                    ? (_m5Frame.MssBull ||
                       _m5Frame.ChochBull ||
                       _m5Frame.StructureBull)
                    : direction == -1 &&
                      (_m5Frame.MssBear ||
                       _m5Frame.ChochBear ||
                       _m5Frame.StructureBear);

            bool displacement =
                direction == 1
                    ? _m5Frame.DisplacementBull
                    : direction == -1 &&
                      _m5Frame.DisplacementBear;

            bool waveTrendAligned =
                direction == 1
                    ? _m5Frame.WaveTrendDirection == 1 ||
                      _m5Frame.WaveTrendBull
                    : direction == -1 &&
                      (_m5Frame.WaveTrendDirection == -1 ||
                       _m5Frame.WaveTrendBear);

            bool waveTrendReversal =
                direction == 1
                    ? (_m5Frame.WaveTrendBullCross ||
                       (_m5Frame.WaveTrendOversold &&
                        _m5Frame.WaveTrendDelta > 0))
                    : direction == -1 &&
                      (_m5Frame.WaveTrendBearCross ||
                       (_m5Frame.WaveTrendOverbought &&
                        _m5Frame.WaveTrendDelta < 0));

            double priorWidth =
                priorRangeHigh -
                priorRangeLow;

            double close =
                _m5Bars.ClosePrices[index];

            double atr =
                Atr(
                    _m5Bars,
                    index);

            bool breakout =
                priorWidth > 0 &&
                atr > 0 &&
                (direction == 1
                    ? close >
                      priorRangeHigh +
                      atr * 0.10
                    : direction == -1 &&
                      close <
                      priorRangeLow -
                      atr * 0.10);

            RangeSignalQualityInput input =
                new RangeSignalQualityInput(
                    direction,
                    rangePosition,
                    regime.RangeEfficiency,
                    regime.Choppiness,
                    regime.Adx,
                    liquidity,
                    structure,
                    displacement,
                    waveTrendAligned,
                    waveTrendReversal,
                    breakout,
                    _m5Frame.WaveTrendQuality,
                    independentEvidence,
                    structuralConfirmations,
                    confidence,
                    smartQuality,
                    edge);

            return RangeSignalQualityRule.Evaluate(
                regime.Regime,
                input);
        }
    }
}
