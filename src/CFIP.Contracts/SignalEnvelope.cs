using System;

namespace CFIP.Contracts
{
    public sealed record SignalEnvelope(
        ContractIdentity Identity,
        SignalStage Stage,
        PlanSnapshot Plan,
        ExecutionIntent Intent,
        DateTime ObservedUtc);
}
