using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private int HigherTimeframeConfidencePenalty(int direction)
        {
            if (direction == 0 ||
                HigherTfPenalty <= 0)
                return 0;

            double opposingInfluence = 0;
            double availableInfluence = 0;

            AccumulateHigherTimeframeInfluence(
                _h1Frame,
                H1Weight,
                direction,
                ref opposingInfluence,
                ref availableInfluence);

            AccumulateHigherTimeframeInfluence(
                _h4Frame,
                H4Weight,
                direction,
                ref opposingInfluence,
                ref availableInfluence);

            AccumulateHigherTimeframeInfluence(
                _d1Frame,
                D1Weight,
                direction,
                ref opposingInfluence,
                ref availableInfluence);

            if (SmartWeeklyContext)
            {
                AccumulateHigherTimeframeInfluence(
                    _w1Frame,
                    W1Weight,
                    direction,
                    ref opposingInfluence,
                    ref availableInfluence);
            }

            if (availableInfluence <= 0 ||
                opposingInfluence <= 0)
                return 0;

            // Confidence penalty is proportional to the quality-weighted
            // higher-timeframe opposition. One weak HTF frame must not have
            // the same effect as several strong opposing frames.
            double oppositionRatio =
                Math.Max(
                    0.0,
                    Math.Min(
                        1.0,
                        opposingInfluence / availableInfluence));

            return Math.Max(
                0,
                (int)Math.Round(
                    HigherTfPenalty *
                    oppositionRatio *
                    1.5));

        }

        private static void AccumulateHigherTimeframeInfluence(
            Frame frame,
            double weight,
            int direction,
            ref double opposingInfluence,
            ref double availableInfluence)
        {
            if (frame == null ||
                direction == 0 ||
                weight <= 0 ||
                frame.Quality <= 0 ||
                frame.Direction == 0)
                return;

            double influence =
                Math.Max(
                    0.0,
                    Math.Min(
                        1.0,
                        frame.Quality / 100.0)) *
                weight;

            availableInfluence += influence;

            if (frame.Direction != direction)
                opposingInfluence += influence;
        }
    }
}