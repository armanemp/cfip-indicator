using System;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private double MaterializeStructuralStop(
            Level best,
            int closedM5,
            int direction,
            double atr)
        {
            double finalFrameAtr =
                ResolveStructuralStopFrameAtr(
                    best,
                    closedM5,
                    atr);

            double finalBuffer =
                finalFrameAtr *
                Math.Max(
                    0.02,
                    best.Timeframe == "M5"
                        ? StopBufferAtr
                        : Math.Max(
                            StopBufferAtr,
                            HtfStopBufferAtr));

            double selectedStop =
                direction == 1
                    ? best.Price - finalBuffer
                    : best.Price + finalBuffer;

            return NormalizePrice(
                selectedStop);
        }
    }
}
