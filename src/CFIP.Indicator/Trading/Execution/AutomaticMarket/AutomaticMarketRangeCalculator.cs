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
            if (_m5Bars == null ||
                closedM5 < 1 ||
                !IsFinitePositive(basePrice) ||
                !IsFinitePositive(Symbol.PipSize))
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
                    Symbol.Ask - Symbol.Bid);

            double spreadPips =
                spread /
                Math.Max(
                    Symbol.PipSize,
                    1e-9);

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
            return Math.Max(
                0.10,
                Math.Min(
                    maximum,
                    desired));
        }

        private TradeResult TryExecuteAutomaticMarketOrderFallback(
            TradeType type,
            double volume,
            double stopPips,
            double targetPips)
        {
            // Kept as the architectural cross-path compatibility boundary. The
            // normal Phase 9.7 path uses Market Range; this fallback is not used
            // while a valid bounded market-range envelope is available.
            return TryExecuteMarketOrder(
                type,
                SymbolName,
                volume,
                NormalizeLabel(),
                stopPips,
                targetPips,
                TradeExecutionMetadata.DefaultExecutionComment,
                false,
                "AUTOMATIC MARKET FALLBACK");
        }
    }
}
