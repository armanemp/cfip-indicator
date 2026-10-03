namespace cAlgo
{
    internal sealed class M2PrecisionSnapshot
    {
        public static readonly M2PrecisionSnapshot Unavailable =
            new M2PrecisionSnapshot(
                -1,
                0,
                0,
                false,
                false);

        public int ClosedIndex { get; }
        public int Direction { get; }
        public int Quality { get; }
        public bool IsUsable { get; }
        public bool AlignsWithM5 { get; }

        public M2PrecisionSnapshot(
            int closedIndex,
            int direction,
            int quality,
            bool isUsable,
            bool alignsWithM5)
        {
            ClosedIndex = closedIndex;
            Direction = direction;
            Quality = quality;
            IsUsable = isUsable;
            AlignsWithM5 = alignsWithM5;
        }
    }
}
