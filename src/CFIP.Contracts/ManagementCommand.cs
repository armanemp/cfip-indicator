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
        string CommandIdempotencyKey)
    {
        public string ExecutionLabel { get; init; }
        public double? DesiredTargetPips { get; init; }
        public double? ExpectedRemainingVolume { get; init; }
        public double? LadderFirstVolume { get; init; }
        public double? LadderFirstTargetPips { get; init; }
        public double? LadderSecondVolume { get; init; }
        public double? LadderSecondTargetPips { get; init; }
        public double? LadderFinalTargetPips { get; init; }
    }
}
