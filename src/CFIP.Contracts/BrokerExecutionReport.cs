using System;

namespace CFIP.Contracts
{
    public sealed record BrokerExecutionReport(
        ContractIdentity Identity,
        BrokerAction Action,
        BrokerReportStatus Status,
        DateTime EventUtc,
        DateTime? SubmittedUtc,
        DateTime? ConfirmedUtc,
        long? BrokerPositionId,
        long? BrokerPendingOrderId,
        double? ConfirmedEntry,
        double? ConfirmedStop,
        double? ConfirmedTarget,
        string BrokerReference,
        string ErrorCode,
        string Reason,
        long AttemptRevision)
    {
        public string CommandIdempotencyKey { get; init; }
    }
}
