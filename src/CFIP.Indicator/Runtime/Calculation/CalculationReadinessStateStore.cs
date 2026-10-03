using System;

namespace cAlgo
{
    public partial class CFIPIndicator
    {
        private CalculationReadinessState _calculationReadinessState =
            CalculationReadinessState.BuildingHistory;

        private DateTime _lastCalculationReadinessProbeUtc =
            DateTime.MinValue;

        private DateTime _lastCalculationReadinessPanelUtc =
            DateTime.MinValue;

        private bool _calculationReadinessStateChanged;

        private CalculationReadinessState CurrentCalculationReadinessState
        {
            get { return _calculationReadinessState; }
        }

        private bool ShouldProbeCalculationReadiness(
            DateTime nowUtc)
        {
            if (_calculationReadinessState ==
                CalculationReadinessState.Ready)
                return true;

            return CalculationReadinessRule.IsProbeDue(
                _lastCalculationReadinessProbeUtc,
                nowUtc,
                CalculationReadinessRule.ProbeIntervalMilliseconds(
                    _calculationReadinessState));
        }

        private void RecordCalculationReadiness(
            CalculationReadinessState state,
            DateTime nowUtc)
        {
            _calculationReadinessStateChanged =
                _calculationReadinessState != state;

            _calculationReadinessState =
                state;

            _lastCalculationReadinessProbeUtc =
                nowUtc;
        }

        private void RenderCalculationReadinessIfNeeded(
            DateTime nowUtc)
        {
            int throttle =
                CurrentCalculationReadinessState ==
                CalculationReadinessState.WaitingForMtfData
                    ? 500
                    : 250;

            bool due =
                _calculationReadinessStateChanged ||
                _lastCalculationReadinessPanelUtc ==
                    DateTime.MinValue ||
                (nowUtc -
                 _lastCalculationReadinessPanelUtc)
                .TotalMilliseconds >=
                throttle;

            if (!due)
                return;

            _status =
                CalculationReadinessRule.StatusText(
                    CurrentCalculationReadinessState);

            _lastCalculationReadinessPanelUtc =
                nowUtc;

            _calculationReadinessStateChanged =
                false;

            try
            {
                RequestPanelContentRefresh();
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP calculation-readiness panel update failed: {0}",
                    ex.Message);
            }
        }

        private int GetManagementClosedM5Fallback()
        {
            if (_lastEvaluatedM5 > 0)
                return _lastEvaluatedM5;

            if (_lastMtfClosedContext != null &&
                _lastMtfClosedContext.M5 > 0)
                return _lastMtfClosedContext.M5;

            return 1;
        }
    }
}
