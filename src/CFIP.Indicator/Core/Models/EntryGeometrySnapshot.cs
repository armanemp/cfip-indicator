namespace cAlgo
{
    internal readonly struct EntryGeometrySnapshot
    {
        public int Direction { get; }
        public ExecutionMode Mode { get; }
        public bool IsValid { get; }
        public bool InsideZone { get; }
        public bool TriggerReached { get; }
        public bool IsLate { get; }
        public double Market { get; }
        public double ZoneLow { get; }
        public double ZoneHigh { get; }
        public double ZoneTolerance { get; }
        public double IdealEntry { get; }
        public double Trigger { get; }
        public double Anchor { get; }
        public double ActualEntry { get; }
        public double EntryDistanceAtr { get; }
        public double ZoneDistanceAtr { get; }
        public double TriggerExtensionAtr { get; }
        public string Reason { get; }

        public EntryGeometrySnapshot(
            int direction,
            ExecutionMode mode,
            bool isValid,
            bool insideZone,
            bool triggerReached,
            bool isLate,
            double market,
            double zoneLow,
            double zoneHigh,
            double zoneTolerance,
            double idealEntry,
            double trigger,
            double anchor,
            double actualEntry,
            double entryDistanceAtr,
            double zoneDistanceAtr,
            double triggerExtensionAtr,
            string reason)
        {
            Direction = direction;
            Mode = mode;
            IsValid = isValid;
            InsideZone = insideZone;
            TriggerReached = triggerReached;
            IsLate = isLate;
            Market = market;
            ZoneLow = zoneLow;
            ZoneHigh = zoneHigh;
            ZoneTolerance = zoneTolerance;
            IdealEntry = idealEntry;
            Trigger = trigger;
            Anchor = anchor;
            ActualEntry = actualEntry;
            EntryDistanceAtr = entryDistanceAtr < 0 ? 0 : entryDistanceAtr;
            ZoneDistanceAtr = zoneDistanceAtr < 0 ? 0 : zoneDistanceAtr;
            TriggerExtensionAtr = triggerExtensionAtr < 0 ? 0 : triggerExtensionAtr;
            Reason = string.IsNullOrWhiteSpace(reason) ? "NOT EVALUATED" : reason;
        }

        public static EntryGeometrySnapshot Invalid(string reason)
        {
            return new EntryGeometrySnapshot(
                0,
                ExecutionMode.None,
                false,
                false,
                false,
                false,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                reason);
        }
    }
}
