using System;

namespace cAlgo
{
    public partial class CFIPIndicator : cAlgo.API.Indicator
    {
        private void MonitorOutcome(
            int closedM5)
        {
            if (!EnableOutcomeTelemetry ||
                _plan == null ||
                !_plan.IsLivePosition ||
                OutcomeMaximumM5Bars <= 0)
                return;

            if (_outcomeTelemetryTimedOut ||
                closedM5 -
                _plan.CreatedM5 <
                OutcomeMaximumM5Bars)
                return;

            // Telemetry timeout is an observation boundary, not a position
            // lifecycle boundary. Keep broker ownership and live protection
            // active until the position is actually closed.
            _outcomeTelemetryTimedOut = true;

            SendUnifiedAlert(
                "OUTCOME-TIMEOUT|" +
                _plan.PositionId,
                "CFIP OUTCOME WINDOW ELAPSED | POSITION #" +
                _plan.PositionId +
                " remains under live management",
                _plan.Direction,
                false);
        }
    }
}
