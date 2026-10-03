using System;
using CFIP.Contracts;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator
    {
        private void ArchiveEconomicNewsRisk(
            int m5,
            DateTime utc,
            string state,
            string action)
        {
            CfipEconomicNewsEvent item =
                _economicNewsBlockingEvent;

            string eventKey =
                item == null
                    ? "NONE"
                    : item.Currency +
                      "|" +
                      item.Title +
                      "|" +
                      item.TimeUtc.Ticks;

            string key =
                state +
                "|" +
                action +
                "|" +
                eventKey;

            if (string.Equals(
                    key,
                    _lastEconomicNewsRiskLogKey,
                    StringComparison.Ordinal))
                return;

            _lastEconomicNewsRiskLogKey = key;

            ArchiveRuntimeEvent(
                "NEWS_RISK",
                m5,
                "NEWS",
                state,
                string.IsNullOrWhiteSpace(action)
                    ? FormatNewsRiskReason(
                        item,
                        utc)
                    : action +
                      " • " +
                      FormatNewsRiskReason(
                          item,
                          utc),
                eventKey,
                item == null
                    ? ""
                    : item.Currency,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                false,
                0,
                0,
                utc);
        }

        private void ApplyEconomicNewsRiskProtection(
            int closedM5)
        {
            if (!EnableEconomicNewsCalendar)
                return;

            string reason;

            if (!NewsBlocked(
                    TimeInUtc,
                    out reason))
                return;

            CfipEconomicNewsEvent item =
                _economicNewsBlockingEvent;

            if (item == null)
            {
                ArchiveEconomicNewsRisk(
                    closedM5,
                    TimeInUtc,
                    "MANUAL/STALE",
                    _economicNewsStatus);
                return;
            }

            double minutes =
                MinutesToBlockingNews(
                    TimeInUtc);

            bool preEvent =
                minutes >= 0;

            if (item.ImpactRank >= 3 &&
                preEvent &&
                minutes <=
                Math.Max(
                    0,
                    HighImpactNewsMinutesBefore))
            {
                PendingOrder pending =
                    GetManagedPendingOrder();

                if (pending != null &&
                    CancelPendingBeforeHighImpactNews)
                {
                    ManagementCommandRequestStatus cancelStatus =
                        RequestCancelPendingOrder(
                            pending,
                            "HIGH IMPACT NEWS");
                    if (cancelStatus.IsAccepted())
                    {
                        ArchiveEconomicNewsRisk(
                            closedM5,
                            TimeInUtc,
                            "PENDING CANCELLED",
                            FormatNewsRiskReason(
                                item,
                                TimeInUtc));
                    }
                }

                Position position =
                    GetManagedPosition();

                if (position != null &&
                    CloseActiveBeforeHighImpactNews)
                {
                    ManagementCommandRequestStatus closeStatus =
                        RequestClosePosition(
                            position,
                            "HIGH IMPACT NEWS PRE-PROTECTION");
                    if (closeStatus.IsAccepted())
                    {
                        SynchronizeLiveBrokerState();

                        ArchiveEconomicNewsRisk(
                            closedM5,
                            TimeInUtc,
                            "POSITION CLOSED",
                            FormatNewsRiskReason(
                                item,
                                TimeInUtc));
                    }
                }
            }
            else
            {
                ArchiveEconomicNewsRisk(
                    closedM5,
                    TimeInUtc,
                    preEvent
                        ? "PRE-EVENT BLOCK"
                        : "POST-EVENT BLOCK",
                    FormatNewsRiskReason(
                        item,
                        TimeInUtc));
            }
        }

        private bool IsBlockingHighImpactNews(
            DateTime utc)
        {
            return
                _economicNewsBlockingEvent != null &&
                _economicNewsBlockingEvent.ImpactRank >= 3;
        }

        private double MinutesToBlockingNews(
            DateTime utc)
        {
            if (_economicNewsBlockingEvent == null)
                return double.MaxValue;

            return
                (_economicNewsBlockingEvent.TimeUtc -
                 new DateTimeOffset(
                    CanonicalTimeRule.EnsureUtc(
                        utc),
                    TimeSpan.Zero))
                .TotalMinutes;
        }
    }
}
