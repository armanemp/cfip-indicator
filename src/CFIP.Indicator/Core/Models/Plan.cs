namespace cAlgo
{
    internal sealed class Plan
    {
        public int Direction = 0;
        public OpportunityLane Lane = default;
        public ExecutionMode EntryMode = default;
        public double Entry = 0d;
        public double IdealEntry = 0d;
        public double EntryZoneLow = 0d;
        public double EntryZoneHigh = 0d;
        public double EntryZoneTolerance = 0d;
        public double EntryTrigger = 0d;
        public double EntryInvalidation = 0d;
        public int EntryQuality = 0;
        public string EntrySource = null;
        public double Stop = 0d;
        public double Tp1 = 0d;
        public double Tp2 = 0d;
        public double Tp3 = 0d;
        public double Tp4 = 0d;
        public double Risk = 0d;
        public double Tp1RR = 0d;
        public double Tp2RR = 0d;
        public double Tp3RR = 0d;
        public double Tp4RR = 0d;
        public int StopQuality = 0;
        public int Tp1Quality = 0;
        public int Tp2Quality = 0;
        public int Tp3Quality = 0;
        public int Tp4Quality = 0;
        public string StopSource = null;
        public string Tp1Source = null;
        public string Tp2Source = null;
        public string Tp3Source = null;
        public string Tp4Source = null;
        public int HtfTargetCount = 0;
        public int CreatedM5 = 0;
        public long SignalBarOpenTimeUtcTicks = 0L;
        public string SignalTraceId = null;
        public double OriginalVolume = 0d;
        public bool CalibrationEligible = false;
        public int CalibrationDirection = 0;
        public OpportunityLane CalibrationLane = default;
        public string CalibrationRegime = null;
        public int CalibrationConfidence = 0;
        public int CalibrationBucket = 0;
        public bool IsLivePosition = false;
        public long PositionId = 0L;
    }
}
