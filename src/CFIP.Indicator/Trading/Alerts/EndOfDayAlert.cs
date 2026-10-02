// ============================================================================
// CFIP Indicator — EndOfDayAlert.cs
// ============================================================================

using System;
using System.Collections.Generic;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void CheckEndOfDayAlert(
            DateTime nowUtc)
        {
            DateTime boundaryUtc;

            if (!SessionWindowRule.TryResolveEndOfDayBoundary(
                    nowUtc,
                    SessionStartUtc,
                    SessionEndUtc,
                    SessionWindowRule.DefaultEndOfDayCloseWindowMinutes,
                    out boundaryUtc))
                return;

            if (EnableEndOfDayAlert &&
                SessionWindowRule.IsWithinPreBoundaryWindow(
                    nowUtc,
                    boundaryUtc,
                    ExecutionThresholdPolicy.NormalizeEndOfDayAlertMinutesBefore(
                        EndOfDayAlertMinutesBefore)) &&
                !CanonicalTimeRule.IsSameUtcDay(
                    _lastEndOfDayAlertDate,
                    boundaryUtc) &&
                HasManagedOpenPosition())
            {
                _lastEndOfDayAlertDate =
                    boundaryUtc.Date;

                int minutesLeft =
                    Math.Max(
                        0,
                        (int)Math.Round(
                            (boundaryUtc -
                             nowUtc).TotalMinutes));

                SendUnifiedAlert(
                    "DAYEND|" +
                    boundaryUtc.ToString(
                        "yyyyMMddHHmm",
                        System.Globalization.CultureInfo.InvariantCulture),
                    "Day-trading session closes in ~" +
                    minutesLeft +
                    " min (" +
                    boundaryUtc.ToString(
                        "HH:mm",
                        System.Globalization.CultureInfo.InvariantCulture) +
                    " UTC) - " +
                    (EnableEndOfDayAutoClose
                        ? "managed positions/orders are scheduled for bounded session-end cleanup."
                        : "review/close managed positions and orders manually."),
                    0,
                    true);
            }

            if (!EnableEndOfDayAutoClose ||
                !SessionWindowRule.IsWithinPostBoundaryWindow(
                    nowUtc,
                    boundaryUtc,
                    SessionWindowRule.DefaultEndOfDayCloseWindowMinutes) ||
                CanonicalTimeRule.IsSameUtcDay(
                    _lastEndOfDayCloseDate,
                    boundaryUtc))
                return;

            List<Position> positionsToClose =
                new List<Position>();

            foreach (Position position in Positions)
            {
                if (position == null ||
                    !IsManagedPosition(position))
                    continue;

                DateTime entryUtc =
                    CanonicalTimeRule.EnsureUtc(
                        position.EntryTime);

                // A position created after the session boundary belongs to the
                // new session and must not be immediately closed by the
                // previous session's EOD cleanup.
                if (entryUtc < boundaryUtc)
                    positionsToClose.Add(position);
            }

            for (int i = 0;
                 i < positionsToClose.Count;
                 i++)
            {
                Position position =
                    positionsToClose[i];

                TryClosePosition(
                    position,
                    "END OF SESSION");
            }

            List<PendingOrder> pendingOrdersToCancel =
                new List<PendingOrder>();

            foreach (PendingOrder order in PendingOrders)
            {
                if (order != null &&
                    IsManagedPendingOrder(order))
                    pendingOrdersToCancel.Add(order);
            }

            for (int i = 0;
                 i < pendingOrdersToCancel.Count;
                 i++)
            {
                TryCancelPendingOrder(
                    pendingOrdersToCancel[i],
                    "END OF SESSION");
            }

            // Never latch EOD completion from a successful mutation request
            // alone. Re-read broker-owned state so a delayed/rejected close or
            // cancellation can be retried during the bounded cleanup window.
            SynchronizeLiveBrokerState();

            bool preBoundaryPositionRemains = false;

            foreach (Position position in Positions)
            {
                if (position == null ||
                    !IsManagedPosition(position))
                    continue;

                DateTime entryUtc =
                    position.EntryTime.Kind == DateTimeKind.Utc
                        ? position.EntryTime
                        : position.EntryTime.ToUniversalTime();

                if (entryUtc < boundaryUtc)
                {
                    preBoundaryPositionRemains = true;
                    break;
                }
            }

            bool managedPendingRemains = false;

            foreach (PendingOrder order in PendingOrders)
            {
                if (order != null &&
                    IsManagedPendingOrder(order))
                {
                    managedPendingRemains = true;
                    break;
                }
            }

            if (preBoundaryPositionRemains ||
                managedPendingRemains)
                return;

            _lastEndOfDayCloseDate =
                boundaryUtc.Date;

            SendUnifiedAlert(
                "DAYEND-CLOSED|" +
                boundaryUtc.ToString(
                    "yyyyMMddHHmm",
                    System.Globalization.CultureInfo.InvariantCulture),
                "CFIP session-end cleanup confirmed by broker state (" +
                boundaryUtc.ToString(
                    "HH:mm",
                    System.Globalization.CultureInfo.InvariantCulture) +
                " UTC).",
                0,
                true);
        }
    }
}
