using System;

namespace cAlgo
{
    internal readonly struct StructuralStopGeometrySnapshot
    {
        public bool IsValid { get; }
        public int Direction { get; }
        public double Entry { get; }
        public double SourcePrice { get; }
        public double FrameAtr { get; }
        public double BufferAtr { get; }
        public double RawStop { get; }
        public double Stop { get; }
        public double Risk { get; }
        public double RiskAtr { get; }
        public string Reason { get; }

        public StructuralStopGeometrySnapshot(
            bool isValid,
            int direction,
            double entry,
            double sourcePrice,
            double frameAtr,
            double bufferAtr,
            double rawStop,
            double stop,
            double risk,
            double riskAtr,
            string reason)
        {
            IsValid = isValid;
            Direction = direction;
            Entry = entry;
            SourcePrice = sourcePrice;
            FrameAtr = frameAtr;
            BufferAtr = bufferAtr;
            RawStop = rawStop;
            Stop = stop;
            Risk = Math.Max(0, risk);
            RiskAtr = Math.Max(0, riskAtr);
            Reason = string.IsNullOrWhiteSpace(reason)
                ? (isValid ? "VALID" : "INVALID")
                : reason;
        }

        public static StructuralStopGeometrySnapshot CreateInvalid(
            int direction,
            double entry,
            double sourcePrice,
            double frameAtr,
            double bufferAtr,
            string reason)
        {
            return new StructuralStopGeometrySnapshot(
                false,
                direction,
                entry,
                sourcePrice,
                frameAtr,
                bufferAtr,
                0,
                0,
                0,
                0,
                reason);
        }
    }
}
