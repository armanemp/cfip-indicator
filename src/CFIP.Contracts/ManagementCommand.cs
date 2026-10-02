using System;

namespace CFIP.Contracts
{
    public sealed record ManagementCommand(
        ContractIdentity Identity,
        ManagementCommandType Command,
        long CommandRevision,
        long? PositionId,
        long? PendingOrderId,
        double? DesiredStop,
        double? DesiredTarget,
        double? PartialCloseVolume,
        DateTime RequestedUtc,
        string Reason,
        string CommandIdempotencyKey);
}
