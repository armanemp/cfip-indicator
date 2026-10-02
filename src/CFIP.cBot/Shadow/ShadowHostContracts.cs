using System;
using CFIP.Contracts;

namespace CFIP.cBot.Shadow
{
    public enum ShadowHostState
    {
        Waiting = 0,
        Observing = 1,
        Validating = 2,
        Ready = 3,
        Blocked = 4,
        Expired = 5,
        Duplicate = 6
    }

    public sealed record ShadowBrokerSnapshot(
        bool TradingPermissionAllowed,
        int ManagedPositionCount,
        int ManagedPendingOrderCount,
        string Symbol,
        double Bid,
        double Ask,
        double PipSize);

    public sealed record ShadowHostResult(
        ShadowHostState State,
        string Reason,
        bool NewRevision,
        long Revision,
        string SignalId,
        string ScenarioId,
        string PlanId,
        string IdempotencyKey);
}