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

            RemoveExpiredPopup();

            if (!HasEnoughData())
            {
                _status = "BUILDING DATA";
                RenderPanel();
                return false;
            }

            reference =
                Server.TimeInUtc;

            mtf =
                BuildMtfClosedContext(
                    reference);

            _lastMtfClosedContext =
                mtf;

            RefreshClosedM1Frame(
                mtf.M1);

            closedM5 =
                mtf.M5;

            if (!mtf.HasPrimaryDecisionHistory)
            {
                _status =
                    "WAITING FOR CLOSED M5";
                RenderPanel();
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
