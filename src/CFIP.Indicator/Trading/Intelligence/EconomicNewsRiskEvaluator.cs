using System;
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
        private CfipEconomicNewsEvent FindBlockingNewsEvent(
            DateTime utc)
        {
            CfipEconomicNewsEvent[] events =
                GetEconomicNewsEvents();

            if (events.Length == 0)
                return null;

            DateTimeOffset now =
                utc.Kind == DateTimeKind.Utc
                    ? new DateTimeOffset(
                        utc,
                        TimeSpan.Zero)
                    : new DateTimeOffset(
                        CanonicalTimeRule.EnsureUtc(
                            utc),
                        TimeSpan.Zero);

            CfipEconomicNewsEvent best = null;
            double bestDistance = double.MaxValue;

            for (int i = 0;
                 i < events.Length;
                 i++)
            {
                CfipEconomicNewsEvent item =
                    events[i];

                if (item == null ||
                    item.ImpactRank <= 0)
                    continue;

                int beforeMinutes =
                    item.ImpactRank >= 3
                        ? Math.Max(
                            0,
                            HighImpactNewsMinutesBefore)
                        : BlockMediumImpactNews &&
                          item.ImpactRank == 2
                            ? Math.Max(
                                0,
                                MediumImpactNewsMinutesBefore)
                            : 0;

                int afterMinutes =
                    item.ImpactRank >= 3
                        ? Math.Max(
                            0,
                            HighImpactNewsMinutesAfter)
                        : BlockMediumImpactNews &&
                          item.ImpactRank == 2
                            ? Math.Max(
                                0,
                                MediumImpactNewsMinutesAfter)
                            : 0;

                if (beforeMinutes <= 0 &&
                    afterMinutes <= 0)
                    continue;

                double deltaMinutes =
                    (item.TimeUtc - now).TotalMinutes;

                bool inside =
                    deltaMinutes <= beforeMinutes &&
                    deltaMinutes >= -afterMinutes;

                if (!inside)
                    continue;

                double distance =
                    Math.Abs(deltaMinutes);

                if (best == null ||
                    item.ImpactRank >
                    best.ImpactRank ||
                    (item.ImpactRank ==
                     best.ImpactRank &&
                     distance <
                     bestDistance))
                {
                    best = item;
                    bestDistance = distance;
                }
            }

            return best;
        }

        private string FormatNewsRiskReason(
            CfipEconomicNewsEvent newsEvent,
            DateTime utc)
        {
            if (newsEvent == null)
                return "NEWS";

            DateTime normalizedUtc =
                CanonicalTimeRule.EnsureUtc(
                    utc);

            DateTimeOffset now =
                new DateTimeOffset(
                    normalizedUtc,
                    TimeSpan.Zero);

            double deltaMinutes =
                (newsEvent.TimeUtc - now).TotalMinutes;

            string timing;

            if (deltaMinutes > 0)
                timing =
                    "IN " +
                    Math.Ceiling(deltaMinutes)
                    .ToString(
                        CultureInfo.InvariantCulture) +
                    "M";
            else if (deltaMinutes < 0)
                timing =
                    Math.Floor(
                        Math.Abs(deltaMinutes))
                    .ToString(
                        CultureInfo.InvariantCulture) +
                    "M AGO";
            else
                timing = "NOW";

            return
                "NEWS • " +
                newsEvent.Currency +
                " • " +
                (string.IsNullOrWhiteSpace(
                    newsEvent.Title)
                    ? "ECONOMIC EVENT"
                    : newsEvent.Title) +
                " • " +
                timing;
        }

        private string NewsRiskPanelLine()
        {
            if (!ShowNewsRiskStatus)
                return "";

            DateTime now =
                CanonicalTimeRule.EnsureUtc(
                    TimeInUtc);

            EconomicNewsFeedState state =
                EconomicNewsFeedStateRule.Evaluate(
                    EnableEconomicNewsCalendar,
                    GetEconomicNewsLastSuccessUtc(),
                    now,
                    MaximumNewsFeedAgeMinutes,
                    false);

            CfipEconomicNewsEvent currentBlocking =
                FindBlockingNewsEvent(
                    now);

            if (currentBlocking != null)
                _economicNewsBlockingEvent =
                    currentBlocking;
            else
                _economicNewsBlockingEvent = null;

            state =
                EconomicNewsFeedStateRule.Evaluate(
                    EnableEconomicNewsCalendar,
                    GetEconomicNewsLastSuccessUtc(),
                    now,
                    MaximumNewsFeedAgeMinutes,
                    _economicNewsBlockingEvent != null);

            if (state ==
                EconomicNewsFeedState.BlockingEvent &&
                _economicNewsBlockingEvent != null)
            {
                return
                    "NEWS CALENDAR • BLOCKING EVENT • " +
                    FormatNewsRiskReason(
                        _economicNewsBlockingEvent,
                        now);
            }

            string status =
                _economicNewsStatus;

            if (state ==
                EconomicNewsFeedState.Disabled)
            {
                status =
                    "NEWS CALENDAR • DISABLED";
            }
            else if (state ==
                     EconomicNewsFeedState.NeverLoaded)
            {
                status =
                    "NEWS CALENDAR • NEVER LOADED";
            }
            else if (state ==
                     EconomicNewsFeedState.Stale)
            {
                status =
                    "NEWS CALENDAR • STALE";
            }
            else if (state ==
                     EconomicNewsFeedState.Healthy)
            {
                status =
                    "NEWS CALENDAR • HEALTHY • " +
                    GetEconomicNewsEvents().Length +
                    " RELEVANT EVENTS";
            }

            if (IsEconomicNewsRequestInFlight())
                status += " • REFRESHING";

            string error =
                GetEconomicNewsLastError();

            if (!string.IsNullOrWhiteSpace(error))
                status +=
                    " • LAST REFRESH FAILED: " +
                    TruncateNewsError(error);

            return status;
        }

        private string TruncateNewsError(
            string error)
        {
            if (string.IsNullOrWhiteSpace(error))
                return "";

            const int maximumLength = 80;

            string normalized =
                error
                    .Replace(
                        "\r",
                        " ")
                    .Replace(
                        "\n",
                        " ")
                    .Trim();

            if (normalized.Length <= maximumLength)
                return normalized;

            return normalized.Substring(
                       0,
                       maximumLength) +
                   "...";
        }

        private bool NewsBlocked(
            DateTime utc,
            out string reason)
        {
            reason = "";
            _economicNewsBlockingEvent = null;

            DateTime normalizedUtc =
                CanonicalTimeRule.EnsureUtc(
                    utc);

            // Existing manual UTC blackout remains a deliberate manual override.
            if (TryManualNewsBlackout(
                    normalizedUtc))
            {
                _economicNewsStatus =
                    "NEWS BLACKOUT • MANUAL";
                reason = "NEWS";
                return true;
            }

            if (!EnableEconomicNewsCalendar)
            {
                UpdateEconomicNewsStatus(
                    normalizedUtc,
                    false);
                return false;
            }

            DateTime lastSuccess =
                GetEconomicNewsLastSuccessUtc();

            EconomicNewsFeedState feedState =
                EconomicNewsFeedStateRule.Evaluate(
                    true,
                    lastSuccess,
                    normalizedUtc,
                    MaximumNewsFeedAgeMinutes,
                    false);

            UpdateEconomicNewsStatus(
                normalizedUtc,
                false);

            if ((feedState ==
                 EconomicNewsFeedState.NeverLoaded ||
                 feedState ==
                 EconomicNewsFeedState.Stale) &&
                NewsFailClosedWhenStale &&
                (AutoTradingEnabled ||
                 AutomaticOrdersEnabled))
            {
                _economicNewsStatus =
                    "NEWS CALENDAR • " +
                    (feedState ==
                     EconomicNewsFeedState.NeverLoaded
                        ? "NEVER LOADED"
                        : "STALE") +
                    " • AUTO BLOCK";

                reason = "NEWS";
                return true;
            }

            CfipEconomicNewsEvent blocking =
                FindBlockingNewsEvent(
                    normalizedUtc);

            if (blocking == null)
            {
                UpdateEconomicNewsStatus(
                    normalizedUtc,
                    false);
                return false;
            }

            _economicNewsBlockingEvent =
                blocking;

            UpdateEconomicNewsStatus(
                normalizedUtc,
                true);

            reason = "NEWS";
            return true;
        }

        private bool TryManualNewsBlackout(
            DateTime utc)
        {
            if (string.IsNullOrWhiteSpace(
                    NewsBlackoutUtc))
                return false;

            string[] items =
                NewsBlackoutUtc.Split(
                    new[] { ',', ';', '|' },
                    StringSplitOptions.RemoveEmptyEntries);

            int current =
                utc.Hour * 60 +
                utc.Minute;

            for (int i = 0;
                 i < items.Length;
                 i++)
            {
                string[] parts =
                    items[i]
                    .Trim()
                    .Split('-');

                if (parts.Length != 2)
                    continue;

                if (!TryParseMinutes(
                        parts[0],
                        out int start) ||
                    !TryParseMinutes(
                        parts[1],
                        out int end))
                    continue;

                bool blocked =
                    start <= end
                        ? current >= start &&
                          current <= end
                        : current >= start ||
                          current <= end;

                if (blocked)
                    return true;
            }

            return false;
        }


    }
}
