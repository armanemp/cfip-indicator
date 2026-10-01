// CFIP Indicator — PlanMarketConstraintValidator.cs
// Validate market, spread and entry-extension constraints for a trade plan.

using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool ValidatePlanMarketConstraints(
            Plan plan,
            double referenceEntry,
            double atr,
            bool checkSpread)
        {
            if (checkSpread &&
                UseSpreadFilter)
            {
                CanonicalPriceSnapshot priceSnapshot =
                    GetCanonicalPriceSnapshot();

                if (priceSnapshot == null ||
                    !priceSnapshot.IsQuoteValid)
                    return false;

                double spread =
                    Math.Max(
                        0,
                        priceSnapshot.Spread);

                if (spread > 0 &&
                    plan.Risk > 0 &&
                    spread / plan.Risk >
                    Math.Max(
                        0.02,
                        MaximumSpreadToStopRiskRatio))
                    return false;
            }

            if (Math.Abs(
                    plan.Entry -
                    referenceEntry) >
                atr *
                Math.Max(
                    0.10,
                    MaximumEntryExtensionAtr))
                return false;

            if (MinimumSmartTargetQualityForTp1 > 0 &&
                plan.Tp1Quality > 0 &&
                plan.Tp1Quality <
                MinimumSmartTargetQualityForTp1)
                return false;

            return true;
        }
    }
}
