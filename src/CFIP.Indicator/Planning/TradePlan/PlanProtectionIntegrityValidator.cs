// CFIP Indicator — PlanProtectionIntegrityValidator.cs
// Validate entry quality and protective stop/target integrity.

using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool ValidatePlanProtectionAndEntry(
            Plan plan,
            int direction)
        {
            if (!IsValidStop(
                    direction,
                    plan.Entry,
                    plan.Stop))
                return false;

            if (!PriceProtectionRule.ValidateTarget(
                    direction,
                    plan.Entry,
                    plan.Tp1,
                    0))
                return false;

            if (!IsValidTarget(
                    direction,
                    plan.Entry,
                    plan.Tp1))
                return false;

            if (!IsFinitePositive(plan.Risk))
                return false;

            if (RequirePrecisionEntry &&
                plan.EntryQuality <
                ActionabilityThresholdPolicy.EffectivePrecisionEntryQualityFloor(
                    MinimumEntryQuality))
                return false;

            return true;
        }
    }
}
