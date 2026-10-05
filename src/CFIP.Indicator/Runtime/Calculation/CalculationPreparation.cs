using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool TryPrepareCalculationCycle(
            out int closedM5,
            out bool newClosedBar,
            out DateTime reference,
            out MtfClosedContext mtf)
        {
            closedM5 = -1;
            newClosedBar = false;
            reference = DateTime.MinValue;
            mtf = null;

            if (!IsLastBar ||
                Bars == null)
                return false;

            DateTime now =
                CanonicalTimeRule.EnsureUtc(
                    Server.TimeInUtc);

            if (_lastMtfClosedContext != null)
            {
                _calculationMarketContext =
                    BuildCalculationMarketContext(
                        Bars.Count - 1,
                        now,
                        _lastMtfClosedContext);
            }

            if (!ShouldProbeCalculationReadiness(
                    now))
                return false;

            bool hasMinimumHistory =
                HasEnoughData();

            if (!hasMinimumHistory)
            {
                RecordCalculationReadiness(
                    CalculationReadinessState.BuildingHistory,
                    now);

                RenderCalculationReadinessIfNeeded(
                    now);
                return false;
            }

            reference =
                now;

            mtf =
                BuildMtfClosedContext(
                    reference);

            _lastMtfClosedContext =
                mtf;

            _calculationMarketContext =
                BuildCalculationMarketContext(
                    Bars == null ? -1 : Bars.Count - 1,
                    reference,
                    mtf);

            RefreshClosedM1Frame(
                mtf.M1);

            closedM5 =
                mtf.M5;

            CalculationReadinessState readiness =
                CalculationReadinessRule.ResolveState(
                    true,
                    true,
                    mtf.HasPrimaryDecisionHistory,
                    closedM5);

            RecordCalculationReadiness(
                readiness,
                now);

            if (readiness !=
                CalculationReadinessState.Ready)
            {
                RenderCalculationReadinessIfNeeded(
                    now);
                return false;
            }

            newClosedBar =
                closedM5 !=
                _lastEvaluatedM5;

            return true;
        }

        
        private void RefreshClosedM1Frame(
            int closedM1)
        {
            if (_m1Bars == null ||
                closedM1 < 30 ||
                closedM1 >= _m1Bars.Count - 1 ||
                closedM1 == _lastPanelM1ClosedIndex)
                return;

            _m1Frame =
                AnalyzeFrame(
                    _m1Bars,
                    closedM1);

            _lastPanelM1ClosedIndex =
                closedM1;
        }
    
    }
}
