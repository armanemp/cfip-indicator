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
        MarketExecutionProfile MarketProfile);
}
