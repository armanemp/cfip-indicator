using System;
using System.Collections.Generic;
using cAlgo.API;

namespace CFIP.cBot.Execution
{
    /// <summary>
    /// Single cBot lifecycle/execution audio owner.
    /// Indicator signal audio remains owned by the Indicator alert rail;
    /// this service covers cBot lifecycle and broker-execution outcomes only.
    /// </summary>
    internal sealed class CbotLifecycleAudioService
    {
        private readonly Dictionary<string, DateTime> _lastPlayedUtc =
            new Dictionary<string, DateTime>(StringComparer.Ordinal);

        private static readonly TimeSpan Debounce =
            TimeSpan.FromMilliseconds(1500);

        public void PlayStarted(
            Robot robot)
        {
            Play(
                robot,
                "CBOT STARTED",
                SoundType.PositiveNotification);
        }

        public void PlayStopped(
            Robot robot)
        {
            Play(
                robot,
                "CBOT STOPPED",
                SoundType.NegativeNotification);
        }

        public void PlayExecutionConfirmed(
            Robot robot,
            string scenarioId)
        {
            Play(
                robot,
                "EXECUTION CONFIRMED|" +
                (scenarioId ?? string.Empty),
                SoundType.PositiveNotification);
        }

        public void PlayExecutionRejected(
            Robot robot,
            string scenarioId)
        {
            Play(
                robot,
                "EXECUTION REJECTED|" +
                (scenarioId ?? string.Empty),
                SoundType.NegativeNotification);
        }

        public void PlayRecoveryRequired(
            Robot robot,
            string scenarioId)
        {
            Play(
                robot,
                "RECOVERY REQUIRED|" +
                (scenarioId ?? string.Empty),
                SoundType.NegativeNotification);
        }

        private void Play(
            Robot robot,
            string key,
            SoundType soundType)
        {
            if (robot == null ||
                string.IsNullOrWhiteSpace(key))
                return;

            DateTime nowUtc =
                robot.Server.TimeInUtc;

            if (_lastPlayedUtc.TryGetValue(
                    key,
                    out DateTime lastUtc) &&
                nowUtc - lastUtc < Debounce)
                return;

            _lastPlayedUtc[key] = nowUtc;

            try
            {
                robot.Notifications.PlaySound(
                    soundType);

                robot.Print(
                    "CFIP CBOT AUDIO | event={0} | cue={1}",
                    key,
                    soundType);
            }
            catch (Exception ex)
            {
                robot.Print(
                    "CFIP CBOT AUDIO FAILED | event={0} | cue={1} | error={2}",
                    key,
                    soundType,
                    ex.Message);
            }

            if (_lastPlayedUtc.Count > 128)
                TrimOldEntries(nowUtc);
        }

        private void TrimOldEntries(
            DateTime nowUtc)
        {
            var cutoff =
                nowUtc - TimeSpan.FromMinutes(5);

            var expired =
                new List<string>();

            foreach (KeyValuePair<string, DateTime> entry
                     in _lastPlayedUtc)
            {
                if (entry.Value < cutoff)
                    expired.Add(entry.Key);
            }

            foreach (string key in expired)
                _lastPlayedUtc.Remove(key);
        }
    }
}
