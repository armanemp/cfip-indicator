using System;

namespace cAlgo
{
    internal sealed class IndependentEvidenceFusionCalculator
    {
        public int Calculate(Frame frame, int direction)
        {
            if (frame == null || direction == 0)
                return 0;

            bool structure =
                direction == 1
                    ? frame.StructureBull
                    : frame.StructureBear;

            bool transition =
                direction == 1
                    ? frame.MssBull || frame.ChochBull
                    : frame.MssBear || frame.ChochBear;

            bool displacement =
                direction == 1
                    ? frame.DisplacementBull
                    : frame.DisplacementBear;

            bool liquidity =
                direction == 1
                    ? frame.LiquidityBull
                    : frame.LiquidityBear;

            bool fvg =
                direction == 1
                    ? frame.FvgBull
                    : frame.FvgBear;

            bool orderBlock =
                direction == 1
                    ? frame.ObBull
                    : frame.ObBear;

            bool trend =
                direction == 1
                    ? frame.TrendBull
                    : frame.TrendBear;

            bool momentum =
                direction == 1
                    ? frame.MomentumBull
                    : frame.MomentumBear;

            bool macd =
                direction == 1
                    ? frame.MacdBull
                    : frame.MacdBear;

            bool vwap =
                direction == 1
                    ? frame.VwapBull
                    : frame.VwapBear;

            bool volume =
                direction == 1
                    ? frame.VolumeBull
                    : frame.VolumeBear;

            bool volatility =
                direction == 1
                    ? frame.VolatilityBull
                    : frame.VolatilityBear;

            bool rejection =
                direction == 1
                    ? frame.RejectionBull
                    : frame.RejectionBear;

            bool equalLevel =
                direction == 1
                    ? frame.EqualLow
                    : frame.EqualHigh;

            double structural =
                CappedContribution(
                    2.0,
                    structure ? 1.0 : 0.0,
                    transition ? 0.5 : 0.0,
                    displacement ? 0.5 : 0.0);

            double location =
                CappedContribution(
                    2.0,
                    liquidity ? 1.0 : 0.0,
                    fvg ? 0.5 : 0.0,
                    orderBlock ? 0.5 : 0.0);

            double trendMomentum =
                CappedContribution(
                    2.0,
                    trend ? 1.0 : 0.0,
                    momentum ? 0.5 : 0.0,
                    macd ? 0.25 : 0.0,
                    vwap ? 0.25 : 0.0);

            double context =
                CappedContribution(
                    2.0,
                    volume ? 1.0 : 0.0,
                    volatility ? 0.5 : 0.0,
                    rejection ? 0.25 : 0.0,
                    equalLevel ? 0.25 : 0.0);

            return NumericGuards.ClampInt(
                (int)Math.Round(
                    structural +
                    location +
                    trendMomentum +
                    context),
                0,
                8);
        }

        private static double CappedContribution(
            double cap,
            params double[] contributions)
        {
            double total = 0;

            foreach (double contribution in contributions)
                total += Math.Max(0, contribution);

            return Math.Min(
                cap,
                total);
        }
    }
}
