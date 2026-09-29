namespace cAlgo
{
    internal readonly struct EmpiricalCalibrationSnapshot
    {
        public bool Available { get; }
        public int Adjustment { get; }
        public int Samples { get; }
        public int Wins { get; }
        public int ConfidenceBucket { get; }
        public double ObservedWinRate { get; }
        public string Source { get; }

        public EmpiricalCalibrationSnapshot(
            bool available,
            int adjustment,
            int samples,
            int wins,
            int confidenceBucket,
            double observedWinRate,
            string source)
        {
            Available = available;
            Adjustment = NumericGuards.ClampInt(
                adjustment,
                -100,
                100);
            Samples = samples < 0 ? 0 : samples;
            Wins = wins < 0 ? 0 : wins;
            ConfidenceBucket = confidenceBucket;
            ObservedWinRate =
                NumericGuards.Clamp(
                    observedWinRate,
                    0,
                    1);
            Source =
                string.IsNullOrWhiteSpace(source)
                    ? "NONE"
                    : source;
        }

        public static EmpiricalCalibrationSnapshot None(
            int confidenceBucket)
        {
            return new EmpiricalCalibrationSnapshot(
                false,
                0,
                0,
                0,
                confidenceBucket,
                0.5,
                "NONE");
        }
    }
}