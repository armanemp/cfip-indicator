namespace cAlgo
{
    internal readonly struct VolumeProfileSnapshot
    {
        public bool IsValid { get; }
        public int ClosedIndex { get; }
        public int BinCount { get; }
        public double ProfileLow { get; }
        public double ProfileHigh { get; }
        public double BinSize { get; }
        public double POC { get; }
        public double VAL { get; }
        public double VAH { get; }
        public double TotalVolume { get; }
        public double PocVolume { get; }

        public VolumeProfileSnapshot(
            bool isValid,
            int closedIndex,
            int binCount,
            double profileLow,
            double profileHigh,
            double binSize,
            double poc,
            double val,
            double vah,
            double totalVolume,
            double pocVolume)
        {
            IsValid = isValid;
            ClosedIndex = closedIndex;
            BinCount = binCount;
            ProfileLow = profileLow;
            ProfileHigh = profileHigh;
            BinSize = binSize;
            POC = poc;
            VAL = val;
            VAH = vah;
            TotalVolume = totalVolume;
            PocVolume = pocVolume;
        }

        public static VolumeProfileSnapshot Empty =>
            new VolumeProfileSnapshot(
                false,
                -1,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0);
    }
}
