using System;

namespace cAlgo
{
    internal sealed class CanonicalTradePathGeometry
    {
        public int Direction { get; }
        public ExecutionMode EntryMode { get; }
        public double Entry { get; }
        public double IdealEntry { get; }
        public double Stop { get; }
        public double Tp1 { get; }
        public double Tp2 { get; }
        public double Tp3 { get; }
        public double Tp4 { get; }
        public double Risk { get; }
        public double Tp1RR { get; }
        public string StopSource { get; }
        public int StopQuality { get; }
        public TradeSetupPreview Preview { get; }

        public CanonicalTradePathGeometry(
            int direction,
            ExecutionMode entryMode,
            double entry,
            double idealEntry,
            double stop,
            double tp1,
            double tp2,
            double tp3,
            double tp4,
            double risk,
            double tp1RR,
            string stopSource,
            int stopQuality,
            TradeSetupPreview preview)
        {
            Direction = direction;
            EntryMode = entryMode;
            Entry = entry;
            IdealEntry = idealEntry;
            Stop = stop;
            Tp1 = tp1;
            Tp2 = tp2;
            Tp3 = tp3;
            Tp4 = tp4;
            Risk = risk;
            Tp1RR = tp1RR;
            StopSource = stopSource ?? string.Empty;
            StopQuality = stopQuality;
            Preview = preview;
        }

        public bool IsValid
        {
            get
            {
                return
                    (Direction == 1 || Direction == -1) &&
                    EntryMode != ExecutionMode.None &&
                    HasFinitePositiveTradePathValue(Entry) &&
                    HasFinitePositiveTradePathValue(Stop) &&
                    HasFinitePositiveTradePathValue(Tp1) &&
                    HasFinitePositiveTradePathValue(Risk) &&
                    Tp1RR > 0;
            }
        }

        private static bool HasFinitePositiveTradePathValue(double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value) &&
                value > 0;
        }
    }
}
