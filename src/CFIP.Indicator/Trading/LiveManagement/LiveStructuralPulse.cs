using System;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool ShouldRunLiveStructuralPulse(
            DateTime nowUtc)
        {
            if (_lastLiveStructuralPulseUtc == DateTime.MinValue ||
                (nowUtc - _lastLiveStructuralPulseUtc).TotalMilliseconds >= 1000)
            {
                _lastLiveStructuralPulseUtc = nowUtc;
                return true;
            }

            return false;
        }
    }
}