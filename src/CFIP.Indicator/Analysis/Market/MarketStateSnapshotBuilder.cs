using System;

namespace cAlgo
{
    public partial class CFIPIndicator
    {
        private MarketStateSnapshot BuildMarketStateSnapshot(
            DateTime reference,
            MtfClosedContext context)
        {
            if (context == null)
                return null;

            return new MarketStateSnapshot(
                reference,
                BuildMarketStateFrameSnapshot(
                    "M1",
                    context.M1,
                    _m1Frame),
                BuildMarketStateFrameSnapshot(
                    "M5",
                    context.M5,
                    _m5Frame),
                BuildMarketStateFrameSnapshot(
                    "M15",
                    context.M15,
                    _m15Frame),
                BuildMarketStateFrameSnapshot(
                    "M30",
                    context.M30,
                    _m30Frame),
                BuildMarketStateFrameSnapshot(
                    "H1",
                    context.H1,
                    _h1Frame),
                BuildMarketStateFrameSnapshot(
                    "H4",
                    context.H4,
                    _h4Frame),
                BuildMarketStateFrameSnapshot(
                    "D1",
                    context.D1,
                    _d1Frame),
                BuildMarketStateFrameSnapshot(
                    "W1",
                    context.W1,
                    _w1Frame),
                UsePremiumDiscount
                    ? PremiumDiscountBias(
                        _m5Bars,
                        context.M5)
                    : 0,
                SessionWindowRule.IsInside(
                    reference,
                    SessionStartUtc,
                    SessionEndUtc));
        }

        private static MarketStateFrameSnapshot BuildMarketStateFrameSnapshot(
            string timeframe,
            int closedIndex,
            Frame frame)
        {
            if (frame == null)
            {
                return new MarketStateFrameSnapshot(
                    timeframe,
                    closedIndex,
                    0,
                    0,
                    MarketRegimeIdentity.Unknown,
                    0,
                    0,
                    MarketRegimeIdentity.Unknown,
                    MarketRegimeTransitionRule.Unknown,
                    0,
                    1,
                    0,
                    0,
                    0,
                    0,
                    0);
            }

            return new MarketStateFrameSnapshot(
                timeframe,
                closedIndex,
                frame.Direction,
                frame.Quality,
                frame.Regime,
                frame.RegimeQuality,
                frame.RegimeStability,
                frame.PreviousRegime,
                frame.RegimeTransition,
                frame.Choppiness,
                frame.AtrRatio,
                frame.EmaSpreadAtr,
                frame.EmaSlopeAtr,
                frame.RangeEfficiency,
                frame.Adx,
                frame.RangeWidthAtr);
        }
    }
}
