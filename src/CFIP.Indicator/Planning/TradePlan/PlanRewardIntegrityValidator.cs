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
                Math.Max(
                    Tp1MinimumRR,
                    MinimumRequiredRR());

            double maximumRR =
                Math.Max(
                    minimumRR,
                    MaximumRewardRR);

            if (!IsFinitePositive(minimumRR) ||
                !IsFinitePositive(maximumRR))
                return RejectPlanRewardStructure("INVALID REWARD RR LIMITS");

            RiskRewardMathResult tp1Geometry =
                RiskRewardMathRule.Evaluate(
                    direction,
                    plan.Entry,
                    plan.Stop,
                    plan.Tp1,
                    0,
                    minimumRR,
                    maximumRR,
                    Symbol.PipSize);

            if (!tp1Geometry.Valid)
                return RejectPlanRewardStructure("TP1 RR OUT OF RANGE");

            double tp1RR =
                tp1Geometry.NominalRR;

            if (!IsFinitePositive(tp1RR))
                return RejectPlanRewardStructure("TP1 RR INVALID");

            if (Math.Abs(
                    plan.Risk -
                    tp1Geometry.Risk) >
                Math.Max(
                    Symbol.TickSize,
                    1e-9))
                return RejectPlanRewardStructure("PLAN RISK DRIFT");

            if (plan.Tp2 > 0)
            {
                double requiredTp2RR =
                    Math.Max(
                        Tp2MinimumRR,
                        tp1RR +
                        Math.Max(
                            0.10,
                            StructuralTpRrStep));

                RiskRewardMathResult tp2Geometry =
                    RiskRewardMathRule.Evaluate(
                        direction,
                        plan.Entry,
                        plan.Stop,
                        plan.Tp2,
                        0,
                        requiredTp2RR,
                        maximumRR,
                        Symbol.PipSize);

                double rr = tp2Geometry.NominalRR;

                if (!tp2Geometry.Valid ||
                    !IsFinitePositive(rr))
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
                double previousRR =
                    plan.Tp2 > 0
                        ? RiskRewardMathRule.Evaluate(
                            direction,
                            plan.Entry,
                            plan.Stop,
                            plan.Tp2,
                            0,
                            0,
                            double.PositiveInfinity,
                            Symbol.PipSize).NominalRR
                        : tp1RR;

                double requiredTp3RR =
                    Math.Max(
                        Tp3MinimumRR,
                        previousRR +
                        Math.Max(
                            0.10,
                            StructuralTpRrStep));

                RiskRewardMathResult tp3Geometry =
                    RiskRewardMathRule.Evaluate(
                        direction,
                        plan.Entry,
                        plan.Stop,
                        plan.Tp3,
                        0,
                        requiredTp3RR,
                        maximumRR,
                        Symbol.PipSize);

                double rr = tp3Geometry.NominalRR;

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
                    return RejectPlanRewardStructure("TP3 RR OUT OF RANGE");

                if (RequireHtfRewardForTp2Plus &&
                    !IsHtfSource(
                        plan.Tp3Source))
                    return RejectPlanRewardStructure("TP3 HTF SOURCE REQUIRED");
            }

            if (plan.Tp4 > 0)
            {
                double previousRR =
                    plan.Tp3 > 0
                        ? RiskRewardMathRule.Evaluate(
                            direction,
                            plan.Entry,
                            plan.Stop,
                            plan.Tp3,
                            0,
                            0,
                            double.PositiveInfinity,
                            Symbol.PipSize).NominalRR
                        : plan.Tp2 > 0
                            ? RiskRewardMathRule.Evaluate(
                                direction,
                                plan.Entry,
                                plan.Stop,
                                plan.Tp2,
                                0,
                                0,
                                double.PositiveInfinity,
                                Symbol.PipSize).NominalRR
                            : tp1RR;

                double requiredTp4RR =
                    Math.Max(
                        Tp4MinimumRR,
                        previousRR +
                        Math.Max(
                            0.10,
                            StructuralTpRrStep));

                RiskRewardMathResult tp4Geometry =
                    RiskRewardMathRule.Evaluate(
                        direction,
                        plan.Entry,
                        plan.Stop,
                        plan.Tp4,
                        0,
                        requiredTp4RR,
                        maximumRR,
                        Symbol.PipSize);

                double rr = tp4Geometry.NominalRR;

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

            bool hasTp2Plus =
                plan.Tp2 > 0 ||
                plan.Tp3 > 0 ||
                plan.Tp4 > 0;

            if (RequireHtfRewardForTp2Plus &&
                hasTp2Plus &&
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
