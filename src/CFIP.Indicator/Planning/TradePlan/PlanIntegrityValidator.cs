// CFIP Indicator — PlanIntegrityValidator.cs
// Single-responsibility planning/risk module.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

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
                                atr <= 0)
                                return false;
                
                            if (!IsValidStop(
                                    direction,
                                    plan.Entry,
                                    plan.Stop))
                                return false;
                
                            if (!IsValidTarget(
                                    direction,
                                    plan.Entry,
                                    plan.Tp1))
                                return false;
                
                            if (plan.Risk <= 0)
                                return false;
                
                            if (RequirePrecisionEntry &&
                                plan.EntryQuality <
                                Math.Max(
                                    40,
                                    MinimumEntryQuality))
                                return false;
                
                            double minimumRR =
                                Math.Max(
                                    Tp1MinimumRR,
                                    MinimumRequiredRR());
                
                            double maximumRR =
                                Math.Max(
                                    minimumRR,
                                    MaximumRewardRR);
                
                            double tp1RR =
                                Math.Abs(
                                    plan.Tp1 -
                                    plan.Entry) /
                                plan.Risk;
                
                            if (tp1RR < minimumRR ||
                                tp1RR > maximumRR)
                                return false;
                
                            if (plan.Tp2 > 0)
                            {
                                double rr =
                                    Math.Abs(
                                        plan.Tp2 -
                                        plan.Entry) /
                                    plan.Risk;
                
                                if (rr <
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
                                        ? plan.Tp2RR
                                        : tp1RR;
                
                                if (rr <
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
                                        ? plan.Tp3RR
                                        : plan.Tp2 > 0
                                            ? plan.Tp2RR
                                            : tp1RR;
                
                                if (rr <
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
                
                            if (checkSpread &&
                                UseSpreadFilter)
                            {
                                double spread =
                                    Math.Max(
                                        0,
                                        Symbol.Ask -
                                        Symbol.Bid);
                
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
