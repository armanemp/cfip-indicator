using System;

namespace cAlgo
{
    internal readonly struct IndependentEvidenceFusionInput
    {
        public bool Structure { get; }
        public bool Transition { get; }
        public bool Displacement { get; }
        public bool Liquidity { get; }
        public bool Fvg { get; }
        public bool OrderBlock { get; }
        public bool Trend { get; }
        public bool Momentum { get; }
        public bool Macd { get; }
        public bool Vwap { get; }
        public bool Volume { get; }
        public bool Volatility { get; }
        public bool Rejection { get; }
        public bool EqualLevel { get; }

        public IndependentEvidenceFusionInput(
            bool structure,
            bool transition,
            bool displacement,
            bool liquidity,
            bool fvg,
            bool orderBlock,
            bool trend,
            bool momentum,
            bool macd,
            bool vwap,
            bool volume,
            bool volatility,
            bool rejection,
            bool equalLevel)
        {
            Structure = structure;
            Transition = transition;
            Displacement = displacement;
            Liquidity = liquidity;
            Fvg = fvg;
            OrderBlock = orderBlock;
            Trend = trend;
            Momentum = momentum;
            Macd = macd;
            Vwap = vwap;
            Volume = volume;
            Volatility = volatility;
            Rejection = rejection;
            EqualLevel = equalLevel;
        }
    }

    internal sealed class IndependentEvidenceFusionCalculator
    {
        public int Calculate(
            IndependentEvidenceFusionInput input)
        {
            double structural =
                CappedContribution(
                    2.0,
                    input.Structure ? 1.0 : 0.0,
                    input.Transition ? 0.5 : 0.0,
                    input.Displacement ? 0.5 : 0.0);

            double location =
                CappedContribution(
                    2.0,
                    input.Liquidity ? 1.0 : 0.0,
                    input.Fvg ? 0.5 : 0.0,
                    input.OrderBlock ? 0.5 : 0.0);

            double trendMomentum =
                CappedContribution(
                    2.0,
                    input.Trend ? 1.0 : 0.0,
                    input.Momentum ? 0.5 : 0.0,
                    input.Macd ? 0.25 : 0.0,
                    input.Vwap ? 0.25 : 0.0);

            double context =
                CappedContribution(
                    2.0,
                    input.Volume ? 1.0 : 0.0,
                    input.Volatility ? 0.5 : 0.0,
                    input.Rejection ? 0.25 : 0.0,
                    input.EqualLevel ? 0.25 : 0.0);

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
