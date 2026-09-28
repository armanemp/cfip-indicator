// ============================================================================
// CFIP Indicator — PartialTakeProfitExecutor.cs
// Single responsibility: confirmed partial take-profit mutation.
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
                    position.SymbolName != SymbolName ||
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

                    bool closeAccepted =
                        TryClosePosition(
                            position,
                            "PARTIAL CLOSE • " + tag,
                            closingEverything
                                ? (double?)null
                                : closeVolume);

                    if (!closeAccepted)
                    {
                        Print(
                            "CFIP partial close rejected ({0}).",
                            tag);
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
                            double market =
                                _plan.Direction == 1
                                    ? Symbol.Bid
                                    : Symbol.Ask;

                            bool breakEvenValid =
                                IsFinitePositive(market) &&
                                IsValidManagedStop(
                                    _plan.Direction,
                                    position.EntryPrice,
                                    market,
                                    position.EntryPrice);

                            bool breakEvenApplied =
                                breakEvenValid &&
                                TryModifyStopLoss(
                                    position,
                                    position.EntryPrice,
                                    "PARTIAL BREAK-EVEN");

                            if (breakEvenApplied)
                            {
                                _plan.Stop =
                                    NormalizePrice(
                                        position.EntryPrice);

                                _brokerProtectionRecoveryRequired =
                                    false;
                            }
                            else
                            {
                                _brokerProtectionRecoveryRequired =
                                    true;

                                SetLifecycleState(
                                    LifecycleState.RecoveryRequired,
                                    "PARTIAL CLOSE • BREAK-EVEN REJECTED");

                                SendUnifiedAlert(
                                    "BREAKEVEN-REJECTED|" +
                                    position.Id +
                                    "|" +
                                    tag,
                                    "CFIP BREAK-EVEN PROTECTION REJECTED | #" +
                                    position.Id +
                                    " | " +
                                    tag,
                                    _plan.Direction,
                                    true);
                            }
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
                            percentOfOriginal.ToString("F0") +
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
