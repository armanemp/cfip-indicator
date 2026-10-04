using System;
using CFIP.Contracts;
using cAlgo.API;

namespace cAlgo
{
    internal readonly struct AlertSoundDecision
    {
        public AlertSoundDecision(SoundType soundType, string groupKey)
        {
            SoundType = soundType;
            GroupKey = groupKey;
        }

        public SoundType SoundType { get; }
        public string GroupKey { get; }
    }

    internal static class AlertSoundPolicy
    {
        public static AlertSoundDecision Resolve(
            string key,
            bool useSemanticSounds,
            SoundType configuredSoundType,
            AlertEnvelope envelope,
            string symbolName)
        {
            string normalized = key ?? string.Empty;
            SoundType soundType = useSemanticSounds
                ? ResolveSemanticSound(normalized, envelope)
                : configuredSoundType;

            return new AlertSoundDecision(
                soundType,
                BuildGroupKey(normalized, envelope, symbolName));
        }

        private static SoundType ResolveSemanticSound(string key, AlertEnvelope envelope)
        {
            if (StartsWithAny(key, "SL|", "INVALID", "INVALIDPLAN|",
                "INVALID-RISK|", "RESTRICT|", "REVERSAL|",
                "REVERSAL-CLOSE|", "EXHAUSTION-CLOSE|", "FILL-MISMATCH|",
                "PROTECTION-REJECTED|", "PROTECTION-SYNC-FAILED|",
                "ORPHAN-PROTECTION-FAILED|", "BREAKEVEN-REJECTED|",
                "STRUCT-INVALID", "STRUCT-INVALID-EXIT-FAILED|",
                "PENDING-FILL-RECONCILIATION-FAILED|",
                "PENDING-FILL-LADDER-RECONCILIATION-FAILED|",
                "AGGRESSIVE-POST-FILL-RECONCILIATION-FAILED|", "DAILYLOSS|"))
                return SoundType.NegativeNotification;

            if (StartsWithAny(key, "ACTION|", "HIGH|", "SMART|", "TP",
                "PARTIAL|", "POSITION-OPEN|", "PENDING-FILL|"))
                return SoundType.PositiveNotification;

            if (StartsWithAny(key, "WATCH|", "EARLY|", "DAYEND|", "AUTOOFF|",
                "PLANUPDATE|", "OUTCOME-TIMEOUT|"))
                return SoundType.Announcement;

            if (StartsWithAny(key, "REACTION|", "AUTO-REACTION|", "BOS|",
                "MSS|", "SWEEP|"))
                return SoundType.Doorbell;

            if (StartsWithAny(key, "PENDING-", "PROTECTION-", "EXECUTION-"))
                return SoundType.NegativeNotification;

            return envelope != null && envelope.Critical
                ? SoundType.Confirmation
                : SoundType.Announcement;
        }

        private static string BuildGroupKey(string key, AlertEnvelope envelope, string symbolName)
        {
            if (envelope == null || envelope.Identity == null)
                return "ALERT|" + (symbolName ?? string.Empty) + "|" + key;

            ContractIdentity identity = envelope.Identity;
            bool signalFamily = StartsWithAny(key, "WATCH|", "EARLY|",
                "REACTION|", "AUTO-REACTION|", "ACTION|", "HIGH|", "SMART|");

            if (signalFamily)
                return "SIGNAL|" + (symbolName ?? string.Empty) + "|" +
                    identity.CreatedClosedM5.ToString() + "|" +
                    identity.Direction.ToString();

            return "ALERT|" + (symbolName ?? string.Empty) + "|" +
                (identity.SignalId ?? string.Empty) + "|" +
                (identity.ScenarioId ?? string.Empty) + "|" +
                (identity.PlanId ?? string.Empty) + "|" +
                identity.CreatedClosedM5.ToString() + "|" +
                identity.Direction.ToString() + "|" + key;
        }

        private static bool StartsWithAny(string value, params string[] prefixes)
        {
            for (int i = 0; i < prefixes.Length; i++)
                if (value.StartsWith(prefixes[i], StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }
    }
}