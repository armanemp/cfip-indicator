using System;

namespace CFIP.Contracts
{
    public sealed record SignalScenarioBatch(
        int ContractVersion,
        string IndicatorInstanceId,
        string Symbol,
        DateTime ObservedUtc,
        long Revision,
        SignalEnvelope[] Scenarios);
}
