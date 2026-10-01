namespace cAlgo
{
    internal sealed class OssIndicatorSettings
    {
        internal static OssIndicatorSettings Default { get; } =
            new OssIndicatorSettings();

        internal int MacdSignalPeriod { get; }
        internal int BollingerPeriod { get; }
        internal double BollingerStandardDeviations { get; }
        internal int MfiPeriod { get; }
        internal int StochLookbackPeriod { get; }
        internal int StochSignalPeriod { get; }
        internal int StochSmoothPeriod { get; }
        internal int SuperTrendPeriod { get; }
        internal double SuperTrendMultiplier { get; }
        internal int AroonPeriod { get; }
        internal int CciPeriod { get; }
        internal double ParabolicSarAccelerationFactor { get; }
        internal double ParabolicSarMaximumAccelerationFactor { get; }

        private OssIndicatorSettings()
        {
            MacdSignalPeriod = 9;
            BollingerPeriod = 20;
            BollingerStandardDeviations = 2.0;
            MfiPeriod = 14;
            StochLookbackPeriod = 14;
            StochSignalPeriod = 3;
            StochSmoothPeriod = 3;
            SuperTrendPeriod = 10;
            SuperTrendMultiplier = 3.0;
            AroonPeriod = 25;
            CciPeriod = 20;
            ParabolicSarAccelerationFactor = 0.02;
            ParabolicSarMaximumAccelerationFactor = 0.20;
        }
    }
}
