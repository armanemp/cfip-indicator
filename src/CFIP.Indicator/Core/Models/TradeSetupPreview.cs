namespace cAlgo
{
    // Read-only setup levels for chart/panel presentation before an executable
    // Plan exists. This object is never consumed by broker submission paths.
    internal sealed class TradeSetupPreview
    {
        public int Direction;
        public ExecutionMode EntryMode;
        public int CreatedM5;

        public double Entry;
        public double IdealEntry;
        public double ZoneLow;
        public double ZoneHigh;
        public double ZoneTolerance;
        public double Trigger;
        public double Invalidation;
        public double Stop;
        public double Tp1;
        public double Tp2;
        public double Tp3;
        public double Tp4;
        public double Risk;
    }
}