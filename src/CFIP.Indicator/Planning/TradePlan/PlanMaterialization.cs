using System;
using System.Collections.Generic;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private Plan CreatePlanFromInputs(
            ExecutionModel execution,
            int direction,
            int closedM5,
            double entry,
            double stop,
            string stopSource,
            int stopQuality,
            double tp1,
            double tp2,
            double tp3,
            double tp4)
        {
            double normalizedEntry =
                NormalizePrice(entry);

            double normalizedTp1 =
                NormalizePrice(tp1);

            if (!PriceProtectionRule.ValidateTarget(
                    direction,
                    normalizedEntry,
                    normalizedTp1,
                    0))
                return null;

            Plan p =
                new Plan
                {
                    Direction = direction,
                    EntryMode =
                        execution == null
                            ? ExecutionMode.None
                            : execution.Mode,
                    Entry = normalizedEntry,
                    IdealEntry =
                        execution == null
                            ? entry
                            : NormalizePrice(
                                execution.IdealEntry),
                    EntryZoneLow =
                        execution == null
                            ? 0
                            : NormalizePrice(
                                execution.ZoneLow),
                    EntryZoneHigh =
                        execution == null
                            ? 0
                            : NormalizePrice(
                                execution.ZoneHigh),
                    EntryZoneTolerance =
                        execution == null
                            ? 0
                            : Math.Max(
                                0,
                                execution.ZoneTolerance),
                    EntryTrigger =
                        execution == null
                            ? 0
                            : NormalizePrice(
                                execution.Trigger),
                    EntryInvalidation =
                        execution == null
                            ? 0
                            : NormalizePrice(
                                execution.Invalidation),
                    EntryQuality =
                        execution == null
                            ? 0
                            : execution.Quality,
                    EntrySource =
                        execution == null
                            ? ""
                            : execution.Source,
                    Stop = NormalizePrice(stop),
                    Tp1 = normalizedTp1,
                    Tp2 =
                        IsValidTarget(
                            direction,
                            entry,
                            tp2)
                            ? NormalizePrice(tp2)
                            : 0,
                    Tp3 =
                        IsValidTarget(
                            direction,
                            entry,
                            tp3)
                            ? NormalizePrice(tp3)
                            : 0,
                    Tp4 =
                        IsValidTarget(
                            direction,
                            entry,
                            tp4)
                            ? NormalizePrice(tp4)
                            : 0,
                    StopSource = stopSource,
                    StopQuality = stopQuality,
                    CreatedM5 = closedM5
                };

            p.Risk =
                Math.Abs(
                    p.Entry -
                    p.Stop);

            if (!IsFinitePositive(p.Risk))
            {
                p.Risk = 0;
                p.Tp1RR = 0;
                p.Tp2RR = 0;
                p.Tp3RR = 0;
                p.Tp4RR = 0;
                return p;
            }

            p.Tp1RR =
                RiskRewardGeometryRule.CalculateNominalRR(
                    p.Entry,
                    p.Tp1,
                    p.Risk);

            p.Tp2RR =
                RiskRewardGeometryRule.CalculateNominalRR(
                    p.Entry,
                    p.Tp2,
                    p.Risk);

            p.Tp3RR =
                RiskRewardGeometryRule.CalculateNominalRR(
                    p.Entry,
                    p.Tp3,
                    p.Risk);

            p.Tp4RR =
                RiskRewardGeometryRule.CalculateNominalRR(
                    p.Entry,
                    p.Tp4,
                    p.Risk);

            return p;
        }

        private void EnrichPlanTargetMetadata(
            Plan p,
            List<Level> selected)
        {
            ApplySelectedTargetMeta(
                selected,
                0,
                p.Tp1,
                out p.Tp1Source,
                out p.Tp1Quality);

            ApplySelectedTargetMeta(
                selected,
                1,
                p.Tp2,
                out p.Tp2Source,
                out p.Tp2Quality);

            ApplySelectedTargetMeta(
                selected,
                2,
                p.Tp3,
                out p.Tp3Source,
                out p.Tp3Quality);

            ApplySelectedTargetMeta(
                selected,
                3,
                p.Tp4,
                out p.Tp4Source,
                out p.Tp4Quality);

            p.HtfTargetCount =
                CountHtfTargetsInPlan(
                    p);
        }
    }
}
