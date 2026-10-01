using System;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void UpdateM1TriggerRuntime(
            int closedM5,
            DateTime reference)
        {
            int decisionDirection =
                _decision == null
                    ? 0
                    : _decision.Direction;

            if (TriggerLifecycleRule.ShouldReset(
                    _triggerRuntime.DecisionM5,
                    _triggerRuntime.Direction,
                    closedM5,
                    decisionDirection))
            {
                _triggerRuntime.Reset(
                    closedM5,
                    decisionDirection);
            }

            if (_decision == null ||
                _decision.Direction == 0 ||
                _m1Bars == null ||
                _m5Bars == null ||
                closedM5 < 20)
            {
                _triggerRuntime.Ready = false;
                _triggerRuntime.Reason = "NO DECISION";
                _triggerRuntime.UpdatedUtc = reference;
                return;
            }

            int liveM5 =
                _m5Bars.Count - 1;

            int closedM1 =
                ClosedIndex(
                    _m1Bars,
                    reference);

            int previousClosedM1 =
                _triggerRuntime.ClosedM1;

            _triggerRuntime.LiveM5Window = liveM5;
            _triggerRuntime.Direction =
                _decision.Direction;

            bool m5Ready =
                ClosedBarTriggerReady(
                    _m5Bars,
                    closedM5,
                    _decision.Direction);

            if (!UseM1Trigger)
            {
                _triggerRuntime.Score = 0;
                _triggerRuntime.RequiredScore = 0;
                _triggerRuntime.Ready = m5Ready;
                _triggerRuntime.Latched = m5Ready;
                _triggerRuntime.ConfirmedM1 = -1;
                _triggerRuntime.Reason =
                    m5Ready
                        ? "M5 TRIGGER READY"
                        : "M5 TRIGGER";
                _triggerRuntime.UpdatedUtc = reference;

                if (_decision != null)
                    _decision.TriggerReady = m5Ready;

                return;
            }

            if (!m5Ready)
            {
                _triggerRuntime.Ready = false;
                _triggerRuntime.Latched = false;
                _triggerRuntime.ConfirmedM1 = -1;
                _triggerRuntime.Reason = "M5 TRIGGER";
                _triggerRuntime.UpdatedUtc = reference;

                if (_decision != null)
                    _decision.TriggerReady = false;

                return;
            }

            if (closedM1 < 20 ||
                closedM1 >= _m1Bars.Count - 1)
            {
                _triggerRuntime.Ready =
                    _triggerRuntime.Latched;
                _triggerRuntime.Reason =
                    "WAITING FOR CLOSED M1";
                _triggerRuntime.UpdatedUtc = reference;

                if (_decision != null)
                    _decision.TriggerReady =
                        _triggerRuntime.Ready;

                return;
            }

            DateTime m1Open =
                _m1Bars.OpenTimes[closedM1];

            DateTime m1NextOpen =
                _m1Bars.OpenTimes[closedM1 + 1];

            int parentM5 =
                FindContainingBarIndex(
                    _m5Bars,
                    m1Open);

            if (parentM5 < 0)
            {
                _triggerRuntime.Ready =
                    TriggerLifecycleRule.IsConfirmed(
                        m5Ready,
                        UseM1Trigger,
                        _triggerRuntime.Latched);
                _triggerRuntime.Reason =
                    "M1 OUTSIDE TRIGGER WINDOW";
                _triggerRuntime.UpdatedUtc = reference;

                if (_decision != null)
                    _decision.TriggerReady =
                        _triggerRuntime.Ready;

                return;
            }

            DateTime m5Open =
                _m5Bars.OpenTimes[parentM5];

            DateTime m5NextOpen =
                parentM5 + 1 < _m5Bars.Count
                    ? _m5Bars.OpenTimes[parentM5 + 1]
                    : m5Open.AddMinutes(5);

            bool inside =
                TriggerLifecycleRule.CanEvaluateLiveM1Confirmation(
                    closedM5,
                    liveM5,
                    parentM5,
                    m1Open,
                    m1NextOpen,
                    m5Open,
                    m5NextOpen,
                    reference);

            if (!inside)
            {
                _triggerRuntime.Ready =
                    TriggerLifecycleRule.IsConfirmed(
                        m5Ready,
                        UseM1Trigger,
                        _triggerRuntime.Latched);
                _triggerRuntime.Reason =
                    "M1 WINDOW NOT CLOSED";
                _triggerRuntime.UpdatedUtc = reference;

                if (_decision != null)
                    _decision.TriggerReady =
                        _triggerRuntime.Latched;

                return;
            }

            if (previousClosedM1 != closedM1 ||
                _m1Frame == null ||
                _m1Frame.Index != closedM1)
            {
                _m1Frame =
                    AnalyzeFrame(
                        _m1Bars,
                        closedM1);
            }

            double atr =
                Atr(
                    _m1Bars,
                    closedM1);

            if (atr <= 0)
            {
                _triggerRuntime.Ready =
                    TriggerLifecycleRule.IsConfirmed(
                        m5Ready,
                        UseM1Trigger,
                        _triggerRuntime.Latched);
                _triggerRuntime.Reason =
                    "M1 ATR UNAVAILABLE";
                _triggerRuntime.UpdatedUtc = reference;

                if (_decision != null)
                    _decision.TriggerReady =
                        _triggerRuntime.Latched;

                return;
            }

            _triggerRuntime.ClosedM1 = closedM1;

            int score =
                _decision.Direction == 1
                    ? BullTriggerScore(
                        _m1Bars,
                        closedM1)
                    : BearTriggerScore(
                        _m1Bars,
                        closedM1);

            int required =
                TriggerThresholdRule.ResolveRequiredScore(
                    UsePrecisionExecutionModel,
                    LiveTriggerScore,
                    PrecisionTriggerScore);

            int start =
                Math.Max(
                    3,
                    closedM1 - 8);

            int end =
                closedM1 - 1;

            if (end < start)
            {
                _triggerRuntime.Ready =
                    TriggerLifecycleRule.IsConfirmed(
                        m5Ready,
                        UseM1Trigger,
                        _triggerRuntime.Latched);
                _triggerRuntime.Reason =
                    "M1 MICROSTRUCTURE WARMUP";
                _triggerRuntime.UpdatedUtc = reference;
                return;
            }

            double priorMicroHigh =
                Highest(
                    _m1Bars,
                    start,
                    end);

            double priorMicroLow =
                Lowest(
                    _m1Bars,
                    start,
                    end);

            bool ready =
                M1TriggerRule.IsReady(
                    _decision.Direction,
                    _m1Frame == null
                        ? 0
                        : _m1Frame.Direction,
                    _m1Bars.OpenPrices[closedM1],
                    _m1Bars.HighPrices[closedM1],
                    _m1Bars.LowPrices[closedM1],
                    _m1Bars.ClosePrices[closedM1],
                    atr,
                    MinimumTriggerBodyAtr,
                    MinimumCloseLocation,
                    MaximumTriggerRangeAtr,
                    score,
                    required,
                    priorMicroHigh,
                    priorMicroLow,
                    StructureBreakAtr,
                    UseDisplacement,
                    DisplacementAtr);

            _triggerRuntime.Score = score;
            _triggerRuntime.RequiredScore = required;

            if (TriggerLifecycleRule.ShouldRecordNewConfirmation(
                    _triggerRuntime.ConfirmedM1,
                    closedM1,
                    ready))
            {
                _triggerRuntime.Latched = true;
                _triggerRuntime.ConfirmedM1 =
                    closedM1;
                _triggerRuntime.ConfirmationUtc =
                    ResolveClosedBarBoundaryUtc(
                        _m1Bars,
                        closedM1,
                        reference);
                _triggerRuntime.ConfirmationRevision++;
            }

            _triggerRuntime.Ready =
                TriggerLifecycleRule.IsConfirmed(
                    m5Ready,
                    UseM1Trigger,
                    _triggerRuntime.Latched);

            _triggerRuntime.Reason =
                _triggerRuntime.Ready
                    ? "M1 TRIGGER CONFIRMED"
                    : "M1 TRIGGER WAIT";
            _triggerRuntime.UpdatedUtc = reference;

            _decision.TriggerReady =
                _triggerRuntime.Ready;
        }

        private DateTime ResolveClosedBarBoundaryUtc(
            Bars bars,
            int closedIndex,
            DateTime fallback)
        {
            if (bars != null &&
                closedIndex >= 0 &&
                closedIndex + 1 < bars.Count)
                return bars.OpenTimes[closedIndex + 1];

            return fallback;
        }

        private int FindContainingBarIndex(
            Bars bars,
            DateTime time)
        {
            if (bars == null ||
                bars.Count == 0 ||
                time == DateTime.MinValue)
                return -1;

            int low = 0;
            int high = bars.Count - 1;
            int candidate = -1;

            while (low <= high)
            {
                int middle =
                    low + ((high - low) / 2);

                DateTime open =
                    bars.OpenTimes[middle];

                if (open <= time)
                {
                    candidate = middle;
                    low = middle + 1;
                }
                else
                {
                    high = middle - 1;
                }
            }

            return candidate;
        }
    }
}