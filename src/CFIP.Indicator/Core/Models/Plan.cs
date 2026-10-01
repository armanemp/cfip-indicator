namespace cAlgo
{
    internal sealed class Plan
                    {
                        public int Direction;
                        public OpportunityLane Lane;
                        public ExecutionMode EntryMode;
                        public double Entry;
                        public double IdealEntry;
                        public double EntryZoneLow;
                        public double EntryZoneHigh;
                        public double EntryZoneTolerance;
                        public double EntryTrigger;
                        public double EntryInvalidation;
                        public int EntryQuality;
                        public string EntrySource;
                        public double Stop;
                        public double Tp1;
                        public double Tp2;
                        public double Tp3;
                        public double Tp4;
                        public double Risk;
                        public double Tp1RR;
                        public double Tp2RR;
                        public double Tp3RR;
                        public double Tp4RR;
                        public int StopQuality;
                        public int Tp1Quality;
                        public int Tp2Quality;
                        public int Tp3Quality;
                        public int Tp4Quality;
                        public string StopSource;
                        public string Tp1Source;
                        public string Tp2Source;
                        public string Tp3Source;
                        public string Tp4Source;
                        public int HtfTargetCount;
                        public int CreatedM5;
                        public long SignalBarOpenTimeUtcTicks;
                        public string SignalTraceId;
                        public double OriginalVolume;

                        public bool CalibrationEligible;
                        public int CalibrationDirection;
                        public OpportunityLane CalibrationLane;
                        public string CalibrationRegime;
                        public int CalibrationConfidence;
                        public int CalibrationBucket;

                        public bool IsLivePosition;
                        public long PositionId;
                    }
}
