using System.Collections.Generic;
using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private const int MaxAlertDeliveriesPerPump = 4;

        // The same causal alert may pass through several presentation stages.
        // Only one audible cue is allowed for one canonical event group.
        private const int MaxRememberedAlertSoundGroups = 256;
        private readonly HashSet<string> _rememberedAlertSoundGroups =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly Queue<string> _rememberedAlertSoundGroupOrder =
            new Queue<string>(MaxRememberedAlertSoundGroups);

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

                RememberAlertSoundGroup(
                    soundGroupKey);
            }
        }

        private bool ShouldQueueAlertSound(
            AlertDelivery delivery,
            out string groupKey)
        {
            groupKey = string.Empty;

            if (!delivery.PlaySound ||
                delivery.Envelope == null)
                return false;

            groupKey = delivery.SoundGroupKey;

            if (string.IsNullOrWhiteSpace(groupKey))
                return true;

            if (_rememberedAlertSoundGroups.Contains(groupKey))
            {
                Print(
                    "CFIP ALERT SOUND SUPPRESSED | group={0} | alreadyPlayed=true",
                    groupKey);
                return false;
            }

            return true;
        }

        private void RememberAlertSoundGroup(string groupKey)
        {
            if (string.IsNullOrWhiteSpace(groupKey) ||
                _rememberedAlertSoundGroups.Contains(groupKey))
                return;

            _rememberedAlertSoundGroups.Add(groupKey);
            _rememberedAlertSoundGroupOrder.Enqueue(groupKey);

            while (_rememberedAlertSoundGroupOrder.Count >
                   MaxRememberedAlertSoundGroups)
            {
                string expiredGroup =
                    _rememberedAlertSoundGroupOrder.Dequeue();

                _rememberedAlertSoundGroups.Remove(expiredGroup);
            }
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
                !UseSemanticAlertSounds &&
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
