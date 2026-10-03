using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private const int MaxAlertDeliveriesPerPump = 4;

        private void ProcessQueuedAlertPresentation()
        {
            if (_alertDeliveryQueue == null ||
                _alertDeliveryQueue.Count == 0)
                return;

            int processed = 0;

            while (processed < MaxAlertDeliveriesPerPump &&
                   _alertDeliveryQueue.TryDequeue(
                       out AlertDelivery next))
            {
                processed++;

                // The panel rail can be updated from the timer without coupling
                // audio delivery to timer scheduling.
                RecordPanelAlertDelivery(next);

                if (!next.PlaySound ||
                    _alertSoundDeliveryQueue == null)
                    continue;

                // Keep the sound-bearing event alive until the Indicator's
                // realtime Calculate/IsLastBar path owns playback.
                if (!_alertSoundDeliveryQueue.Enqueue(next))
                {
                    Print(
                        "CFIP ALERT SOUND QUEUE REJECTED | id={0}",
                        next.Envelope == null
                            ? ""
                            : next.Envelope.AlertId);
                }
            }
        }

        private void ProcessQueuedAlertSoundDelivery()
        {
            // cTrader indicator sound playback is tied to the realtime
            // last-bar Calculate path. OnTimer must not be the playback owner.
            if (!IsLastBar ||
                _alertSoundDeliveryQueue == null ||
                _alertSoundDeliveryQueue.Count == 0)
                return;

            int processed = 0;

            while (processed < MaxAlertDeliveriesPerPump &&
                   _alertSoundDeliveryQueue.TryDequeue(
                       out AlertDelivery next))
            {
                processed++;

                if (!next.PlaySound)
                    continue;

                DeliverAlertSound(next);
            }
        }

        private void DeliverAlertSound(
            AlertDelivery delivery)
        {
            bool attemptedCustomFile =
                !string.IsNullOrWhiteSpace(
                    delivery.SoundFilePath);

            if (attemptedCustomFile)
            {
                try
                {
                    Notifications.PlaySound(
                        delivery.SoundFilePath);

                    Print(
                        "CFIP ALERT SOUND DELIVERED | id={0} | source=FILE | path={1}",
                        delivery.Envelope == null
                            ? ""
                            : delivery.Envelope.AlertId,
                        delivery.SoundFilePath);

                    return;
                }
                catch (Exception ex)
                {
                    Print(
                        "CFIP alert custom sound failed [{0}], falling back to semantic cue: {1}",
                        delivery.Key,
                        ex.Message);
                }
            }

            try
            {
                SoundType parsedSoundType;

                if (Enum.TryParse<SoundType>(
                        delivery.SoundTypeName,
                        true,
                        out parsedSoundType))
                {
                    Notifications.PlaySound(parsedSoundType);

                    Print(
                        "CFIP ALERT SOUND DELIVERED | id={0} | source=SEMANTIC | cue={1}",
                        delivery.Envelope == null
                            ? ""
                            : delivery.Envelope.AlertId,
                        parsedSoundType);

                    return;
                }

                Notifications.PlaySound(AlertSoundType);

                Print(
                    "CFIP ALERT SOUND DELIVERED | id={0} | source=DEFAULT | cue={1}",
                    delivery.Envelope == null
                        ? ""
                        : delivery.Envelope.AlertId,
                    AlertSoundType);
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP sound alert delivery failed [{0}]: {1}",
                    delivery.Key,
                    ex.Message);
            }
        }
    }
}
