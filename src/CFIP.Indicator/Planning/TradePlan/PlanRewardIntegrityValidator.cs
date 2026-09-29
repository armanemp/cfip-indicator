// CFIP Indicator — PlanRewardIntegrityValidator.cs
// Validate reward, target progression and higher-timeframe target constraints.

using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool ValidatePlanRewardStructure(
            Plan plan,
            int direction)
        {
            if (plan == null ||
                !IsFinitePositive(plan.Entry) ||
                !IsFinitePositive(plan.Risk) ||
                !IsFinitePositive(plan.Tp1) ||
                plan.Tp2 < 0 ||
                plan.Tp3 < 0 ||
                plan.Tp4 < 0 ||
                double.IsNaN(plan.Tp2) ||
                double.IsInfinity(plan.Tp2) ||
                double.IsNaN(plan.Tp3) ||
                double.IsInfinity(plan.Tp3) ||
                double.IsNaN(plan.Tp4) ||
                double.IsInfinity(plan.Tp4))
                return false;

            double minimumRR =
                Math.Max(
                    Tp1MinimumRR,
                    MinimumRequiredRR());

            double maximumRR =
                Math.Max(
                    minimumRR,
                    MaximumRewardRR);

            if (!IsFinitePositive(minimumRR) ||
                !IsFinitePositive(maximumRR))
                return false;

            double tp1RR =
                Math.Abs(
                    plan.Tp1 -
                    plan.Entry) /
                plan.Risk;

            if (!IsFinitePositive(tp1RR) ||
                tp1RR < minimumRR ||
                tp1RR > maximumRR)
                return false;

            if (plan.Tp2 > 0)
            {
                double rr =
                    Math.Abs(
                        plan.Tp2 -
                        plan.Entry) /
                    plan.Risk;

                if (!IsFinitePositive(rr) ||
                    rr <
                        Math.Max(
                            Tp2MinimumRR,
                            tp1RR +
                            Math.Max(
                                0.10,
                                StructuralTpRrStep)) ||
                    rr > maximumRR)
                    return false;

                if (RequireHtfRewardForTp2Plus &&
                    !IsHtfSource(
                        plan.Tp2Source))
                    return false;
            }
            else if (MinimumTargetsForPlan >= 2)
            {
                return false;
            }

            if (plan.Tp3 > 0)
            {
                double rr =
                    Math.Abs(
                        plan.Tp3 -
                        plan.Entry) /
                    plan.Risk;

                double previousRR =
                    plan.Tp2 > 0
                        ? Math.Abs(
                            plan.Tp2 -
                            plan.Entry) /
                          plan.Risk
                        : tp1RR;

                if (!IsFinitePositive(rr) ||
                    !IsFinitePositive(previousRR) ||
                    rr <
                        Math.Max(
                            Tp3MinimumRR,
                            previousRR +
                            Math.Max(
                                0.10,
                                StructuralTpRrStep)) ||
                    rr > maximumRR)
                    return false;

                if (RequireHtfRewardForTp2Plus &&
                    !IsHtfSource(
                        plan.Tp3Source))
                    return false;
            }

            if (plan.Tp4 > 0)
            {
                double rr =
                    Math.Abs(
                        plan.Tp4 -
                        plan.Entry) /
                plan.Risk;

                double previousRR =
                    plan.Tp3 > 0
                        ? Math.Abs(
                            plan.Tp3 -
                            plan.Entry) /
                          plan.Risk
                        : plan.Tp2 > 0
                            ? Math.Abs(
                                plan.Tp2 -
                                plan.Entry) /
                              plan.Risk
                            : tp1RR;

                if (!IsFinitePositive(rr) ||
                    !IsFinitePositive(previousRR) ||
                    rr <
                        Math.Max(
                            Tp4MinimumRR,
                            previousRR +
                            Math.Max(
                                0.10,
                                StructuralTpRrStep)) ||
                    rr > maximumRR)
                    return false;

                if (RequireHtfRewardForTp2Plus &&
                    !IsHtfSource(
                        plan.Tp4Source))
                    return false;
            }

            if (RequireHtfRewardForTp1 &&
                !IsHtfSource(
                    plan.Tp1Source))
                return false;

            if (RequireHtfRewardForTp2Plus &&
                plan.HtfTargetCount <= 0)
                return false;

            if (plan.Tp2 > 0 &&
                !IsProgressiveTarget(
                    direction,
                    plan.Tp1,
                    plan.Tp2))
                return false;

            if (plan.Tp3 > 0 &&
                !IsProgressiveTarget(
                    direction,
                    plan.Tp2 > 0
                        ? plan.Tp2
                        : plan.Tp1,
                    plan.Tp3))
                return false;

            if (plan.Tp4 > 0 &&
                !IsProgressiveTarget(
                    direction,
                    plan.Tp3 > 0
                        ? plan.Tp3
                        : plan.Tp2 > 0
                            ? plan.Tp2
                            : plan.Tp1,
                    plan.Tp4))
                return false;

            return true;
        }
    }
}
