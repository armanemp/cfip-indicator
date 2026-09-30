using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void ProcessQueuedPopups()
        {
            if (_popupAlertQueue == null ||
                _popupAlertQueue.Count == 0)
                return;

            PopupAlert next;
            if (!_popupAlertQueue.TryPeek(out next))
                return;

            bool popupActive =
                _popup != null &&
                (KeepPopupUntilNextAlert ||
                 _popupUntilUtc > TimeInUtc);

            if (popupActive && !next.Critical)
                return;

            if (_popup != null)
                RemovePopup();

            if (_popupAlertQueue.TryDequeue(out next))
                ShowPopup(next.Message, next.Critical);
        }
    }
}
