using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private const int MaxAlertDeliveriesPerPump = 4;

        private void ProcessQueuedAlertDelivery()
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

                // Sound delivery is intentionally independent from panel presentation.
                // A panel rendering problem must never suppress a realtime audio cue.
                if (next.PlaySound)
                    DeliverAlertSound(next);

                try
                {
                    RecordPanelAlertDelivery(next);
                }
                catch (Exception ex)
                {
                    Print(
                        "CFIP alert panel delivery failed [{0}]: {1}",
                        next.Key,
                        ex.Message);
                }
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
