using System;

namespace CFIP.Contracts
{
    public sealed record LifecycleEvent(
        ContractIdentity Identity,
        LifecycleEventType EventType,
        DateTime EventUtc,
        long EventRevision,
        long? PositionId,
        long? PendingOrderId,
        BrokerReportStatus BrokerStatus,
        string Reason);
}
