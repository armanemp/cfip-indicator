// CFIP Indicator — PlanRewardIntegrityValidator.cs
// Validate reward, target progression and higher-timeframe target constraints.

using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private int _lastPlanRewardRejectionM5 = int.MinValue;
        private string _lastPlanRewardRejectionReason = "";

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
                return RejectPlanRewardStructure("INVALID PLAN GEOMETRY");

            if (!PriceProtectionRule.ValidateTarget(
                    direction,
                    plan.Entry,
                    plan.Tp1,
                    0))
                return RejectPlanRewardStructure("TP1 DIRECTION INVALID");

            double minimumRR =
                RiskRewardPolicyRule.NormalizeMinimum(
                    MinimumRequiredRR(),
                    Tp1MinimumRR);

            double maximumRR =
                RiskRewardPolicyRule.NormalizeMaximum(
                    MaximumRewardRR,
                    minimumRR);

            if (!IsFinitePositive(minimumRR) ||
                !IsFinitePositive(maximumRR))
                return RejectPlanRewardStructure("INVALID REWARD RR LIMITS");

            double tp1RR =
                RiskRewardGeometryRule.CalculateNominalRR(
                    plan.Entry,
                    plan.Tp1,
                    plan.Risk);

            if (!IsFinitePositive(tp1RR) ||
                !RiskRewardPolicyRule.MeetsMinimum(
                    tp1RR,
                    minimumRR) ||
                !RiskRewardPolicyRule.IsWithinMaximum(
                    tp1RR,
                    maximumRR))
                return RejectPlanRewardStructure("TP1 RR OUT OF RANGE");

            if (plan.Tp2 > 0)
            {
                double rr =
                    RiskRewardGeometryRule.CalculateNominalRR(
                        plan.Entry,
                        plan.Tp2,
                        plan.Risk);

                if (!IsFinitePositive(rr) ||
                    !RiskRewardPolicyRule.MeetsMinimum(
                        rr,
                        Math.Max(
                            Tp2MinimumRR,
                            tp1RR +
                            Math.Max(
                                0.10,
                                StructuralTpRrStep))) ||
                    !RiskRewardPolicyRule.IsWithinMaximum(
                        rr,
                        maximumRR))
                    return RejectPlanRewardStructure("TP2 RR OUT OF RANGE");

                if (RequireHtfRewardForTp2Plus &&
                    !IsHtfSource(
                        plan.Tp2Source))
                    return RejectPlanRewardStructure("TP2 HTF SOURCE REQUIRED");
            }
            else if (MinimumTargetsForPlan >= 2)
            {
                return RejectPlanRewardStructure("TP2 REQUIRED");
            }

            if (plan.Tp3 > 0)
            {
                double rr =
                    RiskRewardGeometryRule.CalculateNominalRR(
                        plan.Entry,
                        plan.Tp3,
                        plan.Risk);

                double previousRR =
                    plan.Tp2 > 0
                        ? RiskRewardGeometryRule.CalculateNominalRR(
                            plan.Entry,
                            plan.Tp2,
                            plan.Risk)
                        : tp1RR;

                if (!IsFinitePositive(rr) ||
                    !IsFinitePositive(previousRR) ||
                    !RiskRewardPolicyRule.MeetsMinimum(
                        rr,
                        Math.Max(
                            Tp3MinimumRR,
                            previousRR +
                            Math.Max(
                                0.10,
                                StructuralTpRrStep))) ||
                    !RiskRewardPolicyRule.IsWithinMaximum(
                        rr,
                        maximumRR))
                    return RejectPlanRewardStructure("TP3 RR OUT OF RANGE");

                if (RequireHtfRewardForTp2Plus &&
                    !IsHtfSource(
                        plan.Tp3Source))
                    return RejectPlanRewardStructure("TP3 HTF SOURCE REQUIRED");
            }

            if (plan.Tp4 > 0)
            {
                double rr =
                    RiskRewardGeometryRule.CalculateNominalRR(
                        plan.Entry,
                        plan.Tp4,
                        plan.Risk);

                double previousRR =
                    plan.Tp3 > 0
                        ? RiskRewardGeometryRule.CalculateNominalRR(
                            plan.Entry,
                            plan.Tp3,
                            plan.Risk)
                        : plan.Tp2 > 0
                            ? RiskRewardGeometryRule.CalculateNominalRR(
                                plan.Entry,
                                plan.Tp2,
                                plan.Risk)
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
                    return RejectPlanRewardStructure("TP4 RR OUT OF RANGE");

                if (RequireHtfRewardForTp2Plus &&
                    !IsHtfSource(
                        plan.Tp4Source))
                    return RejectPlanRewardStructure("TP4 HTF SOURCE REQUIRED");
            }

            if (RequireHtfRewardForTp1 &&
                !IsHtfSource(
                    plan.Tp1Source))
                return RejectPlanRewardStructure("TP1 HTF SOURCE REQUIRED");

            if (RequireHtfRewardForTp2Plus &&
                plan.HtfTargetCount <= 0)
                return RejectPlanRewardStructure("HTF TARGET REQUIRED");

            if (plan.Tp2 > 0 &&
                !IsProgressiveTarget(
                    direction,
                    plan.Tp1,
                    plan.Tp2))
                return RejectPlanRewardStructure("TP2 PROGRESSION INVALID");

            if (plan.Tp3 > 0 &&
                !IsProgressiveTarget(
                    direction,
                    plan.Tp2 > 0
                        ? plan.Tp2
                        : plan.Tp1,
                    plan.Tp3))
                return RejectPlanRewardStructure("TP3 PROGRESSION INVALID");

            if (plan.Tp4 > 0 &&
                !IsProgressiveTarget(
                    direction,
                    plan.Tp3 > 0
                        ? plan.Tp3
                        : plan.Tp2 > 0
                            ? plan.Tp2
                            : plan.Tp1,
                    plan.Tp4))
                return RejectPlanRewardStructure("TP4 PROGRESSION INVALID");

            return true;
        }
        private bool RejectPlanRewardStructure(
            string reason)
        {
            int m5 = Math.Max(-1, _lastEvaluatedM5);
            string normalizedReason =
                string.IsNullOrWhiteSpace(reason)
                    ? "UNSPECIFIED"
                    : reason.Trim();

            if (EnableOutcomeTelemetry &&
                (m5 != _lastPlanRewardRejectionM5 ||
                 !string.Equals(
                     normalizedReason,
                     _lastPlanRewardRejectionReason,
                     StringComparison.Ordinal)))
            {
                _lastPlanRewardRejectionM5 = m5;
                _lastPlanRewardRejectionReason = normalizedReason;

                RecordExecutionTelemetryHistory(
                    "PLAN_REWARD",
                    m5,
                    "REJECTED",
                    normalizedReason);
            }

            return false;
        }

    }
}
