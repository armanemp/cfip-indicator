using System;

namespace CFIP.Contracts
{
    public sealed record ExecutionIntent(
        ContractIdentity Identity,
        ExecutionAction Action,
        double RequestedEntry,
        double Stop,
        double InitialTarget,
        double? RequestedVolume,
        string SizingMode,
        DateTime RequestedUtc,
        DateTime? ExpiryUtc,
        string Reason,
        string ExecutionLabel,
        MarketExecutionProfile MarketProfile)

    {
        public ExecutionIntent(
            ContractIdentity identity,
            ExecutionAction action,
            double requestedEntry,
            double stop,
            double initialTarget,
            double? requestedVolume,
            string sizingMode,
            DateTime requestedUtc,
            DateTime? expiryUtc,
            string reason)
            : this(
                identity,
                action,
                requestedEntry,
                stop,
                initialTarget,
                requestedVolume,
                sizingMode,
                requestedUtc,
                expiryUtc,
                reason,
                "CFIP-SMART",
                null)
        {
        }
    }
}
