using System;
using System.Collections.Generic;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator
    {
        private bool ReconcileLiveFillExitGeometry(
            int closedM5,
            int direction,
            double actualEntry,
            double atr,
            Position position,
            Plan absoluteReferencePlan = null)
        {
            if (_plan == null ||
                position == null ||
                (direction != 1 && direction != -1) ||
                !IsFinitePositive(actualEntry) ||
                atr <= 0)
                return false;

            double market =
                direction == 1
                    ? Symbol.Bid
                    : Symbol.Ask;

            if (!IsFinitePositive(market))
                return false;

            Plan referencePlan =
                absoluteReferencePlan ?? _plan;

            double oldStop =
                referencePlan != null &&
                IsFinitePositive(referencePlan.Stop)
                    ? referencePlan.Stop
                    : _plan.Stop;

            double oldTp1 =
                referencePlan != null &&
                IsFinitePositive(referencePlan.Tp1)
                    ? referencePlan.Tp1
                    : _plan.Tp1;

            double oldTp2 =
                referencePlan != null &&
                IsFinitePositive(referencePlan.Tp2)
                    ? referencePlan.Tp2
                    : _plan.Tp2;

            double oldTp3 =
                referencePlan != null &&
                IsFinitePositive(referencePlan.Tp3)
                    ? referencePlan.Tp3
                    : _plan.Tp3;

            double oldTp4 =
                referencePlan != null &&
                IsFinitePositive(referencePlan.Tp4)
                    ? referencePlan.Tp4
                    : _plan.Tp4;

            string stopSource =
                _plan.StopSource ?? "";
            int stopQuality =
                _plan.StopQuality;

            double stopCandidate =
                BuildStructuralStop(
                    closedM5,
                    direction,
                    actualEntry,
                    atr,
                    out string structuralStopSource,
                    out int structuralStopQuality);

            double minimumStopDistance =
                Math.Max(
                    Symbol.TickSize,
                    MinimumProtectionDistancePriceForDirection(
                        direction));

            double brokerStop =
                position.StopLoss.HasValue
                    ? position.StopLoss.Value
                    : 0;

            PendingFillExitResolution stopResolution =
                PendingFillExitResolutionRule.ResolveProtectiveStop(
                    direction,
                    actualEntry,
                    market,
                    oldStop,
                    brokerStop,
                    minimumStopDistance);

            double candidateStop =
                stopResolution.Allowed
                    ? NormalizePrice(stopResolution.Price)
                    : oldStop;

            bool oldStopManaged =
                IsValidManagedStop(
                    direction,
                    actualEntry,
                    market,
                    candidateStop);

            bool structuralStopManaged =
                IsValidManagedStop(
                    direction,
                    actualEntry,
                    market,
                    stopCandidate);

            if (!oldStopManaged)
            {
                if (structuralStopManaged)
                {
                    candidateStop = NormalizePrice(stopCandidate);
                    stopSource = structuralStopSource ?? "LIVE / STRUCTURAL";
                    stopQuality = Math.Max(60, structuralStopQuality);
                }
                else if (position.StopLoss.HasValue &&
                         IsValidManagedStop(
                             direction,
                             actualEntry,
                             market,
                             position.StopLoss.Value))
                {
                    candidateStop =
                        NormalizePrice(position.StopLoss.Value);
                    stopSource = "BROKER FILL PROTECTION";
                    stopQuality = 100;
                }
            }
            else if (structuralStopManaged &&
                     ProtectionProgressionRule.ShouldAdvanceStop(
                         direction,
                         candidateStop,
                         stopCandidate))
            {
                candidateStop =
                    NormalizePrice(stopCandidate);
                stopSource = structuralStopSource ?? stopSource;
                stopQuality = Math.Max(
                    stopQuality,
                    structuralStopQuality);
            }

            if (!IsValidManagedStop(
                    direction,
                    actualEntry,
                    market,
                    candidateStop))
                return false;

            double candidateRisk =
                Math.Max(
                    Symbol.PipSize,
                    Math.Abs(
                        actualEntry -
                        candidateStop));

            List<Level> levels =
                BuildTargetLevels(
                    closedM5,
                    direction,
                    actualEntry,
                    atr);

            List<Level> selected =
                SelectTargets(
                    levels,
                    closedM5,
                    actualEntry,
                    candidateRisk,
                    direction,
                    atr,
                    _plan.Lane);

            double targetSpacing =
                MinimumLiveTargetDistancePrice(
                    direction,
                    atr);

            double brokerTarget =
                position.TakeProfit.HasValue
                    ? position.TakeProfit.Value
                    : 0;

            PendingFillExitResolution targetResolution =
                PendingFillExitResolutionRule.ResolveProgressiveTarget(
                    direction,
                    actualEntry,
                    market,
                    oldTp1,
                    brokerTarget,
                    targetSpacing);

            double reconciledTp1 =
                targetResolution.Allowed
                    ? targetResolution.Price
                    : oldTp1;

            double candidateTp1 =
                SelectLiveFillTarget(
                    selected,
                    0,
                    reconciledTp1,
                    actualEntry,
                    market,
                    direction,
                    candidateRisk,
                    atr,
                    Math.Max(
                        FallbackTp1RR,
                        MinimumRequiredRR()));

            if (!IsFinitePositive(candidateTp1))
                return false;

            double candidateTp2 =
                SelectLiveFillTarget(
                    selected,
                    1,
                    oldTp2,
                    actualEntry,
                    market,
                    direction,
                    candidateRisk,
                    atr,
                    Math.Max(
                        FallbackTp2RR,
                        Tp2MinimumRR),
                    candidateTp1);

            double candidateTp3 =
                SelectLiveFillTarget(
                    selected,
                    2,
                    oldTp3,
                    actualEntry,
                    market,
                    direction,
                    candidateRisk,
                    atr,
                    Math.Max(
                        FallbackTp3RR,
                        Tp3MinimumRR),
                    candidateTp2 > 0
                        ? candidateTp2
                        : candidateTp1);

            double candidateTp4 =
                SelectLiveFillTarget(
                    selected,
                    3,
                    oldTp4,
                    actualEntry,
                    market,
                    direction,
                    candidateRisk,
                    atr,
                    Math.Max(
                        FallbackTp4RR,
                        Tp4MinimumRR),
                    candidateTp3 > 0
                        ? candidateTp3
                        : candidateTp2 > 0
                            ? candidateTp2
                            : candidateTp1);

            if (!LiveExitGeometryRule.IsProgressiveTargetLadder(
                    direction,
                    actualEntry,
                    candidateTp1,
                    candidateTp2,
                    candidateTp3,
                    candidateTp4) ||
                !IsLiveTargetBrokerSafe(
                    direction,
                    actualEntry,
                    market,
                    candidateTp1,
                    atr) ||
                (candidateTp2 > 0 &&
                 !IsLiveTargetBrokerSafe(
                     direction,
                     actualEntry,
                     market,
                     candidateTp2,
                     atr)) ||
                (candidateTp3 > 0 &&
                 !IsLiveTargetBrokerSafe(
                     direction,
                     actualEntry,
                     market,
                     candidateTp3,
                     atr)) ||
                (candidateTp4 > 0 &&
                 !IsLiveTargetBrokerSafe(
                     direction,
                     actualEntry,
                     market,
                     candidateTp4,
                     atr)))
                return false;

            _plan.Entry = actualEntry;
            _plan.Stop = NormalizePrice(candidateStop);
            _plan.StopSource = string.IsNullOrWhiteSpace(stopSource)
                ? "LIVE / STRUCTURAL"
                : stopSource;
            _plan.StopQuality = Math.Max(
                0,
                Math.Min(
                    100,
                    stopQuality));
            _plan.Risk = candidateRisk;

            _plan.Tp1 = NormalizePrice(candidateTp1);
            _plan.Tp2 = candidateTp2 > 0
                ? NormalizePrice(candidateTp2)
                : 0;
            _plan.Tp3 = candidateTp3 > 0
                ? NormalizePrice(candidateTp3)
                : 0;
            _plan.Tp4 = candidateTp4 > 0
                ? NormalizePrice(candidateTp4)
                : 0;

            ApplyResolvedTargetMeta(
                selected,
                0,
                _plan.Tp1,
                oldTp1,
                referencePlan?.Tp1Source,
                referencePlan?.Tp1Quality ?? 0,
                out _plan.Tp1Source,
                out _plan.Tp1Quality);

            ApplyResolvedTargetMeta(
                selected,
                1,
                _plan.Tp2,
                oldTp2,
                referencePlan?.Tp2Source,
                referencePlan?.Tp2Quality ?? 0,
                out _plan.Tp2Source,
                out _plan.Tp2Quality);

            ApplyResolvedTargetMeta(
                selected,
                2,
                _plan.Tp3,
                oldTp3,
                referencePlan?.Tp3Source,
                referencePlan?.Tp3Quality ?? 0,
                out _plan.Tp3Source,
                out _plan.Tp3Quality);

            ApplyResolvedTargetMeta(
                selected,
                3,
                _plan.Tp4,
                oldTp4,
                referencePlan?.Tp4Source,
                referencePlan?.Tp4Quality ?? 0,
                out _plan.Tp4Source,
                out _plan.Tp4Quality);

            _plan.HtfTargetCount =
                CountHtfTargetsInPlan(_plan);

            RecalculatePlanRR();

            return true;
        }

        private double SelectLiveFillTarget(
            List<Level> selected,
            int stage,
            double existing,
            double entry,
            double market,
            int direction,
            double risk,
            double atr,
            double fallbackRR,
            double previousTarget = 0)
        {
            double spacing =
                MinimumLiveTargetDistancePrice(
                    direction,
                    atr);

            double previous =
                previousTarget > 0
                    ? previousTarget
                    : entry;

            if (IsFinitePositive(existing) &&
                IsLiveTargetBrokerSafe(
                    direction,
                    entry,
                    market,
                    existing,
                    atr) &&
                LiveExitGeometryRule.ShouldAdvanceLiveTarget(
                    direction,
                    0,
                    existing,
                    market,
                    spacing) &&
                TargetProgressionRule.IsValid(
                    direction,
                    previous,
                    existing))
            {
                return NormalizePrice(existing);
            }

            if (selected != null &&
                stage >= 0 &&
                stage < selected.Count)
            {
                Level candidate =
                    selected[stage];

                if (candidate != null &&
                    IsFinitePositive(candidate.Price) &&
                    IsLiveTargetBrokerSafe(
                        direction,
                        entry,
                        market,
                        candidate.Price,
                        atr) &&
                    LiveExitGeometryRule.ShouldAdvanceLiveTarget(
                        direction,
                        existing,
                        candidate.Price,
                        market,
                        spacing) &&
                    TargetProgressionRule.IsValid(
                        direction,
                        previous,
                        candidate.Price))
                {
                    return NormalizePrice(candidate.Price);
                }
            }

            if (AllowSyntheticTargetFallback &&
                !RequiresHtfRewardForTargetStage(
                    stage,
                    _plan.Lane))
            {
                double synthetic =
                    direction == 1
                        ? entry + risk * fallbackRR
                        : entry - risk * fallbackRR;

                if (IsLiveTargetBrokerSafe(
                        direction,
                        entry,
                        market,
                        synthetic,
                        atr) &&
                    LiveExitGeometryRule.ShouldAdvanceLiveTarget(
                        direction,
                        existing,
                        synthetic,
                        market,
                        spacing) &&
                    TargetProgressionRule.IsValid(
                        direction,
                        previous,
                        synthetic))
                    return NormalizePrice(synthetic);
            }

            return 0;
        }
    }
}
