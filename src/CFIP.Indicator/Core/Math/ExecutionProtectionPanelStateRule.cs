namespace cAlgo
{
    internal enum ExecutionPanelStateKind
    {
        Disabled,
        Armed,
        Ready,
        Active,
        Blocked,
        RecoveryRequired
    }

    internal enum ProtectionPanelStateKind
    {
        Off,
        NoLivePosition,
        Protected,
        RecoveryRequired
    }

    internal static class ExecutionProtectionPanelStateRule
    {
        public static ExecutionPanelStateKind ResolveAutoTrading(
            bool enabled,
            bool active,
            bool ready,
            bool blocked,
            bool recoveryRequired)
        {
            if (!enabled)
                return ExecutionPanelStateKind.Disabled;

            if (recoveryRequired)
                return ExecutionPanelStateKind.RecoveryRequired;

            if (active)
                return ExecutionPanelStateKind.Active;

            if (blocked)
                return ExecutionPanelStateKind.Blocked;

            if (ready)
                return ExecutionPanelStateKind.Ready;

            return ExecutionPanelStateKind.Armed;
        }

        public static ExecutionPanelStateKind ResolveAutoOrders(
            bool enabled,
            bool active,
            bool ready,
            bool blocked,
            bool recoveryRequired)
        {
            if (!enabled)
                return ExecutionPanelStateKind.Disabled;

            if (recoveryRequired)
                return ExecutionPanelStateKind.RecoveryRequired;

            if (active)
                return ExecutionPanelStateKind.Active;

            if (blocked)
                return ExecutionPanelStateKind.Blocked;

            if (ready)
                return ExecutionPanelStateKind.Ready;

            return ExecutionPanelStateKind.Armed;
        }

        public static ProtectionPanelStateKind ResolveProtection(
            bool monitoringConfigured,
            bool livePosition,
            bool brokerStopValid,
            bool targetRequired,
            bool brokerTargetValid,
            bool serverLadderActive,
            bool recoveryRequired)
        {
            if (!monitoringConfigured && !livePosition)
                return ProtectionPanelStateKind.Off;

            if (recoveryRequired)
                return ProtectionPanelStateKind.RecoveryRequired;

            if (!livePosition)
                return ProtectionPanelStateKind.NoLivePosition;

            bool targetProtected =
                !targetRequired ||
                serverLadderActive ||
                brokerTargetValid;

            return brokerStopValid && targetProtected
                ? ProtectionPanelStateKind.Protected
                : ProtectionPanelStateKind.RecoveryRequired;
        }
    }
}
