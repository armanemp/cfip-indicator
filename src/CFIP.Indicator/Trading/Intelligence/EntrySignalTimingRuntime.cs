using System;

namespace cAlgo
{
    public partial class CFIPIndicator
    {
        private void UpdateEntrySignalTiming(
            int closedM5)
        {
            if (_decision == null ||
                _decision.Direction == 0)
                return;

            int direction =
                _decision.Direction;

            if (_entryTimingM5 != closedM5 ||
                _entryTimingDirection != direction)
            {
                _entryTimingM5 = closedM5;
                _entryTimingDirection = direction;
                _entrySignalTiming =
                    EntrySignalTiming.NotMeasured();
            }

            if (!_decision.ActionableNow ||
                _entrySignalTiming.Measured)
                return;

            DateTime causalEventUtc =
                ResolveEntrySignalCausalEventUtc(
                    closedM5,
                    direction);

            EntrySignalTiming timing =
                EntrySignalTimingRule.Measure(
                    causalEventUtc,
                    Server.TimeInUtc);

            if (!timing.Measured)
                return;

            _entrySignalTiming = timing;

            ArchiveRuntimeEntrySignalTiming(
                closedM5,
                direction,
                timing,
                BuildSignalTraceId(closedM5));
        }

        private DateTime ResolveEntrySignalCausalEventUtc(
            int closedM5,
            int direction)
        {
            if (UseM1Trigger &&
                _triggerRuntime.DecisionM5 == closedM5 &&
                _triggerRuntime.Direction == direction &&
                _triggerRuntime.ConfirmationUtc != DateTime.MinValue)
                return _triggerRuntime.ConfirmationUtc;

            if (_m5Bars != null &&
                closedM5 >= 0 &&
                closedM5 + 1 < _m5Bars.Count)
                return _m5Bars.OpenTimes[closedM5 + 1];

            if (_m5Bars != null &&
                closedM5 >= 0 &&
                closedM5 < _m5Bars.Count)
                return _m5Bars.OpenTimes[closedM5];

            return DateTime.MinValue;
        }
    }
}
