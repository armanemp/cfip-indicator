using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool HasHigherTfZonePathObstacle(
            DateTime reference,
            int direction,
            double entry,
            double target)
        {
            Bars[] frames =
            {
                _m15Bars,
                _m30Bars,
                _h1Bars,
                _h4Bars
            };

            for (int i = 0;
                 i < frames.Length;
                 i++)
            {
                Bars frame =
                    frames[i];

                if (frame == null)
                    continue;

                int index =
                    ClosedIndex(
                        frame,
                        reference);

                if (index < 8)
                    continue;

                double frameAtr =
                    Atr(
                        frame,
                        index);

                if (frameAtr <= 0)
                    continue;

                if (HasOpposingZonePathObstacle(
                        frame,
                        index,
                        direction,
                        entry,
                        target,
                        frameAtr))
                    return true;
            }

            return false;
        }
    }
}
