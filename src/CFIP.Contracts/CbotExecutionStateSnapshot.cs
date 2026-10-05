using System;

namespace CFIP.Contracts
{
    /// <summary>
    /// Broker-facing cBot runtime state published for the Indicator panel.
    /// The snapshot is observational only; it never grants broker authority
    /// to the Indicator.
    /// </summary>
    public sealed record CbotExecutionStateSnapshot(
        int ContractVersion,
        string IndicatorInstanceId,
        string Symbol,
        DateTime ObservedUtc,
        long Revision,
        string RuntimeState,
        string Reason,
        bool DemoAccount,
        bool MarketExecutionEnabled,
        bool PendingStopExecutionEnabled,
        bool PendingLimitExecutionEnabled,
        bool AggressiveExecutionEnabled,
        bool ManagementExecutionEnabled,
        int ManagedPositions,
        int ManagedPendingOrders,
        long? BrokerPositionId,
        long? BrokerPendingOrderId,
        double? Entry,
        double? Stop,
        double? Target,
        string ExecutionLabel,
        string ExecutionScenarioId,
        long SignalRevision)
    {
        public bool CbotAutoTradingEnabled { get; init; }
        public bool CbotAutomaticOrdersEnabled { get; init; }
        public bool EffectiveAutoTradingEnabled { get; init; }
        public bool EffectiveAutomaticOrdersEnabled { get; init; }
        public string LifecycleState { get; init; } = "UNKNOWN";
        public string ProtectionState { get; init; } = "UNKNOWN";
        public bool RecoveryRequired { get; init; }
        public string RecoveryReason { get; init; } = string.Empty;
        public string ExecutionAccountMode { get; init; } = "UNKNOWN";
    }
}