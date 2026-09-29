namespace cAlgo
{
    internal readonly struct WaveTrendSnapshot
    {
        public bool Valid { get; }
        public double Wave { get; }
        public double Signal { get; }
        public double Histogram { get; }
        public double PreviousWave { get; }
        public double PreviousSignal { get; }
        public double WaveDelta { get; }
        public bool BullCross { get; }
        public bool BearCross { get; }
        public bool AboveZero { get; }
        public bool BelowZero { get; }
        public bool Oversold { get; }
        public bool Overbought { get; }
        public bool Rising { get; }
        public bool Falling { get; }

        public WaveTrendSnapshot(
            bool valid,
            double wave,
            double signal,
            double histogram,
            double previousWave,
            double previousSignal,
            double waveDelta,
            bool bullCross,
            bool bearCross,
            bool aboveZero,
            bool belowZero,
            bool oversold,
            bool overbought,
            bool rising,
            bool falling)
        {
            Valid = valid;
            Wave = wave;
            Signal = signal;
            Histogram = histogram;
            PreviousWave = previousWave;
            PreviousSignal = previousSignal;
            WaveDelta = waveDelta;
            BullCross = bullCross;
            BearCross = bearCross;
            AboveZero = aboveZero;
            BelowZero = belowZero;
            Oversold = oversold;
            Overbought = overbought;
            Rising = rising;
            Falling = falling;
        }
    }
}
