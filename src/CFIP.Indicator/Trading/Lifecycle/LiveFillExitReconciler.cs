using System;
using System.Collections.Generic;

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

            double stopCandidate =
                BuildStructuralStop(
                    closedM5,
                    direction,
                    actualEntry,
                    atr,
                    out string stopSource,
                    out int stopQuality);

            double selectedStop = oldStop;

            if (!IsValidStop(
                    direction,
                    actualEntry,
                    selectedStop))
            {
                selectedStop = stopCandidate;
                stopSource = string.IsNullOrWhiteSpace(stopSource)
                    ? "LIVE / STRUCTURAL"
                    : stopSource;
                stopQuality = Math.Max(60, stopQuality);

                if (position.StopLoss.HasValue &&
                    IsValidStop(
                        direction,
                        actualEntry,
                        position.StopLoss.Value) &&
                    (!IsFinitePositive(selectedStop) ||
                     !ProtectionProgressionRule.ShouldAdvanceStop(
                         direction,
                         selectedStop,
                         position.StopLoss.Value)))
                {
                    selectedStop =
                        NormalizePrice(
                            position.StopLoss.Value);
                    stopSource = "BROKER FILL PROTECTION";
                    stopQuality = 100;
                }
            }
            else if (IsFinitePositive(stopCandidate) &&
                     IsValidStop(
                         direction,
                         actualEntry,
                         stopCandidate) &&
                     ProtectionProgressionRule.ShouldAdvanceStop(
                         direction,
                         selectedStop,
                         stopCandidate))
            {
                selectedStop =
                    NormalizePrice(stopCandidate);
            }

            if (!IsValidStop(
                    direction,
                    actualEntry,
                    selectedStop))
                return false;

            _plan.Entry =
                actualEntry;

            _plan.Stop =
                NormalizePrice(selectedStop);

            _plan.StopSource =
                stopSource ??
                _plan.StopSource ??
                "LIVE / STRUCTURAL";

            _plan.StopQuality =
                Math.Max(
                    0,
                    Math.Min(
                        100,
                        stopQuality));

            _plan.Risk =
                Math.Max(
                    Symbol.PipSize,
                    Math.Abs(
                        _plan.Entry -
                        _plan.Stop));

            List<Level> levels =
                BuildTargetLevels(
                    closedM5,
                    direction,
                    _plan.Entry,
                    atr);

            List<Level> selected =
                SelectTargets(
                    levels,
                    closedM5,
                    _plan.Entry,
                    _plan.Risk,
                    direction,
                    atr,
                    _plan.Lane);

            _plan.Tp1 =
                SelectLiveFillTarget(
                    selected,
                    0,
                    oldTp1,
                    _plan.Entry,
                    market,
                    direction,
                    _plan.Risk,
                    atr,
                    Math.Max(
                        FallbackTp1RR,
                        MinimumRequiredRR));

            if (!IsFinitePositive(_plan.Tp1))
                return false;

            _plan.Tp2 =
                SelectLiveFillTarget(
                    selected,
                    1,
                    oldTp2,
                    _plan.Entry,
                    market,
                    direction,
                    _plan.Risk,
                    atr,
                    Math.Max(
                        FallbackTp2RR,
                        Tp2MinimumRR),
                    _plan.Tp1);

            _plan.Tp3 =
                SelectLiveFillTarget(
                    selected,
                    2,
                    oldTp3,
                    _plan.Entry,
                    market,
                    direction,
                    _plan.Risk,
                    atr,
                    Math.Max(
                        FallbackTp3RR,
                        Tp3MinimumRR),
                    _plan.Tp2 > 0
                        ? _plan.Tp2
                        : _plan.Tp1);

            _plan.Tp4 =
                SelectLiveFillTarget(
                    selected,
                    3,
                    oldTp4,
                    _plan.Entry,
                    market,
                    direction,
                    _plan.Risk,
                    atr,
                    Math.Max(
                        FallbackTp4RR,
                        Tp4MinimumRR),
                    _plan.Tp3 > 0
                        ? _plan.Tp3
                        : _plan.Tp2 > 0
                            ? _plan.Tp2
                            : _plan.Tp1);

            if (!LiveExitGeometryRule.IsProgressiveTargetLadder(
                    direction,
                    _plan.Entry,
                    _plan.Tp1,
                    _plan.Tp2,
                    _plan.Tp3,
                    _plan.Tp4))
                return false;

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
