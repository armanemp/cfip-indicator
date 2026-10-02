using System;

namespace CFIP.Contracts
{
    public sealed record ContractIdentity(
        int ContractVersion,
        string SignalId,
        string ScenarioId,
        string PlanId,
        string Symbol,
        TradeDirection Direction,
        OpportunityLane Lane,
        string SourceTimeframe,
        DateTime CreatedUtc,
        int CreatedClosedM5,
        DateTime? ExpiryUtc,
        long Revision,
        string CorrelationId,
        string IdempotencyKey);
}
