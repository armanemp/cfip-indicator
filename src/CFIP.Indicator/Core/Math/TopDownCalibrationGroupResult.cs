using System;

namespace cAlgo
{
    internal readonly struct TopDownCalibrationGroupResult
    {
        public int Direction { get; }
        public int Alignment { get; }
        public int AbsoluteStrength { get; }

        public TopDownCalibrationGroupResult(
            int direction,
            int alignment,
            int absoluteStrength)
        {
            Direction = direction;
            Alignment = Math.Max(0, Math.Min(100, alignment));
            AbsoluteStrength = Math.Max(0, Math.Min(100, absoluteStrength));
        }
    }
}
