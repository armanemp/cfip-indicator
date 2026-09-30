using System;

namespace cAlgo
{
    internal sealed class HistoricalSignalPresentation
    {
        public DateTime OpenTime { get; set; }
        public int Direction { get; set; }
        public double Price { get; set; }
        public bool IsSignal { get; set; }
    }
}
