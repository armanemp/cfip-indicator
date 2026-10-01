using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void ProcessQueuedAlertDelivery()
        {
            if (_alertDeliveryQueue == null ||
                _alertDeliveryQueue.Count == 0)
                return;

            AlertDelivery next;
            if (!_alertDeliveryQueue.TryPeek(out next))
                return;

            bool popupActive =
                _popup != null &&
                (KeepPopupUntilNextAlert ||
                 _popupUntilUtc > TimeInUtc);

            if (next.ShowPopup &&
                popupActive &&
                !next.Critical)
                return;

            if (!_alertDeliveryQueue.TryDequeue(out next))
                return;

            // Presentation is updated first; the audible cue is emitted only
            // after the same canonical event has reached its popup surface.
            if (next.ShowPopup)
            {
                if (_popup != null)
                    RemovePopup();

                ShowPopup(
                    next.Message,
                    next.Critical);
            }

            if (next.PlaySound)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(
                            next.SoundFilePath))
                    {
                        Notifications.PlaySound(
                            next.SoundFilePath);
                    }
                    else
                    {
                        SoundType soundType;

                        if (Enum.TryParse<SoundType>(
                                next.SoundTypeName,
                                true,
                                out soundType))
                        {
                            Notifications.PlaySound(
                                soundType);
                        }
                        else
                        {
                            Print(
                                "CFIP unknown semantic sound cue [{0}], using configured fallback.",
                                next.SoundTypeName);

                            Notifications.PlaySound(
                                AlertSoundType);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Print(
                        "CFIP sound alert delivery failed [{0}]: {1}",
                        next.Key,
                        ex.Message);
                }
            }
        }
    }
}
