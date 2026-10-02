using System;

namespace CFIP.Contracts
{
    // One immutable event source for popup, sound and signal presentation metadata.
    // This contract contains identity only; it has no cTrader or broker behavior.
    public sealed record AlertEnvelope(
        ContractIdentity Identity,
        SignalStage Stage,
        string AlertId,
        string AlertKey,
        string Message,
        bool Critical,
        bool VisualMarkAllowed,
        DateTime EventUtc);
}
