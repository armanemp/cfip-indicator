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

                // One canonical alert event feeds both presentation channels:
                // the bounded panel alert rail first, then the optional sound cue.
                RecordPanelAlertDelivery(next);

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
                SoundType soundType;

                if (!Enum.TryParse<SoundType>(
                        delivery.SoundTypeName,
                        true,
                        out soundType))
                {
                    soundType = AlertSoundType;
                }

                Notifications.PlaySound(soundType);

                Print(
                    "CFIP ALERT SOUND DELIVERED | id={0} | source=SEMANTIC | cue={1}",
                    delivery.Envelope == null
                        ? ""
                        : delivery.Envelope.AlertId,
                    soundType);
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
