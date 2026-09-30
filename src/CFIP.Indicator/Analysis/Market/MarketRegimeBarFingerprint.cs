using System;
using cAlgo.API;

namespace cAlgo
{
    internal readonly struct MarketRegimeBarFingerprint
    {
        public DateTime OpenTime { get; }
        public double Open { get; }
        public double High { get; }
        public double Low { get; }
        public double Close { get; }
        public double TickVolume { get; }

        public MarketRegimeBarFingerprint(
            DateTime openTime,
            double open,
            double high,
            double low,
            double close,
            double tickVolume)
        {
            OpenTime = openTime;
            Open = open;
            High = high;
            Low = low;
            Close = close;
            TickVolume = tickVolume;
        }
    }
}
