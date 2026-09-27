using System;

namespace cAlgo
{
    internal sealed class OssIndicatorSnapshot
    {
        public int BullVotes { get; set; }
        public int BearVotes { get; set; }
        public int IndicatorCount { get; set; }

        public double Rsi { get; set; } = double.NaN;
        public double MacdHistogram { get; set; } = double.NaN;
        public double BollingerPercentB { get; set; } = double.NaN;
        public double Mfi { get; set; } = double.NaN;
        public double StochK { get; set; } = double.NaN;
        public double StochD { get; set; } = double.NaN;
        public double SuperTrend { get; set; } = double.NaN;

        public double AroonOscillator { get; set; } = double.NaN;
        public double Cci { get; set; } = double.NaN;
        public double ObvBias { get; set; } = double.NaN;
        public double ParabolicSar { get; set; } = double.NaN;
    }
}
