namespace cAlgo
{
    internal readonly struct TopDownCalibrationGroupResult
    {
        public int Direction { get; }
        public int Alignment { get; }

        public TopDownCalibrationGroupResult(
            int direction,
            int alignment)
        {
            Direction = direction;
            Alignment = alignment;
        }
    }
}
