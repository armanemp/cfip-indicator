using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using cAlgo.API;

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
        private const int EconomicNewsRequestTimeoutSeconds = 30;

        private readonly object _economicNewsSync =
            new object();

        // Immutable-after-publication cache. Refreshes build a new array and
        // atomically publish the reference; readers never allocate or enumerate
        // a collection that can be mutated underneath them.
        private CfipEconomicNewsEvent[] _economicNewsEvents =
            new CfipEconomicNewsEvent[0];

        private DateTime _economicNewsLastAttemptUtc =
            DateTime.MinValue;

        private DateTime _economicNewsRequestStartedUtc =
            DateTime.MinValue;

        private DateTime _economicNewsLastSuccessUtc =
            DateTime.MinValue;

        private DateTime _economicNewsLastFailureUtc =
            DateTime.MinValue;

        private string _economicNewsLastError =
            "";

        private int _economicNewsRequestGeneration;

        private bool _economicNewsRequestInFlight;

        private bool _economicNewsDisposed;

        private CfipEconomicNewsEvent _economicNewsBlockingEvent;

        private string _economicNewsStatus =
            "NEWS CALENDAR • NEVER LOADED";

        private bool _economicNewsFetchHealthy;

        private string _lastEconomicNewsRiskLogKey =
            "";

        private string[] InferNewsCurrencies()
        {
            return EconomicNewsCurrencyRule.ResolveCurrencies(
                SymbolName,
                AdditionalNewsCurrencies,
                AdditionalNewsCurrencies);
        }

        private bool IsNewsEventRelevant(
            CfipEconomicNewsEvent newsEvent,
            string[] relevantCurrencies)
        {
            if (newsEvent == null ||
                string.IsNullOrWhiteSpace(
                    newsEvent.Currency) ||
                relevantCurrencies == null)
                return false;

            string currency =
                newsEvent.Currency.Trim().ToUpperInvariant();

            for (int i = 0;
                 i < relevantCurrencies.Length;
                 i++)
            {
                if (currency ==
                    relevantCurrencies[i])
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
            {
                UpdateEconomicNewsStatus(
                    utc,
                    false);
                return false;
            }

            DateTime normalizedUtc =
                CanonicalTimeRule.EnsureUtc(
                    utc);

            string uri =
                EconomicNewsDataUri;

            string[] relevantCurrencies =
                InferNewsCurrencies();

            int requestId;

            lock (_economicNewsSync)
            {
                if (_economicNewsRequestInFlight)
                {
                    if ((normalizedUtc -
                         _economicNewsRequestStartedUtc).TotalSeconds <
                        EconomicNewsRequestTimeoutSeconds)
                    {
                        UpdateEconomicNewsStatusUnsafe(
                            normalizedUtc);
                        return false;
                    }

                    // The old request has exceeded our bounded freshness window.
                    // Invalidate its callback and allow a fresh request. The old
                    // response may still arrive; its generation will be ignored.
                    _economicNewsRequestInFlight = false;
                    _economicNewsRequestGeneration++;
                    _economicNewsLastAttemptUtc =
                        DateTime.MinValue;
                    _economicNewsLastFailureUtc =
                        normalizedUtc;
                    _economicNewsLastError =
                        "NEWS FEED REQUEST TIMEOUT";
                }

                if (_economicNewsLastAttemptUtc !=
                        DateTime.MinValue &&
                    (normalizedUtc -
                     _economicNewsLastAttemptUtc).TotalMinutes <
                    Math.Max(
                        1,
                        NewsRefreshMinutes))
                {
                    UpdateEconomicNewsStatusUnsafe(
                        normalizedUtc);
                    return false;
                }

                if (string.IsNullOrWhiteSpace(uri))
                {
                    _economicNewsLastAttemptUtc =
                        normalizedUtc;
                    _economicNewsFetchHealthy = false;
                    _economicNewsLastFailureUtc =
                        normalizedUtc;
                    _economicNewsLastError =
                        "NEWS FEED URI EMPTY";
                    UpdateEconomicNewsStatusUnsafe(
                        normalizedUtc);
                    return false;
                }

                _economicNewsLastAttemptUtc =
                    normalizedUtc;
                _economicNewsRequestStartedUtc =
                    normalizedUtc;
                _economicNewsRequestInFlight = true;
                _economicNewsRequestGeneration++;

                requestId =
                    _economicNewsRequestGeneration;

                UpdateEconomicNewsStatusUnsafe(
                    normalizedUtc);
            }

            try
            {
                lock (_economicNewsSync)
                {
                    if (_economicNewsDisposed)
                    {
                        _economicNewsRequestInFlight = false;
                        return false;
                    }
                }

                Http.GetAsync(
                    uri,
                    response =>
                        CompleteEconomicNewsRequest(
                            requestId,
                            normalizedUtc,
                            relevantCurrencies,
                            response));

                return true;
            }
            catch (Exception ex)
            {
                CompleteEconomicNewsRequest(
                    requestId,
                    normalizedUtc,
                    relevantCurrencies,
                    null,
                    ex.Message);

                return false;
            }
        }

        private void CompleteEconomicNewsRequest(
            int requestId,
            DateTime requestUtc,
            string[] relevantCurrencies,
            HttpResponse response,
            string transportError = null)
        {
            try
            {
                lock (_economicNewsSync)
                {
                    if (_economicNewsDisposed ||
                        requestId !=
                        _economicNewsRequestGeneration)
                        return;
                }

                if (!string.IsNullOrWhiteSpace(transportError))
                    throw new InvalidOperationException(
                        transportError);

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

                List<CfipEconomicNewsEvent> next =
                    DeserializeEconomicNews(
                        xml,
                        relevantCurrencies);

                lock (_economicNewsSync)
                {
                    if (requestId !=
                        _economicNewsRequestGeneration)
                        return;

                    _economicNewsEvents =
                        next.ToArray();

                    _economicNewsLastSuccessUtc =
                        requestUtc;

                    _economicNewsFetchHealthy = true;
                    _economicNewsLastFailureUtc =
                        DateTime.MinValue;
                    _economicNewsLastError = "";
                    _economicNewsRequestInFlight =
                        false;
                    _economicNewsRequestStartedUtc =
                        DateTime.MinValue;

                    UpdateEconomicNewsStatusUnsafe(
                        requestUtc);
                }
            }
            catch (Exception ex)
            {
                lock (_economicNewsSync)
                {
                    if (requestId !=
                        _economicNewsRequestGeneration)
                        return;

                    _economicNewsFetchHealthy = false;
                    _economicNewsLastFailureUtc =
                        requestUtc;
                    _economicNewsLastError =
                        ex.Message ??
                        "UNKNOWN FEED ERROR";
                    _economicNewsRequestInFlight =
                        false;
                    _economicNewsRequestStartedUtc =
                        DateTime.MinValue;

                    UpdateEconomicNewsStatusUnsafe(
                        requestUtc);
                }

                Print(
                    "CFIP economic news refresh failed: {0}",
                    ex.Message);
            }
        }

        private List<CfipEconomicNewsEvent> DeserializeEconomicNews(
            string xml,
            string[] relevantCurrencies)
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

            if (parsed == null ||
                parsed.Events == null)
                return next;

            for (int i = 0;
                 i < parsed.Events.Count;
                 i++)
            {
                CfipEconomicCalendarEventXml raw =
                    parsed.Events[i];

                if (raw == null)
                    continue;

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

                if (item.ImpactRank <= 0 ||
                    !IsNewsEventRelevant(
                        item,
                        relevantCurrencies))
                    continue;

                next.Add(item);
            }

            return next
                .OrderBy(x => x.TimeUtc)
                .ThenByDescending(x => x.ImpactRank)
                .ToList();
        }

        private void UpdateEconomicNewsStatus(
            DateTime utc,
            bool blockingEvent)
        {
            lock (_economicNewsSync)
            {
                UpdateEconomicNewsStatusUnsafe(
                    utc,
                    blockingEvent);
            }
        }

        private void UpdateEconomicNewsStatusUnsafe(
            DateTime utc,
            bool blockingEvent = false)
        {
            EconomicNewsFeedState state =
                EconomicNewsFeedStateRule.Evaluate(
                    EnableEconomicNewsCalendar,
                    _economicNewsLastSuccessUtc,
                    utc,
                    MaximumNewsFeedAgeMinutes,
                    blockingEvent);

            string stateText =
                state == EconomicNewsFeedState.Disabled
                    ? "DISABLED"
                    : state == EconomicNewsFeedState.NeverLoaded
                        ? "NEVER LOADED"
                        : state == EconomicNewsFeedState.Healthy
                            ? "HEALTHY"
                            : state == EconomicNewsFeedState.Stale
                                ? "STALE"
                                : "BLOCKING EVENT";

            string eventCount =
                state == EconomicNewsFeedState.BlockingEvent
                    ? ""
                    : " • " +
                      _economicNewsEvents.Length +
                      " RELEVANT EVENTS";

            string refresh =
                _economicNewsRequestInFlight
                    ? " • REFRESHING"
                    : "";

            _economicNewsStatus =
                "NEWS CALENDAR • " +
                stateText +
                eventCount +
                refresh;
        }

        private bool IsEconomicNewsRequestInFlight()
        {
            lock (_economicNewsSync)
                return _economicNewsRequestInFlight;
        }

        private string GetEconomicNewsLastError()
        {
            lock (_economicNewsSync)
                return _economicNewsLastError;
        }

        private DateTime GetEconomicNewsLastSuccessUtc()
        {
            lock (_economicNewsSync)
                return _economicNewsLastSuccessUtc;
        }

        private bool IsEconomicNewsFetchHealthy()
        {
            lock (_economicNewsSync)
                return _economicNewsFetchHealthy;
        }

        private void ResetEconomicNewsClientLifecycle()
        {
            lock (_economicNewsSync)
            {
                _economicNewsDisposed = false;
                _economicNewsRequestInFlight = false;
                _economicNewsRequestGeneration++;
            }
        }

        private void DisposeEconomicNewsClient()
        {
            lock (_economicNewsSync)
            {
                _economicNewsDisposed = true;
                _economicNewsRequestInFlight = false;
                _economicNewsRequestGeneration++;
            }
        }

        private CfipEconomicNewsEvent[] GetEconomicNewsEvents()
        {
            return _economicNewsEvents;
        }
    }
}
