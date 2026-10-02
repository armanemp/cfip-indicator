using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private double CalculateAutomaticMarketRangePips(
            int closedM5,
            double basePrice)
        {
            CanonicalPriceSnapshot priceSnapshot =
                GetCanonicalPriceSnapshot();

            if (_m5Bars == null ||
                closedM5 < 1 ||
                !IsFinitePositive(basePrice) ||
                priceSnapshot == null ||
                !priceSnapshot.IsQuoteValid ||
                !IsFinitePositive(priceSnapshot.PipSize))
                return 0;

            double atr =
                Atr(
                    _m5Bars,
                    closedM5);

            if (!IsFinitePositive(atr))
                return 0;

            double spread =
                Math.Max(
                    0,
                    priceSnapshot.Spread);

            double spreadPips =
                priceSnapshot.SpreadPips;

            double atrPips =
                atr /
                Math.Max(
                    Symbol.PipSize,
                    1e-9);

            // Market-range is a bounded execution envelope, not permission to
            // chase price. Keep it close to the live spread while allowing a
            // small ATR-scaled tolerance for normal quote movement.
            double maximum =
                atrPips *
                Math.Max(
                    0.08,
                    Math.Min(
                        0.20,
                        Math.Max(
                            0.08,
                            MaximumEntryExtensionAtr * 0.50)));

            if (maximum <= 0)
                return 0;

            double minimumUsableRange =
                Math.Max(
                    0.10,
                    spreadPips * 1.10);

            double desired =
                Math.Max(
                    minimumUsableRange,
                    atrPips * 0.02);

            // The market-range envelope is always capped by the ATR-derived
            // maximum; it may never silently exceed the execution extension gate.
            double range =
                Math.Max(
                    0.10,
                    Math.Min(
                        maximum,
                        desired));

            if (_cfipProviderExecutionIntent != null &&
                !double.IsNaN(range) &&
                !double.IsInfinity(range) &&
                range >= 0)
            {
                _cfipProviderExecutionIntent.MarketRangePips =
                    range;
            }

            return range;
        }

    }
}
