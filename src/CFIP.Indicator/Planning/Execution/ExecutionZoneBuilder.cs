using System;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool TryBuildExecutionZone(
            int closedM5,
            int direction,
            out double atr,
            out double market,
            out double low,
            out double high,
            out double ideal,
            out double tolerance,
            out double triggerBuffer,
            out string source,
            out int quality)
        {
            atr = 0;
            market = 0;
            low = 0;
            high = 0;
            ideal = 0;
            tolerance = 0;
            triggerBuffer = 0;
            source = "NONE";
            quality = 0;

            if (_m5Bars == null ||
                closedM5 < 20 ||
                (direction != 1 &&
                 direction != -1))
                return false;

            atr =
                Atr(
                    _m5Bars,
                    closedM5);

            if (atr <= 0)
                return false;

            CanonicalPriceSnapshot priceSnapshot =
                GetCanonicalPriceSnapshot();

            if (priceSnapshot == null ||
                !priceSnapshot.IsQuoteValid)
                return false;

            market =
                NormalizePrice(
                    priceSnapshot.GetExecutablePrice(direction));

            if (!TrySelectExecutionZoneCandidate(
                    closedM5,
                    direction,
                    atr,
                    market,
                    out low,
                    out high,
                    out source,
                    out quality))
                return false;

            quality =
                EvaluateExecutionZoneQuality(
                    closedM5,
                    direction,
                    quality);

            ideal =
                low +
                (high - low) *
                0.50;

            tolerance =
                atr *
                Math.Max(
                    0.02,
                    ExecutionZoneAtr);

            triggerBuffer =
                atr *
                Math.Max(
                    0.01,
                    PrecisionBreakoutBufferAtr);

            return true;
        }
    }
}
