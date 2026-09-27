// ============================================================================
// CFIP Indicator — LiveExitGuards.cs
// One responsibility per module. Behavioral parity with v73 is preserved.
// ============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool HasManagedOpenPosition()
                        {
                            foreach (Position position in Positions)
                            {
                                if (position == null ||
                                    position.SymbolName !=
                                    SymbolName)
                                    continue;
                
                                if (!IsManagedPosition(position))
                                    continue;
                
                                return true;
                            }
                
                            return false;
                        }
        
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
                                    _decision.IndependentEvidence < Math.Max(2, ReversalCloseMinimumEvidence) ||
                                    _decision.TimeframeAgreement < Math.Max(50, ReversalCloseMinimumMtf))
                                    continue;
                
                                if (!IsDecisiveOppositeDirection(positionDirection) ||
                                    position.NetProfit <= 0)
                                    continue;
                
                                double protectedProfit =
                                    position.NetProfit;
                
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
                                    "REVERSAL-CLOSE|" +
                                    position.Id,
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
                                evidence < Math.Max(1, ReversalCloseMinimumEvidence))
                                return false;
                
                            if (RequireReversalForce &&
                                !m5Force)
                                return false;
                
                            if (RequireM15ReversalForOpposite &&
                                (!m15Aligned || !m15Structure))
                                return false;
                
                            return true;
                        }
        
        private bool ExecutePartialClose(
                            double percentOfOriginal,
                            string tag)
                        {
                            if (!EnablePartialTakeProfit ||
                                _plan == null ||
                                _plan.OriginalVolume <= 0 ||
                                percentOfOriginal <= 0)
                                return true;
                
                            foreach (Position position in Positions)
                            {
                                if (position == null ||
                                    position.SymbolName !=
                                    SymbolName ||
                                    !IsManagedPosition(position))
                                    continue;
                
                                double closeVolume =
                                    Symbol.NormalizeVolumeInUnits(
                                        _plan.OriginalVolume *
                                        percentOfOriginal /
                                        100.0,
                                        RoundingMode.Down);
                
                                closeVolume =
                                    Math.Min(
                                        closeVolume,
                                        position.VolumeInUnits);
                
                                if (closeVolume <
                                    Symbol.VolumeInUnitsMin)
                                    return false;
                
                                double remainder =
                                    position.VolumeInUnits -
                                    closeVolume;
                
                                if (remainder > 0 &&
                                    remainder <
                                    Symbol.VolumeInUnitsMin)
                                    closeVolume =
                                        position.VolumeInUnits;
                
                                try
                                {
                                    bool closingEverything =
                                        closeVolume >=
                                        position.VolumeInUnits;
                
                                    TradeResult closeResult =
                                        closingEverything
                                            ? ClosePosition(position)
                                            : ClosePosition(
                                                position,
                                                closeVolume);
                
                                    if (closeResult == null ||
                                        !closeResult.IsSuccessful)
                                    {
                                        Print(
                                            "CFIP partial close rejected ({0}): {1}",
                                            tag,
                                            closeResult != null &&
                                            closeResult.Error.HasValue
                                                ? closeResult.Error.Value.ToString()
                                                : "UNKNOWN");
                                        return false;
                                    }
                
                                    if (MoveToBreakEvenAfterPartial &&
                                        !closingEverything)
                                    {
                                        bool shouldMove =
                                            !position.StopLoss.HasValue ||
                                            BetterStop(
                                                _plan.Direction,
                                                position.EntryPrice,
                                                position.StopLoss.Value);
                
                                        if (shouldMove)
                                        {
                                            TryModifyStopLoss(
                                                position,
                                                position.EntryPrice,
                                                "PARTIAL BREAK-EVEN");
                                        }
                                    }
                
                                    if (EnableLevelHitAlerts &&
                                        AlertOnLevelHit)
                                    {
                                        SendUnifiedAlert(
                                            "PARTIAL|" +
                                            tag +
                                            "|" +
                                            _plan.CreatedM5,
                                            "CFIP partial close at " +
                                            tag +
                                            " - closed " +
                                            percentOfOriginal.ToString(
                                                "F0") +
                                            "% of original size",
                                            _plan.Direction,
                                            false);
                                    }
                
                                    return true;
                                }
                                catch (Exception ex)
                                {
                                    Print(
                                        "CFIP partial close failed ({0}): {1}",
                                        tag,
                                        ex.Message);
                                    return false;
                                }
                            }
                
                            return false;
                        }
    }
}
