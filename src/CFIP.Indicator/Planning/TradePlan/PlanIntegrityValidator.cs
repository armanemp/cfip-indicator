// CFIP Indicator — PlanIntegrityValidator.cs
// Thin trade-plan integrity orchestration boundary.

using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool ValidatePlanIntegrity(
            Plan plan,
            int direction,
            double referenceEntry,
            double atr,
            bool checkSpread)
        {
            if (plan == null ||
                (direction != 1 &&
                 direction != -1) ||
                !IsFinitePositive(referenceEntry) ||
                !IsFinitePositive(atr))
                return false;

            if (!ValidatePlanProtectionAndEntry(
                    plan,
                    direction))
                return false;

            if (!ValidatePlanRewardStructure(
                    plan,
                    direction))
                return false;

            return ValidatePlanMarketConstraints(
                plan,
                referenceEntry,
                atr,
                checkSpread);
        }
    }
}
