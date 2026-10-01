namespace cAlgo
{
    public partial class CFIPIndicator
    {
        private Plan _pendingOrderPlanSnapshot;

        private Plan CapturePendingOrderPlanSnapshot()
        {
            if (_plan == null)
                return null;

            return new Plan
            {
                Direction = _plan.Direction,
                Lane = _plan.Lane,
                EntryMode = _plan.EntryMode,
                Entry = _plan.Entry,
                IdealEntry = _plan.IdealEntry,
                EntryZoneLow = _plan.EntryZoneLow,
                EntryZoneHigh = _plan.EntryZoneHigh,
                EntryTrigger = _plan.EntryTrigger,
                EntryInvalidation = _plan.EntryInvalidation,
                EntryQuality = _plan.EntryQuality,
                EntrySource = _plan.EntrySource,
                Stop = _plan.Stop,
                Tp1 = _plan.Tp1,
                Tp2 = _plan.Tp2,
                Tp3 = _plan.Tp3,
                Tp4 = _plan.Tp4,
                Risk = _plan.Risk,
                Tp1RR = _plan.Tp1RR,
                Tp2RR = _plan.Tp2RR,
                Tp3RR = _plan.Tp3RR,
                Tp4RR = _plan.Tp4RR,
                StopQuality = _plan.StopQuality,
                Tp1Quality = _plan.Tp1Quality,
                Tp2Quality = _plan.Tp2Quality,
                Tp3Quality = _plan.Tp3Quality,
                Tp4Quality = _plan.Tp4Quality,
                StopSource = _plan.StopSource,
                Tp1Source = _plan.Tp1Source,
                Tp2Source = _plan.Tp2Source,
                Tp3Source = _plan.Tp3Source,
                Tp4Source = _plan.Tp4Source,
                HtfTargetCount = _plan.HtfTargetCount,
                CreatedM5 = _plan.CreatedM5,
                SignalBarOpenTimeUtcTicks = _plan.SignalBarOpenTimeUtcTicks,
                SignalTraceId = _plan.SignalTraceId,
                OriginalVolume = _plan.OriginalVolume,
                CalibrationEligible = _plan.CalibrationEligible,
                CalibrationDirection = _plan.CalibrationDirection,
                CalibrationLane = _plan.CalibrationLane,
                CalibrationRegime = _plan.CalibrationRegime,
                CalibrationConfidence = _plan.CalibrationConfidence,
                CalibrationBucket = _plan.CalibrationBucket,
                IsLivePosition = false,
                PositionId = 0
            };
        }

        private void ClearPendingOrderPlanSnapshot()
        {
            _pendingOrderPlanSnapshot = null;
        }
    }
}
