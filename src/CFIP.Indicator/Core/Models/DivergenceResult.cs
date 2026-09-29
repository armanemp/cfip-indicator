namespace cAlgo
{
    internal readonly struct DivergenceResult
    {
        public int Direction { get; }
        public int Quality { get; }
        public string Type { get; }
        public bool RegularBull { get; }
        public bool RegularBear { get; }
        public bool HiddenBull { get; }
        public bool HiddenBear { get; }

        public bool HasSignal =>
            Direction != 0 &&
            Quality > 0 &&
            !string.IsNullOrWhiteSpace(Type);

        public DivergenceResult(
            int direction,
            int quality,
            string type,
            bool regularBull,
            bool regularBear,
            bool hiddenBull,
            bool hiddenBear)
        {
            Direction = direction == 1 || direction == -1 ? direction : 0;
            Quality = NumericGuards.ClampInt(quality, 0, 100);
            Type = type ?? "NONE";
            RegularBull = regularBull;
            RegularBear = regularBear;
            HiddenBull = hiddenBull;
            HiddenBear = hiddenBear;
        }

        public static DivergenceResult CreateNoDivergence()
        {
            return new DivergenceResult(
                0,
                0,
                "NONE",
                false,
                false,
                false,
                false);
        }
    }
}
