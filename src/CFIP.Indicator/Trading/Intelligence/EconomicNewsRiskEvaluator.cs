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
            if (_economicNewsEvents.Count == 0)
                return null;

            DateTimeOffset now =
                utc.Kind == DateTimeKind.Utc
                    ? new DateTimeOffset(
                        utc,
                        TimeSpan.Zero)
                    : new DateTimeOffset(
                        utc.ToUniversalTime(),
                        TimeSpan.Zero);

            CfipEconomicNewsEvent best = null;
            double bestDistance = double.MaxValue;

            for (int i = 0;
                 i < _economicNewsEvents.Count;
                 i++)
            {
                CfipEconomicNewsEvent item =
                    _economicNewsEvents[i];

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

            DateTimeOffset now =
                utc.Kind == DateTimeKind.Utc
                    ? new DateTimeOffset(
                        utc,
                        TimeSpan.Zero)
                    : new DateTimeOffset(
                        utc.ToUniversalTime(),
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

            if (_economicNewsBlockingEvent != null)
            {
                string detail =
                    FormatNewsRiskReason(
                        _economicNewsBlockingEvent,
                        TimeInUtc);

                return detail;
            }

            if (!_economicNewsFetchHealthy &&
                EnableEconomicNewsCalendar)
            {
                return
                    _economicNewsStatus +
                    (string.IsNullOrWhiteSpace(
                        _economicNewsLastError)
                        ? ""
                        : " • " +
                          _economicNewsLastError);
            }

            return _economicNewsStatus;
        }

        private bool NewsBlocked(
            DateTime utc,
            out string reason)
        {
            reason = "";

            _economicNewsBlockingEvent = null;

            // Existing manual UTC blackout remains a deliberate manual override.
            if (TryManualNewsBlackout(
                    utc))
            {
                _economicNewsStatus =
                    "NEWS BLACKOUT • MANUAL";
                reason = "NEWS";
                return true;
            }

            if (!EnableEconomicNewsCalendar)
                return false;

            bool healthy;

            if (_economicNewsLastAttemptUtc ==
                DateTime.MinValue)
            {
                // Network refresh is owned by startup/Timer. Do not block a
                // decision tick on an external feed request.
                healthy =
                    _economicNewsFetchHealthy;
            }
            else
            {
                healthy =
                    _economicNewsFetchHealthy;
            }

            if (!healthy &&
                NewsFailClosedWhenStale &&
                AutoTradingEnabled)
            {
                bool stale =
                    _economicNewsLastSuccessUtc ==
                        DateTime.MinValue ||
                    (utc -
                     _economicNewsLastSuccessUtc)
                    .TotalMinutes >
                    Math.Max(
                        15,
                        MaximumNewsFeedAgeMinutes);

                if (stale)
                {
                    _economicNewsStatus =
                        "NEWS CALENDAR • STALE • AUTO BLOCK";
                    reason = "NEWS";
                    return true;
                }
            }

            CfipEconomicNewsEvent blocking =
                FindBlockingNewsEvent(
                    utc);

            if (blocking == null)
                return false;

            _economicNewsBlockingEvent =
                blocking;

            _economicNewsStatus =
                FormatNewsRiskReason(
                    blocking,
                    utc);

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
