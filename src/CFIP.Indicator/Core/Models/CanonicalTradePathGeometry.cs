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
        public string Tp1Source { get; }
        public int Tp1Quality { get; }
        public string Tp2Source { get; }
        public int Tp2Quality { get; }
        public string Tp3Source { get; }
        public int Tp3Quality { get; }
        public string Tp4Source { get; }
        public int Tp4Quality { get; }
        public int HtfTargetCount { get; }
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
            string tp1Source,
            int tp1Quality,
            string tp2Source,
            int tp2Quality,
            string tp3Source,
            int tp3Quality,
            string tp4Source,
            int tp4Quality,
            int htfTargetCount,
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
            Tp1Source = tp1Source ?? string.Empty;
            Tp1Quality = tp1Quality;
            Tp2Source = tp2Source ?? string.Empty;
            Tp2Quality = tp2Quality;
            Tp3Source = tp3Source ?? string.Empty;
            Tp3Quality = tp3Quality;
            Tp4Source = tp4Source ?? string.Empty;
            Tp4Quality = tp4Quality;
            HtfTargetCount = Math.Max(0, htfTargetCount);
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
