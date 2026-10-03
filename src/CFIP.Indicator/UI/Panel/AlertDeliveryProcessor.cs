using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private const int MaxAlertDeliveriesPerPump = 4;

        // Multiple semantic signal alerts can legitimately target the same M5
        // market event (WATCH/REACTION/ACTION, parallel lanes, etc.). They remain
        // visible in the panel, but only one sound is emitted per event and
        // direction; a higher-priority escalation may replace a lower one.
        private string _lastSignalSoundGroupKey = string.Empty;
        private int _lastSignalSoundPriority;

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

                if (!ShouldQueueAlertSound(next) ||
                    _alertSoundDeliveryQueue == null)
                    continue;

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

        private bool ShouldQueueAlertSound(
            AlertDelivery delivery)
        {
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

            string groupKey =
                (SymbolName ?? string.Empty) +
                "|" +
                signalId +
                "|" +
                createdClosedM5.ToString() +
                "|" +
                delivery.Direction.ToString();

            if (string.Equals(
                    groupKey,
                    _lastSignalSoundGroupKey,
                    StringComparison.OrdinalIgnoreCase) &&
                priority <= _lastSignalSoundPriority)
            {
                Print(
                    "CFIP ALERT SOUND SUPPRESSED | group={0} | priority={1} | previousPriority={2}",
                    groupKey,
                    priority,
                    _lastSignalSoundPriority);
                return false;
            }

            _lastSignalSoundGroupKey =
                groupKey;

            _lastSignalSoundPriority =
                priority;

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
