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
