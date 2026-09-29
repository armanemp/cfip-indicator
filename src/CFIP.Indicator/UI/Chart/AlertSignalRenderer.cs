using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void RenderLatestAlertSignalMarker(
            int fallbackM5)
        {
            // Entry visualization has one owner: SignalRenderer.
            // Alert rendering must never create a second entry arrow.
            Chart.RemoveObject(
                P + "ALERT_SIGNAL");

            RemovePlanLabel(
                P + "ALERT_SIGNAL_LABEL");
        }
    }
}
