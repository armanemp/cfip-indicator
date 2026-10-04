using System.Collections.Generic;
using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private const int MaxAlertDeliveriesPerPump = 4;

        // Multiple semantic signal stages can legitimately belong to the same M5
        // market event. Each distinct canonical alert key gets its own semantic cue;
        // exact repeats are still deduplicated by this bounded delivery fingerprint.
        private const int MaxRememberedSignalSoundGroups = 256;
        private readonly HashSet<string> _rememberedSignalSoundGroups =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly Queue<string> _rememberedSignalSoundGroupOrder =
            new Queue<string>(MaxRememberedSignalSoundGroups);

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

                // Keep panel delivery and sound delivery separate. A presentation
                // failure must never suppress realtime sound, and sound playback
                // is owned by the last-bar Calculate path.
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

                if (_alertSoundDeliveryQueue == null)
                    continue;

                string soundGroupKey;
                if (!ShouldQueueAlertSound(
                        next,
                        out soundGroupKey))
                    continue;

                if (!_alertSoundDeliveryQueue.Enqueue(next))
                {
                    Print(
                        "CFIP ALERT SOUND QUEUE REJECTED | id={0}",
                        next.Envelope == null
                            ? ""
                            : next.Envelope.AlertId);
                    continue;
                }

                RememberSignalSoundGroup(
                    soundGroupKey);
            }
        }

        private void RememberSignalSoundGroup(
            string groupKey)
        {
            if (string.IsNullOrWhiteSpace(groupKey) ||
                _rememberedSignalSoundGroups.Contains(groupKey))
                return;

            _rememberedSignalSoundGroups.Add(groupKey);
            _rememberedSignalSoundGroupOrder.Enqueue(groupKey);

            while (_rememberedSignalSoundGroupOrder.Count >
                   MaxRememberedSignalSoundGroups)
            {
                string expiredGroup =
                    _rememberedSignalSoundGroupOrder.Dequeue();

                _rememberedSignalSoundGroups.Remove(
                    expiredGroup);
            }
        }

        private bool ShouldQueueAlertSound(
            AlertDelivery delivery,
            out string groupKey)
        {
            groupKey = delivery.SoundGroupKey ?? string.Empty;

            if (!delivery.PlaySound)
                return false;

            if (string.IsNullOrWhiteSpace(groupKey))
                return true;

            if (_rememberedSignalSoundGroups.Contains(groupKey))
            {
                Print(
                    "CFIP ALERT SOUND SUPPRESSED | group={0} | alreadyPlayed=true",
                    groupKey);
                return false;
            }

            return true;
        }


        private void ProcessQueuedAlertSoundDelivery()
        {
            // cTrader indicator sound playback stays on the realtime last-bar
            // Calculate boundary rather than the timer/panel rendering path.
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
