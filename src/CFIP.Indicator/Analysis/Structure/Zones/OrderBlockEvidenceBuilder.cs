using System;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool TryBuildOrderBlockImpulseEvidence(
            Bars bars,
            int createdIndex,
            int currentIndex,
            int direction,
            double atr,
            out int impulseEnd,
            out bool displacement,
            out double strongestBody,
            out bool structureBreak)
        {
            impulseEnd =
                Math.Min(
                    currentIndex,
                    createdIndex +
                    Math.Max(
                        2,
                        ObImpulseBars));
            displacement = false;
            strongestBody = 0;

            for (int j =
                     createdIndex + 1;
                 j <= impulseEnd;
                 j++)
            {
                double nextBody =
                    Math.Abs(
                        bars.ClosePrices[j] -
                        bars.OpenPrices[j]);

                bool directional =
                    direction == 1
                        ? bars.ClosePrices[j] >
                          bars.OpenPrices[j]
                        : bars.ClosePrices[j] <
                          bars.OpenPrices[j];

                if (directional &&
                    nextBody >=
                    atr *
                    ObDisplacementAtr)
                {
                    displacement = true;
                    strongestBody =
                        Math.Max(
                            strongestBody,
                            nextBody);
                }
            }

            if (RequireObDisplacement &&
                !displacement)
                return false;

            structureBreak = false;

            int structureStart =
                Math.Max(
                    1,
                    createdIndex -
                    Math.Max(
                        3,
                        ObStructureLookback));

            if (direction == 1)
            {
                double priorHigh =
                    Highest(
                        bars,
                        structureStart,
                        createdIndex - 1);

                for (int j =
                         createdIndex + 1;
                     j <= impulseEnd;
                     j++)
                {
                    if (bars.ClosePrices[j] >
                        priorHigh +
                        atr *
                        StructureBreakAtr)
                    {
                        structureBreak = true;
                        break;
                    }
                }
            }
            else
            {
                double priorLow =
                    Lowest(
                        bars,
                        structureStart,
                        createdIndex - 1);

                for (int j =
                         createdIndex + 1;
                     j <= impulseEnd;
                     j++)
                {
                    if (bars.ClosePrices[j] <
                        priorLow -
                        atr *
                        StructureBreakAtr)
                    {
                        structureBreak = true;
                        break;
                    }
                }
            }

            return true;
        }
    }
}
