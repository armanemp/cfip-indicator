namespace cAlgo
{
    internal sealed class ExecutionModel
                    {
                        public int Direction;
                        public ExecutionMode Mode;
                        public double IdealEntry;
                        public double ActualEntry;
                        public double ZoneLow;
                        public double ZoneHigh;
                        public double ZoneTolerance;
                        public double Trigger;
                        public double Invalidation;
                        public int Quality;
                        public bool Ready;
                        public bool IsLate;
                        public string Source;
                    }
}
