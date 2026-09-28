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

            RefreshLiveM1Frame();

            RemoveExpiredPopup();

            if (!HasEnoughData())
            {
                _status = "BUILDING DATA";
                RenderPanel();
                return false;
            }

            reference =
                _m5Bars.OpenTimes[
                    _m5Bars.Count - 1];

            mtf =
                BuildMtfClosedContext(
                    reference);

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

        private void RefreshLiveM1Frame()
        {
            if (_m1Bars == null ||
                _m1Bars.Count < 32)
                return;

            int closedM1 =
                _m1Bars.Count - 2;

            if (closedM1 < 30 ||
                closedM1 == _lastLiveM1FrameIndex)
                return;

            _m1Frame =
                AnalyzeFrame(
                    _m1Bars,
                    closedM1);

            _lastLiveM1FrameIndex =
                closedM1;
        }
    }
}
