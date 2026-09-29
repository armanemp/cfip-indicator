using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Serialization;

namespace cAlgo
{
    internal sealed class CfipEconomicNewsEvent
    {
        public string Title { get; set; }
        public string Currency { get; set; }
        public string UtcDate { get; set; }
        public string UtcTime { get; set; }
        public string Impact { get; set; }
        public string Previous { get; set; }
        public string Forecast { get; set; }
        public DateTimeOffset TimeUtc { get; set; }

        public int ImpactRank
        {
            get
            {
                string impact =
                    string.IsNullOrWhiteSpace(Impact)
                        ? ""
                        : Impact.Trim().ToUpperInvariant();

                if (impact.Contains("HIGH"))
                    return 3;

                if (impact.Contains("MED"))
                    return 2;

                if (impact.Contains("LOW"))
                    return 1;

                return 0;
            }
        }
    }

    [XmlRoot("weeklyevents")]
    public sealed class CfipEconomicCalendar
    {
        [XmlElement("event")]
        public List<CfipEconomicCalendarEventXml> Events { get; set; }
    }

    public sealed class CfipEconomicCalendarEventXml
    {
        [XmlElement("title")]
        public string Title { get; set; }

        [XmlElement("country")]
        public string Currency { get; set; }

        [XmlElement("date")]
        public string UtcDate { get; set; }

        [XmlElement("time")]
        public string UtcTime { get; set; }

        [XmlElement("impact")]
        public string Impact { get; set; }

        [XmlElement("previous")]
        public string Previous { get; set; }

        [XmlElement("forecast")]
        public string Forecast { get; set; }
    }

    public partial class CFIPIndicator
    {
        private readonly List<CfipEconomicNewsEvent> _economicNewsEvents =
            new List<CfipEconomicNewsEvent>();

        private DateTime _economicNewsLastAttemptUtc =
            DateTime.MinValue;

        private DateTime _economicNewsLastSuccessUtc =
            DateTime.MinValue;

        private string _economicNewsLastError =
            "";

        private CfipEconomicNewsEvent _economicNewsBlockingEvent;

        private string _economicNewsStatus =
            "NEWS CALENDAR • NOT LOADED";

        private bool _economicNewsFetchHealthy;

        private string[] InferNewsCurrencies()
        {
            HashSet<string> result =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

            string symbol =
                string.IsNullOrWhiteSpace(SymbolName)
                    ? ""
                    : SymbolName.ToUpperInvariant();

            string[] known =
            {
                "USD", "EUR", "GBP", "JPY",
                "AUD", "CAD", "NZD", "CHF",
                "CNY", "CNH", "HKD", "SGD",
                "NOK", "SEK", "ZAR"
            };

            for (int i = 0; i < known.Length; i++)
            {
                if (symbol.Contains(known[i]))
                    result.Add(known[i]);
            }

            if (!string.IsNullOrWhiteSpace(
                    AdditionalNewsCurrencies))
            {
                string[] extras =
                    AdditionalNewsCurrencies.Split(
                        new[] { ',', ';', ' ', '|' },
                        StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < extras.Length; i++)
                {
                    string value =
                        extras[i].Trim().ToUpperInvariant();

                    if (value.Length >= 3)
                        result.Add(value);
                }
            }

            return result.ToArray();
        }

        private bool IsNewsEventRelevant(
            CfipEconomicNewsEvent newsEvent)
        {
            if (newsEvent == null ||
                string.IsNullOrWhiteSpace(
                    newsEvent.Currency))
                return false;

            string currency =
                newsEvent.Currency.Trim().ToUpperInvariant();

            string[] currencies =
                InferNewsCurrencies();

            for (int i = 0; i < currencies.Length; i++)
            {
                if (currency ==
                    currencies[i])
                    return true;
            }

            return false;
        }

        private bool TryParseEconomicEventTime(
            string date,
            string time,
            out DateTimeOffset value)
        {
            value = default(DateTimeOffset);

            if (string.IsNullOrWhiteSpace(date) ||
                string.IsNullOrWhiteSpace(time))
                return false;

            string combined =
                date.Trim() +
                " " +
                time.Trim();

            string[] formats =
            {
                "MM-dd-yyyy h:mmtt",
                "MM-dd-yyyy hh:mmtt",
                "MM/dd/yyyy h:mmtt",
                "MM/dd/yyyy hh:mmtt",
                "yyyy-MM-dd HH:mm",
                "yyyy-MM-ddTHH:mm:ss",
                "yyyy-MM-ddTHH:mm:ssZ"
            };

            for (int i = 0;
                 i < formats.Length;
                 i++)
            {
                if (DateTimeOffset.TryParseExact(
                        combined,
                        formats[i],
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal |
                        DateTimeStyles.AdjustToUniversal,
                        out value))
                    return true;
            }

            return DateTimeOffset.TryParse(
                combined,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal |
                DateTimeStyles.AdjustToUniversal,
                out value);
        }

        private bool RefreshEconomicNewsIfNeeded(
            DateTime utc)
        {
            if (!EnableEconomicNewsCalendar)
                return false;

            DateTime normalizedUtc =
                utc.Kind == DateTimeKind.Utc
                    ? utc
                    : utc.ToUniversalTime();

            if (_economicNewsLastAttemptUtc != DateTime.MinValue &&
                (normalizedUtc -
                 _economicNewsLastAttemptUtc).TotalMinutes <
                Math.Max(
                    1,
                    NewsRefreshMinutes))
            {
                return _economicNewsFetchHealthy;
            }

            _economicNewsLastAttemptUtc =
                normalizedUtc;

            try
            {
                if (string.IsNullOrWhiteSpace(
                        EconomicNewsDataUri))
                {
                    throw new InvalidOperationException(
                        "NEWS FEED URI EMPTY");
                }

                HttpWebRequest request =
                    WebRequest.CreateHttp(
                        EconomicNewsDataUri);

                request.Method = "GET";
                request.Timeout = 6000;
                request.ReadWriteTimeout = 6000;
                request.UserAgent =
                    "CFIPIndicator/1.0";

                using (WebResponse response =
                    request.GetResponse())
                using (Stream stream =
                    response.GetResponseStream())
                using (StreamReader reader =
                    new StreamReader(
                        stream ?? Stream.Null,
                        Encoding.UTF8))
                {
                    string xml =
                        reader.ReadToEnd();

                    XmlSerializer serializer =
                        new XmlSerializer(
                            typeof(CfipEconomicCalendar));

                    CfipEconomicCalendar parsed =
                        serializer.Deserialize(
                            new StringReader(xml))
                        as CfipEconomicCalendar;

                    List<CfipEconomicNewsEvent> next =
                        new List<CfipEconomicNewsEvent>();

                    if (parsed != null &&
                        parsed.Events != null)
                    {
                        for (int i = 0;
                             i < parsed.Events.Count;
                             i++)
                        {
                            CfipEconomicCalendarEventXml raw =
                                parsed.Events[i];

                            DateTimeOffset eventTime;

                            if (!TryParseEconomicEventTime(
                                    raw.UtcDate,
                                    raw.UtcTime,
                                    out eventTime))
                                continue;

                            CfipEconomicNewsEvent item =
                                new CfipEconomicNewsEvent
                                {
                                    Title = raw.Title ?? "",
                                    Currency = raw.Currency ?? "",
                                    UtcDate = raw.UtcDate ?? "",
                                    UtcTime = raw.UtcTime ?? "",
                                    Impact = raw.Impact ?? "",
                                    Previous = raw.Previous ?? "",
                                    Forecast = raw.Forecast ?? "",
                                    TimeUtc = eventTime
                                };

                            if (item.ImpactRank <= 0)
                                continue;

                            if (!IsNewsEventRelevant(item))
                                continue;

                            next.Add(item);
                        }
                    }

                    _economicNewsEvents.Clear();

                    _economicNewsEvents.AddRange(
                        next
                            .OrderBy(
                                x => x.TimeUtc)
                            .ThenByDescending(
                                x => x.ImpactRank));

                    _economicNewsLastSuccessUtc =
                        normalizedUtc;

                    _economicNewsFetchHealthy = true;
                    _economicNewsLastError = "";
                    _economicNewsStatus =
                        "NEWS CALENDAR • OK • " +
                        _economicNewsEvents.Count +
                        " RELEVANT EVENTS";

                    return true;
                }
            }
            catch (Exception ex)
            {
                _economicNewsFetchHealthy = false;
                _economicNewsLastError =
                    ex.Message ?? "UNKNOWN FEED ERROR";

                _economicNewsStatus =
                    "NEWS CALENDAR • FEED ERROR";

                return false;
            }
        }

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

            bool healthy =
                RefreshEconomicNewsIfNeeded(
                    utc);

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
                    utc.Kind == DateTimeKind.Utc
                        ? utc
                        : utc.ToUniversalTime(),
                    TimeSpan.Zero))
                .TotalMinutes;
        }
    }
}
