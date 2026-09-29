using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private WaveTrendSnapshot GetWaveTrendSnapshot(
            Bars bars,
            int index)
        {
            if (!UseWaveTrendEvidence ||
                bars == null ||
                index < 1 ||
                index >= bars.Count)
                return default(WaveTrendSnapshot);

            if (!_waveTrendEngines.TryGetValue(
                    bars,
                    out WaveTrendEngine engine))
            {
                engine =
                    new WaveTrendEngine(
                        bars,
                        WaveTrendLength,
                        WaveTrendMomentumLength,
                        WaveTrendSmoothMA,
                        WaveTrendSmoothingLength,
                        WaveTrendSignalMA,
                        WaveTrendSignalLength,
                        WaveTrendOs1,
                        WaveTrendOs2,
                        WaveTrendOb1,
                        WaveTrendOb2);

                _waveTrendEngines[bars] = engine;
            }

            return engine.GetSnapshot(index);
        }

        private void ApplyWaveTrendEvidence(
            Frame frame,
            Bars bars,
            int index)
        {
            if (frame == null)
                return;

            WaveTrendSnapshot snapshot =
                GetWaveTrendSnapshot(
                    bars,
                    index);

            WaveTrendEvidenceResult result =
                WaveTrendEvidenceRule.Evaluate(
                    snapshot,
                    WaveTrendOs1,
                    WaveTrendOb1,
                    MinimumWaveTrendQuality);

            frame.WaveTrend =
                snapshot.Wave;
            frame.WaveTrendSignal =
                snapshot.Signal;
            frame.WaveTrendHistogram =
                snapshot.Histogram;
            frame.WaveTrendPrevious =
                snapshot.PreviousWave;
            frame.WaveTrendSignalPrevious =
                snapshot.PreviousSignal;
            frame.WaveTrendDelta =
                snapshot.WaveDelta;
            frame.WaveTrendQuality =
                result.Quality;
            frame.WaveTrendDirection =
                result.Direction;
            frame.WaveTrendBull =
                result.Bull;
            frame.WaveTrendBear =
                result.Bear;
            frame.WaveTrendBullCross =
                snapshot.BullCross;
            frame.WaveTrendBearCross =
                snapshot.BearCross;
            frame.WaveTrendAboveZero =
                snapshot.AboveZero;
            frame.WaveTrendBelowZero =
                snapshot.BelowZero;
            frame.WaveTrendOversold =
                snapshot.Oversold;
            frame.WaveTrendOverbought =
                snapshot.Overbought;
        }
    }
}
