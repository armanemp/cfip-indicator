// ============================================================================
// CFIP Indicator — ReversalCloseGuard.cs
// Single responsibility: protective reversal-exit gating.
// ============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void CheckReversalProtection()
        {
            if (!EnableReversalProtectionClose ||
                _decision == null ||
                _decision.Direction == 0 ||
                (_plan != null &&
                 _lifecycleState !=
                    LifecycleState.LivePosition))
                return;

            foreach (Position position in Positions)
            {
                if (position == null ||
                    position.SymbolName != SymbolName)
                    continue;

                if (!IsManagedPosition(position))
                    continue;

                int positionDirection =
                    position.TradeType == TradeType.Buy ? 1 : -1;

                if (_decision.Direction == positionDirection ||
                    _decision.SmartQuality < ReversalProtectionMinimumQuality ||
                    _decision.IndependentEvidence <
                        Math.Max(2, ReversalCloseMinimumEvidence) ||
                    _decision.TimeframeAgreement <
                        Math.Max(50, ReversalCloseMinimumMtf))
                    continue;

                if (!IsDecisiveOppositeDirection(positionDirection) ||
                    position.NetProfit <= 0)
                    continue;

                double protectedProfit = position.NetProfit;

                if (_plan != null &&
                    _plan.IsLivePosition)
                {
                    SetLifecycleState(
                        LifecycleState.ExitRequested,
                        "REVERSAL PROTECTION");
                }

                if (!TryClosePosition(
                        position,
                        "REVERSAL PROTECTION"))
                {
                    if (_plan != null &&
                        _plan.IsLivePosition)
                    {
                        SetLifecycleState(
                            LifecycleState.RecoveryRequired,
                            "REVERSAL PROTECTION • EXIT REJECTED");
                    }

                    continue;
                }

                SendUnifiedAlert(
                    "REVERSAL-CLOSE|" + position.Id,
                    "CFIP REVERSAL EXIT REQUESTED | #" +
                    position.Id +
                    " | protected +" +
                    protectedProfit.ToString("F2") +
                    " | Q " +
                    _decision.SmartQuality +
                    " | MTF " +
                    _decision.TimeframeAgreement +
                    " | EVID " +
                    _decision.IndependentEvidence,
                    positionDirection,
                    true);
            }
        }
    }
}
