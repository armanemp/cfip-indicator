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

        private int ResolveSignalSoundPriority(
            string key)
        {
            string normalized =
                key ?? string.Empty;

            if (normalized.StartsWith(
                    "ACTION|",
                    StringComparison.OrdinalIgnoreCase) ||
                normalized.StartsWith(
                    "HIGH|",
                    StringComparison.OrdinalIgnoreCase) ||
                normalized.StartsWith(
                    "SMART|",
                    StringComparison.OrdinalIgnoreCase))
                return 3;

            if (normalized.StartsWith(
                    "EARLY|",
                    StringComparison.OrdinalIgnoreCase) ||
                normalized.StartsWith(
                    "WATCH|",
                    StringComparison.OrdinalIgnoreCase))
                return 2;

            if (normalized.StartsWith(
                    "REACTION|",
                    StringComparison.OrdinalIgnoreCase) ||
                normalized.StartsWith(
                    "AUTO-REACTION|",
                    StringComparison.OrdinalIgnoreCase))
                return 1;

            return 0;
        }

        private bool IsSignalSoundAlertKey(
            string key)
        {
            string normalized =
                key ?? string.Empty;

            return
                normalized.StartsWith("WATCH|", StringComparison.OrdinalIgnoreCase) ||
                normalized.StartsWith("REACTION|", StringComparison.OrdinalIgnoreCase) ||
                normalized.StartsWith("ACTION|", StringComparison.OrdinalIgnoreCase) ||
                normalized.StartsWith("HIGH|", StringComparison.OrdinalIgnoreCase) ||
                normalized.StartsWith("SMART|", StringComparison.OrdinalIgnoreCase) ||
                normalized.StartsWith("EARLY|", StringComparison.OrdinalIgnoreCase) ||
                normalized.StartsWith("AUTO-REACTION|", StringComparison.OrdinalIgnoreCase);
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
            groupKey = string.Empty;
            if (!delivery.PlaySound)
                return false;

            int priority =
                ResolveSignalSoundPriority(
                    delivery.Key);

            if (priority <= 0 ||
                delivery.Envelope == null ||
                delivery.Envelope.Identity == null)
                return true;

            string signalId =
                delivery.Envelope.Identity.SignalId ?? string.Empty;

            int createdClosedM5 =
                delivery.Envelope.Identity.CreatedClosedM5;

            if (string.IsNullOrWhiteSpace(signalId) ||
                createdClosedM5 < 0)
                return true;

            // The alert key is part of the sound fingerprint. This preserves
            // one cue per distinct semantic stage (WATCH, REACTION, ACTION, TP, SL,
            // REVERSAL, execution outcome, etc.) while exact duplicate deliveries
            // remain silent. Signal-family classification is retained for diagnostics
            // and priority handling, but never collapses distinct stages into one cue.
            groupKey =
                (SymbolName ?? string.Empty) +
                "|" +
                signalId +
                "|" +
                delivery.Envelope.Identity.ScenarioId +
                "|" +
                delivery.Envelope.Identity.PlanId +
                "|" +
                createdClosedM5.ToString() +
                "|" +
                delivery.Direction.ToString() +
                "|" +
                (delivery.Key ?? string.Empty);


            if (_rememberedSignalSoundGroups.Contains(groupKey))
            {
                Print(
                    "CFIP ALERT SOUND SUPPRESSED | group={0} | priority={1} | alreadyPlayed=true",
                    groupKey,
                    priority);
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
