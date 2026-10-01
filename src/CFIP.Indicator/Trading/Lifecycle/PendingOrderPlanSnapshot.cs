using System;
using System.Collections.Generic;

namespace cAlgo
{
    public partial class CFIPIndicator
    {
        private Plan _pendingOrderPlanSnapshot;

        private Plan CapturePendingOrderPlanSnapshot()
        {
            if (_plan == null)
                return null;

            return new Plan
            {
                Direction = _plan.Direction,
                Lane = _plan.Lane,
                EntryMode = _plan.EntryMode,
                Entry = _plan.Entry,
                IdealEntry = _plan.IdealEntry,
                EntryZoneLow = _plan.EntryZoneLow,
                EntryZoneHigh = _plan.EntryZoneHigh,
                EntryZoneTolerance = _plan.EntryZoneTolerance,
                EntryTrigger = _plan.EntryTrigger,
                EntryInvalidation = _plan.EntryInvalidation,
                EntryQuality = _plan.EntryQuality,
                EntrySource = _plan.EntrySource,
                Stop = _plan.Stop,
                Tp1 = _plan.Tp1,
                Tp2 = _plan.Tp2,
                Tp3 = _plan.Tp3,
                Tp4 = _plan.Tp4,
                Risk = _plan.Risk,
                Tp1RR = _plan.Tp1RR,
                Tp2RR = _plan.Tp2RR,
                Tp3RR = _plan.Tp3RR,
                Tp4RR = _plan.Tp4RR,
                StopQuality = _plan.StopQuality,
                Tp1Quality = _plan.Tp1Quality,
                Tp2Quality = _plan.Tp2Quality,
                Tp3Quality = _plan.Tp3Quality,
                Tp4Quality = _plan.Tp4Quality,
                StopSource = _plan.StopSource,
                Tp1Source = _plan.Tp1Source,
                Tp2Source = _plan.Tp2Source,
                Tp3Source = _plan.Tp3Source,
                Tp4Source = _plan.Tp4Source,
                HtfTargetCount = _plan.HtfTargetCount,
                CreatedM5 = _plan.CreatedM5,
                SignalBarOpenTimeUtcTicks = _plan.SignalBarOpenTimeUtcTicks,
                SignalTraceId = _plan.SignalTraceId,
                OriginalVolume = _plan.OriginalVolume,
                CalibrationEligible = _plan.CalibrationEligible,
                CalibrationDirection = _plan.CalibrationDirection,
                CalibrationLane = _plan.CalibrationLane,
                CalibrationRegime = _plan.CalibrationRegime,
                CalibrationConfidence = _plan.CalibrationConfidence,
                CalibrationBucket = _plan.CalibrationBucket,
                IsLivePosition = false,
                PositionId = 0
            };
        }

        private Plan CapturePendingOrderPlanSnapshot(
            ExecutionIntent intent,
            int closedM5,
            double atr)
        {
            if (intent == null ||
                (intent.Direction != 1 &&
                 intent.Direction != -1) ||
                !IsFinitePositive(intent.RequestedEntry) ||
                !IsFinitePositive(intent.Stop) ||
                !IsFinitePositive(intent.Target) ||
                closedM5 < 1 ||
                atr <= 0)
                return null;

            double entry = intent.RequestedEntry;
            double risk = Math.Abs(entry - intent.Stop);

            if (!IsFinitePositive(risk))
                return null;

            OpportunityLane lane =
                intent.Kind == ExecutionIntentKind.Limit
                    ? OpportunityLane.MicroReaction
                    : ResolveContinuationPendingLane(intent.Direction);

            List<Level> levels =
                BuildTargetLevels(
                    closedM5,
                    intent.Direction,
                    entry,
                    atr);

            List<Level> selected =
                SelectTargets(
                    levels,
                    closedM5,
                    entry,
                    Math.Max(Symbol.PipSize, risk),
                    intent.Direction,
                    atr,
                    lane);

            double[] requiredRR =
                BuildTargetSelectionRequiredRR(
                    Math.Max(
                        0.10,
                        StructuralTpRrStep),
                    lane);

            double tp1 =
                SelectTarget(
                    selected,
                    0,
                    entry,
                    risk,
                    intent.Direction,
                    Math.Max(
                        FallbackTp1RR,
                        requiredRR[0]),
                    lane);

            double tp2 =
                SelectTarget(
                    selected,
                    1,
                    entry,
                    risk,
                    intent.Direction,
                    Math.Max(
                        FallbackTp2RR,
                        requiredRR[1]),
                    lane);

            double tp3 =
                SelectTarget(
                    selected,
                    2,
                    entry,
                    risk,
                    intent.Direction,
                    Math.Max(
                        FallbackTp3RR,
                        requiredRR[2]),
                    lane);

            double tp4 =
                SelectTarget(
                    selected,
                    3,
                    entry,
                    risk,
                    intent.Direction,
                    Math.Max(
                        FallbackTp4RR,
                        requiredRR[3]),
                    lane);

            int configuredStage =
                ClampInt(
                    (int)EffectiveAutoTpStage(),
                    0,
                    3);

            switch (configuredStage)
            {
                case 0:
                    tp1 = intent.Target;
                    break;
                case 1:
                    tp2 = intent.Target;
                    break;
                case 2:
                    tp3 = intent.Target;
                    break;
                default:
                    tp4 = intent.Target;
                    break;
            }

            if (!IsFinitePositive(tp1))
                tp1 = intent.Target;

            if (!LiveExitGeometryRule.IsProgressiveTargetLadder(
                    intent.Direction,
                    entry,
                    tp1,
                    tp2,
                    tp3,
                    tp4))
            {
                if (configuredStage == 0)
                {
                    tp2 = tp2 > 0
                        ? tp2
                        : 0;
                    tp3 = tp3 > 0
                        ? tp3
                        : 0;
                    tp4 = tp4 > 0
                        ? tp4
                        : 0;
                }
            }

            return new Plan
            {
                Direction = intent.Direction,
                Lane = lane,
                EntryMode =
                    intent.Kind == ExecutionIntentKind.Stop
                        ? ExecutionMode.ContinuationStop
                        : ExecutionMode.ReversalLimit,
                Entry = NormalizePrice(entry),
                IdealEntry = NormalizePrice(entry),
                EntryTrigger = NormalizePrice(intent.Trigger),
                EntryZoneLow = NormalizePrice(intent.ZoneLow),
                EntryZoneHigh = NormalizePrice(intent.ZoneHigh),
                EntryZoneTolerance = 0,
                Stop = NormalizePrice(intent.Stop),
                Tp1 = IsFinitePositive(tp1)
                    ? NormalizePrice(tp1)
                    : 0,
                Tp2 = IsFinitePositive(tp2)
                    ? NormalizePrice(tp2)
                    : 0,
                Tp3 = IsFinitePositive(tp3)
                    ? NormalizePrice(tp3)
                    : 0,
                Tp4 = IsFinitePositive(tp4)
                    ? NormalizePrice(tp4)
                    : 0,
                Risk = Math.Max(
                    Symbol.PipSize,
                    risk),
                Tp1RR =
                    IsFinitePositive(tp1)
                        ? Math.Abs(tp1 - entry) / risk
                        : 0,
                Tp2RR =
                    IsFinitePositive(tp2)
                        ? Math.Abs(tp2 - entry) / risk
                        : 0,
                Tp3RR =
                    IsFinitePositive(tp3)
                        ? Math.Abs(tp3 - entry) / risk
                        : 0,
                Tp4RR =
                    IsFinitePositive(tp4)
                        ? Math.Abs(tp4 - entry) / risk
                        : 0,
                StopQuality = 100,
                Tp1Quality = 100,
                Tp2Quality = 100,
                Tp3Quality = 100,
                Tp4Quality = 100,
                StopSource = "PENDING ABSOLUTE SNAPSHOT",
                Tp1Source = "PENDING ABSOLUTE SNAPSHOT",
                Tp2Source = "PENDING ABSOLUTE SNAPSHOT",
                Tp3Source = "PENDING ABSOLUTE SNAPSHOT",
                Tp4Source = "PENDING ABSOLUTE SNAPSHOT",
                CreatedM5 = intent.CreatedM5 > 0
                    ? intent.CreatedM5
                    : closedM5,
                OriginalVolume = intent.Volume,
                CalibrationEligible =
                    _decision != null &&
                    _decision.EntryAllowed,
                CalibrationDirection =
                    intent.Direction,
                CalibrationLane = lane,
                CalibrationRegime =
                    _decision == null
                        ? "UNKNOWN"
                        : _decision.Regime,
                CalibrationConfidence =
                    _decision == null
                        ? 0
                        : _decision.Confidence,
                CalibrationBucket =
                    _decision == null
                        ? 0
                        : Math.Max(
                            0,
                            _decision.Confidence / 5),
                IsLivePosition = false,
                PositionId = 0
            };
        }

        private OpportunityLane ResolveContinuationPendingLane(
            int direction)
        {
            if (_decision != null &&
                _decision.Direction == direction &&
                _decision.TopDownEligible &&
                string.Equals(
                    _decision.TopDownStage,
                    "ENTRY CALIBRATED",
                    StringComparison.OrdinalIgnoreCase))
                return OpportunityLane.Strategic;

            if (_decision != null &&
                _decision.Direction == direction &&
                (_decision.TacticalOpportunityLane ==
                 OpportunityLane.Tactical ||
                 _decision.TacticalOpportunityLane ==
                 OpportunityLane.CounterHtfTactical))
                return _decision.TacticalOpportunityLane;

            return OpportunityLane.Tactical;
        }

        private void ClearPendingOrderPlanSnapshot()
        {
            _pendingOrderPlanSnapshot = null;
        }
    }
}
