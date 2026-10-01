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

            if (plan.EntryMode ==
                    ExecutionMode.BreakoutMarket ||
                plan.EntryMode ==
                    ExecutionMode.RetestMarket)
            {
                EntryGeometrySnapshot geometry =
                    EntryGeometryRule.Evaluate(
                        plan.Direction,
                        plan.EntryMode,
                        referenceEntry,
                        plan.EntryZoneLow,
                        plan.EntryZoneHigh,
                        plan.EntryZoneTolerance,
                        plan.IdealEntry,
                        plan.EntryTrigger,
                        plan.Entry,
                        atr,
                        Symbol.TickSize,
                        Symbol.PipSize,
                        plan.EntryMode ==
                            ExecutionMode.BreakoutMarket,
                        false,
                        MaximumEntryExtensionAtr,
                        MaximumEntryDistanceAtr);

                if (!geometry.IsValid ||
                    geometry.Mode != plan.EntryMode ||
                    geometry.IsLate)
                    return false;
            }

            if (MinimumSmartTargetQualityForTp1 > 0 &&
                plan.Tp1Quality > 0 &&
                plan.Tp1Quality <
                MinimumSmartTargetQualityForTp1)
                return false;

            return true;
        }
    }
}
