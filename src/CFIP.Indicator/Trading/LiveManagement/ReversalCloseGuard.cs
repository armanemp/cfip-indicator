// ============================================================================
// CFIP Indicator — ReversalCloseGuard.cs
// Single responsibility: protective reversal-exit gating.
// ============================================================================

using System;
using CFIP.Contracts;
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
                        ExecutionThresholdPolicy.NormalizeReversalEvidence(ReversalCloseMinimumEvidence) ||
                    _decision.TimeframeAgreement <
                        ExecutionThresholdPolicy.NormalizeReversalMtf(ReversalCloseMinimumMtf))
                    continue;

                if (!IsDecisiveOppositeDirection(positionDirection) ||
                    !ReversalProfitThresholdRule.MeetsMinimumNetProfit(
                        position.NetProfit,
                        ReversalCloseMinimumNetProfit))
                    continue;

                double protectedProfit = position.NetProfit;

                if (_plan != null &&
                    _plan.IsLivePosition)
                {
                    SetLifecycleState(
                        LifecycleState.ExitRequested,
                        "REVERSAL PROTECTION");
                }

                ManagementCommandRequestStatus closeStatus =
                    RequestClosePosition(
                        position,
                        "REVERSAL PROTECTION");
                if (!closeStatus.IsAccepted())
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
        private bool IsDecisiveOppositeDirection(int positionDirection)
        {
            if (_m5Frame == null ||
                _m5Bars == null ||
                positionDirection == 0)
                return false;

            int opposite = positionDirection * -1;

            bool m5Structural =
                opposite == 1
                    ? (_m5Frame.MssBull || _m5Frame.ChochBull)
                    : (_m5Frame.MssBear || _m5Frame.ChochBear);

            bool m5Force =
                opposite == 1
                    ? (_m5Frame.DisplacementBull &&
                       _m5Frame.LiquidityBull)
                    : (_m5Frame.DisplacementBear &&
                       _m5Frame.LiquidityBear);

            bool m15Aligned =
                _m15Frame != null &&
                _m15Frame.Direction == opposite;

            bool m15Structure =
                m15Aligned &&
                (opposite == 1
                    ? (_m15Frame.MssBull || _m15Frame.ChochBull)
                    : (_m15Frame.MssBear || _m15Frame.ChochBear));

            int evidence = 0;

            if (opposite == 1)
            {
                if (_m5Frame.MssBull) evidence++;
                if (_m5Frame.ChochBull) evidence++;
                if (_m5Frame.DisplacementBull) evidence++;
                if (_m5Frame.LiquidityBull) evidence++;
            }
            else
            {
                if (_m5Frame.MssBear) evidence++;
                if (_m5Frame.ChochBear) evidence++;
                if (_m5Frame.DisplacementBear) evidence++;
                if (_m5Frame.LiquidityBear) evidence++;
            }

            if (!m5Structural ||
                evidence < ExecutionThresholdPolicy.NormalizeReversalEvidence(ReversalCloseMinimumEvidence))
                return false;

            if (RequireReversalForce &&
                !m5Force)
                return false;

            if (RequireM15ReversalForOpposite &&
                (!m15Aligned || !m15Structure))
                return false;

            return true;
        }

    }
}
