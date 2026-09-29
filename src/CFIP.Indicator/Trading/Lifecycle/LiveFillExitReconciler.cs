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
            Position position)
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

            double oldStop = _plan.Stop;
            double oldTp1 = _plan.Tp1;
            double oldTp2 = _plan.Tp2;
            double oldTp3 = _plan.Tp3;
            double oldTp4 = _plan.Tp4;

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

            double candidateStop = oldStop;

            if (!IsValidStop(
                    direction,
                    actualEntry,
                    candidateStop))
            {
                if (IsValidStop(
                        direction,
                        actualEntry,
                        stopCandidate))
                {
                    candidateStop = NormalizePrice(stopCandidate);
                    stopSource = structuralStopSource ?? "LIVE / STRUCTURAL";
                    stopQuality = Math.Max(60, structuralStopQuality);
                }
                else if (position.StopLoss.HasValue &&
                         IsValidStop(
                             direction,
                             actualEntry,
                             position.StopLoss.Value))
                {
                    candidateStop =
                        NormalizePrice(position.StopLoss.Value);
                    stopSource = "BROKER FILL PROTECTION";
                    stopQuality = 100;
                }
            }
            else if (IsValidStop(
                         direction,
                         actualEntry,
                         stopCandidate) &&
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

            if (!IsValidStop(
                    direction,
                    actualEntry,
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

            double candidateTp1 =
                SelectLiveFillTarget(
                    selected,
                    0,
                    oldTp1,
                    actualEntry,
                    market,
                    direction,
                    candidateRisk,
                    atr,
                    Math.Max(
                        FallbackTp1RR,
                        MinimumRequiredRR));

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
                    candidateTp4))
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

            ApplyTargetMeta(
                levels,
                _plan.Tp1,
                atr,
                out _plan.Tp1Source,
                out _plan.Tp1Quality);

            ApplyTargetMeta(
                levels,
                _plan.Tp2,
                atr,
                out _plan.Tp2Source,
                out _plan.Tp2Quality);

            ApplyTargetMeta(
                levels,
                _plan.Tp3,
                atr,
                out _plan.Tp3Source,
                out _plan.Tp3Quality);

            ApplyTargetMeta(
                levels,
                _plan.Tp4,
                atr,
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
                Math.Max(
                    Symbol.PipSize,
                    atr *
                    Math.Max(
                        0.05,
                        MinimumTpSpacingAtr));

            double previous =
                previousTarget > 0
                    ? previousTarget
                    : entry;

            if (IsFinitePositive(existing) &&
                LiveExitGeometryRule.ShouldAdvanceTarget(
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
                    LiveExitGeometryRule.ShouldAdvanceTarget(
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

                if (LiveExitGeometryRule.ShouldAdvanceTarget(
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
