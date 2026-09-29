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

        private string _lastEconomicNewsRiskLogKey =
            "";
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

                var response =
                    Http.Get(
                        EconomicNewsDataUri);

                if (response == null ||
                    !response.IsSuccessful)
                {
                    throw new InvalidOperationException(
                        "NEWS FEED HTTP FAILURE");
                }

                string xml =
                    response.Body ?? "";

                if (string.IsNullOrWhiteSpace(xml))
                    throw new InvalidOperationException(
                        "NEWS FEED BODY EMPTY");

                {
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


    }
}
