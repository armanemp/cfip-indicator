// CFIP Indicator — PlanIntegrityValidator.cs
// Thin trade-plan integrity orchestration boundary.

using System;
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

            PlanRewardRiskQualityResult rewardRisk =
                PlanRewardRiskQualityRule.Evaluate(
                    direction,
                    plan.Entry,
                    plan.Stop,
                    plan.Tp1,
                    atr,
                    checkSpread
                        ? Math.Max(
                            0,
                            Symbol.Ask - Symbol.Bid)
                        : 0,
                    Math.Max(
                        Tp1MinimumRR,
                        MinimumRequiredRRForRegime(
                            _decision == null
                                ? "UNKNOWN"
                                : _decision.Regime)),
                    PreferredStopRiskAtr,
                    StructuralStopRiskRule.EffectiveMaximumStopRiskAtr(
                        MinimumSlAtr,
                        MaximumSlAtr,
                        MaximumStructuralStopAtr),
                        Math.Max(0, MaximumRewardRR),
                        Symbol.PipSize);

            if (!rewardRisk.Allowed)
                return false;

            return ValidatePlanMarketConstraints(
                plan,
                referenceEntry,
                atr,
                checkSpread);
        }
    }
}
