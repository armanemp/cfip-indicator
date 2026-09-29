namespace cAlgo
{
    internal readonly struct TradeActionabilityResult
    {
        public bool Actionable { get; }
        public int LocationQuality { get; }
        public int TimingQuality { get; }
        public int PricePositionQuality { get; }
        public double EntryDistanceAtr { get; }
        public double Tp1RR { get; }
        public int DivergenceQuality { get; }
        public int DivergenceDirection { get; }
        public string DivergenceType { get; }
        public string Reason { get; }

        public TradeActionabilityResult(
            bool actionable,
            int locationQuality,
            int timingQuality,
            int pricePositionQuality,
            double entryDistanceAtr,
            double tp1RR,
            int divergenceQuality,
            int divergenceDirection,
            string divergenceType,
            string reason)
        {
            Actionable = actionable;
            LocationQuality = NumericGuards.ClampInt(locationQuality, 0, 100);
            TimingQuality = NumericGuards.ClampInt(timingQuality, 0, 100);
            PricePositionQuality = NumericGuards.ClampInt(pricePositionQuality, 0, 100);
            EntryDistanceAtr = entryDistanceAtr < 0 ? 0 : entryDistanceAtr;
            Tp1RR = tp1RR < 0 ? 0 : tp1RR;
            DivergenceQuality = NumericGuards.ClampInt(divergenceQuality, 0, 100);
            DivergenceDirection = divergenceDirection == 1 || divergenceDirection == -1 ? divergenceDirection : 0;
            DivergenceType = divergenceType ?? "NONE";
            Reason = string.IsNullOrWhiteSpace(reason) ? "NOT EVALUATED" : reason;
        }

        public static TradeActionabilityResult Blocked(
            string reason,
            int divergenceQuality = 0,
            int divergenceDirection = 0,
            string divergenceType = "NONE")
        {
            return new TradeActionabilityResult(
                false,
                0,
                0,
                0,
                0,
                0,
                divergenceQuality,
                divergenceDirection,
                divergenceType,
                reason);
        }
    }
}
