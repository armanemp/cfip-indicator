// ============================================================================
// CFIP Indicator — PartialTakeProfitExecutor.cs
// Single responsibility: confirmed partial take-profit mutation.
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
        private bool ExecutePartialClose(
            int closedM5,
            double percentOfOriginal,
            string tag)
        {
            if (_serverSideTakeProfitLadderActive)
                return false;

            if (!EnablePartialTakeProfit ||
                _plan == null ||
                _plan.OriginalVolume <= 0 ||
                percentOfOriginal <= 0)
                return true;

            if (!PartialTakeProfitRetryRule.ShouldAttemptStage(
                    closedM5,
                    GetPartialTakeProfitLastAttemptM5(tag),
                    GetPartialTakeProfitLastAttemptStage(tag),
                    tag))
            {
                return false;
            }

            Position position =
                GetManagedLivePositionForPlan();

            if (position == null)
                return false;

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

            if (closeVolume < Symbol.VolumeInUnitsMin)
                return false;

            double remainder =
                position.VolumeInUnits -
                closeVolume;

            if (remainder > 0 &&
                remainder < Symbol.VolumeInUnitsMin)
                closeVolume =
                    position.VolumeInUnits;

            RecordPartialTakeProfitAttempt(
                closedM5,
                tag);

            try
            {
                bool closingEverything =
                    closeVolume >=
                    position.VolumeInUnits;

                ManagementCommandRequestStatus closeStatus =
                    RequestClosePosition(
                        position,
                        "PARTIAL CLOSE • " + tag,
                        closingEverything
                            ? (double?)null
                            : closeVolume);

                if (!closeStatus.IsBrokerConfirmed())
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
                        ProtectionProgressionRule.ShouldAdvanceStop(
                            _plan.Direction,
                            position.StopLoss.Value,
                            position.EntryPrice);

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

                        if (breakEvenValid)
                        {
                            double riskPips =
                                Math.Abs(
                                    position.EntryPrice -
                                    _plan.Stop) /
                                Math.Max(
                                    Symbol.PipSize,
                                    1e-9);

                            double tp1Pips =
                                Math.Abs(
                                    _plan.Tp1 -
                                    position.EntryPrice) /
                                Math.Max(
                                    Symbol.PipSize,
                                    1e-9);

                            double spreadPips =
                                Math.Max(
                                    0,
                                    (Symbol.Ask - Symbol.Bid) /
                                    Math.Max(
                                        Symbol.PipSize,
                                        1e-9));

                            SmartBreakEvenResult smartBreakEven =
                                SmartBreakEvenRule.Evaluate(
                                    riskPips,
                                    tp1Pips,
                                    spreadPips,
                                    BreakEvenTriggerRR,
                                    BreakEvenBufferPips,
                                    RiskFreeLockPips,
                                    UseSpreadAwareBreakEven);

                            _lastBreakEvenDiagnostic =
                                smartBreakEven.Allowed
                                    ? smartBreakEven.Reason
                                    : "NOT APPLICABLE • " +
                                      smartBreakEven.Reason;

                            double breakEvenPrice =
                                smartBreakEven.Allowed
                                    ? _plan.Direction == 1
                                        ? position.EntryPrice +
                                          smartBreakEven.OffsetPips *
                                          Symbol.PipSize
                                        : position.EntryPrice -
                                          smartBreakEven.OffsetPips *
                                          Symbol.PipSize
                                    : position.EntryPrice;

                            bool breakEvenGeometryValid =
                                IsFinitePositive(breakEvenPrice) &&
                                IsValidManagedStop(
                                    _plan.Direction,
                                    position.EntryPrice,
                                    market,
                                    breakEvenPrice);

                            if (breakEvenGeometryValid)
                            {
                                ManagementCommandRequestStatus breakEvenStatus =
                                    TryModifyStopLoss(
                                        position,
                                        NormalizePrice(breakEvenPrice),
                                        "PARTIAL BREAK-EVEN");

                                if (breakEvenStatus.IsBrokerConfirmed())
                                {
                                    // Broker mutation has succeeded; only now may the
                                    // in-memory plan adopt the broker-confirmed protection.
                                    ApplyBrokerConfirmedProtectionState(
                                        position.Id,
                                        position.EntryPrice,
                                        position.StopLoss,
                                        position.TakeProfit,
                                        true);

                                    _brokerProtectionRecoveryRequired =
                                        false;
                                    _lastBreakEvenDiagnostic =
                                        "APPLIED • " +
                                        NormalizePrice(
                                            breakEvenPrice).ToString(
                                                "R",
                                                System.Globalization.CultureInfo.InvariantCulture);
                                }
                                else
                                {
                                    // Keep the last known plan stop. A rejected mutation
                                    // must never manufacture a new protective price.
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

            // The broker owns confirmation of the position mutation; no synthetic
            // post-close state is created from the requested volume.
        }
        
        private int GetPartialTakeProfitLastAttemptM5(
            string tag)
        {
            return string.Equals(
                       tag,
                       "TP2",
                       StringComparison.OrdinalIgnoreCase)
                ? _lastPartialTp2AttemptM5
                : _lastPartialTp1AttemptM5;
        }

        private string GetPartialTakeProfitLastAttemptStage(
            string tag)
        {
            return string.Equals(
                       tag,
                       "TP2",
                       StringComparison.OrdinalIgnoreCase)
                ? "TP2"
                : "TP1";
        }

        private void RecordPartialTakeProfitAttempt(
            int closedM5,
            string tag)
        {
            if (string.Equals(
                    tag,
                    "TP2",
                    StringComparison.OrdinalIgnoreCase))
            {
                _lastPartialTp2AttemptM5 =
                    closedM5;
                return;
            }

            _lastPartialTp1AttemptM5 =
                closedM5;
        }
    }
}
