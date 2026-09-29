namespace cAlgo
{
    internal readonly struct DivergenceResult
    {
        public int Direction { get; }
        public int Quality { get; }
        public string Type { get; }
        public bool BullRegular { get; }
        public bool BearRegular { get; }
        public bool BullHidden { get; }
        public bool BearHidden { get; }
        public int AgeBars { get; }
        public bool Valid { get; }

        public DivergenceResult(
            int direction,
            int quality,
            string type,
            bool bullRegular,
            bool bearRegular,
            bool bullHidden,
            bool bearHidden,
            int ageBars)
        {
            Direction =
                direction == 1 || direction == -1
                    ? direction
                    : 0;
            Quality =
                NumericGuards.ClampInt(
                    quality,
                    0,
                    100);
            Type =
                type ?? "NONE";
            BullRegular = bullRegular;
            BearRegular = bearRegular;
            BullHidden = bullHidden;
            BearHidden = bearHidden;
            AgeBars = System.Math.Max(0, ageBars);
            Valid =
                Direction != 0 &&
                Quality > 0;
        }

        public static DivergenceResult None()
        {
            return new DivergenceResult(
                0,
                0,
                "NONE",
                false,
                false,
                false,
                false,
                0);
        }

        public bool IsRegular
        {
            get { return BullRegular || BearRegular; }
        }

        public bool IsHidden
        {
            get { return BullHidden || BearHidden; }
        }
    }
}
