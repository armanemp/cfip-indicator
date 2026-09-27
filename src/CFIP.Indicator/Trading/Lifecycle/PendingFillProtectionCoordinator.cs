// CFIP Indicator — PendingFillProtectionCoordinator.cs
// Single-responsibility broker-protection coordination after a confirmed pending fill.

using System;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void ApplyPendingFillProtection(
            Position position,
            double stop,
            double target,
            int direction,
            bool protectionMissing)
        {
            _activeBrokerStop =
                position.StopLoss.HasValue
                    ? NormalizePrice(position.StopLoss.Value)
                    : 0;

            _activeBrokerTarget =
                position.TakeProfit.HasValue
                    ? NormalizePrice(position.TakeProfit.Value)
                    : 0;

            _brokerProtectionRecoveryRequired =
                protectionMissing;

            SetLifecycleState(
                protectionMissing
                    ? LifecycleState.RecoveryRequired
                    : LifecycleState.LivePosition,
                protectionMissing
                    ? "PENDING FILL • BROKER PROTECTION MISSING"
                    : "PENDING FILL • LIVE");

            if (!AutoBrokerProtection)
                return;

            bool protectedOk =
                EnsureBrokerProtectionForPosition(
                    position,
                    stop,
                    target,
                    "PENDING FILL",
                    direction);

            if (!protectedOk)
                return;

            _activeBrokerStop = stop;
            _activeBrokerTarget = target;
            _brokerProtectionRecoveryRequired = false;

            SetLifecycleState(
                LifecycleState.LivePosition,
                "PENDING FILL • PROTECTED");
        }
    }
}
