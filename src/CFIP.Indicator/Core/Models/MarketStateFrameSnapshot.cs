using System;

namespace cAlgo
{
    internal sealed class MarketStateFrameSnapshot
    {
        public string Timeframe { get; }
        public int ClosedIndex { get; }
        public int Direction { get; }
        public int Quality { get; }
        public string Regime { get; }
        public int RegimeQuality { get; }
        public int RegimeStability { get; }
        public string PreviousRegime { get; }
        public string RegimeTransition { get; }
        public double Choppiness { get; }
        public double AtrRatio { get; }
        public double EmaSpreadAtr { get; }
        public double EmaSlopeAtr { get; }
        public double RangeEfficiency { get; }
        public double Adx { get; }
        public double RangeWidthAtr { get; }

        public MarketStateFrameSnapshot(
            string timeframe,
            int closedIndex,
            int direction,
            int quality,
            string regime,
            int regimeQuality,
            int regimeStability,
            string previousRegime,
            string regimeTransition,
            double choppiness,
            double atrRatio,
            double emaSpreadAtr,
            double emaSlopeAtr,
            double rangeEfficiency,
            double adx = 0,
            double rangeWidthAtr = 0)
        {
            Timeframe =
                string.IsNullOrWhiteSpace(timeframe)
                    ? "UNKNOWN"
                    : timeframe.Trim().ToUpperInvariant();

            ClosedIndex = closedIndex;
            Direction =
                direction == 1 || direction == -1
                    ? direction
                    : 0;
            Quality = Math.Max(0, Math.Min(100, quality));
            Regime = MarketRegimeIdentity.NormalizeMarketRegime(regime);
            RegimeQuality = Math.Max(0, Math.Min(100, regimeQuality));
            RegimeStability = Math.Max(0, Math.Min(3, regimeStability));
            PreviousRegime =
                MarketRegimeIdentity.NormalizeMarketRegime(
                    previousRegime);
            RegimeTransition =
                string.IsNullOrWhiteSpace(regimeTransition)
                    ? MarketRegimeTransitionRule.Unknown
                    : regimeTransition.Trim().ToUpperInvariant();
            Choppiness = choppiness;
            AtrRatio = atrRatio;
            EmaSpreadAtr = emaSpreadAtr;
            EmaSlopeAtr = emaSlopeAtr;
            RangeEfficiency = rangeEfficiency;
            Adx = adx;
            RangeWidthAtr = rangeWidthAtr;
        }
    }
}
