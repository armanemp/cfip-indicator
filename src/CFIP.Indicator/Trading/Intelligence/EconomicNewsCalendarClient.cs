using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
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

    public sealed class CfipEconomicCalendarEventJson
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("country")]
        public string Currency { get; set; }
        [JsonPropertyName("date")]
        public string UtcTimestamp { get; set; }
        [JsonPropertyName("impact")]
        public string Impact { get; set; }
        [JsonPropertyName("previous")]
        public string Previous { get; set; }
        [JsonPropertyName("forecast")]
        public string Forecast { get; set; }
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
        private const int EconomicNewsProviderMinimumRefreshMinutes = 5;
        private const string CanonicalEconomicNewsJsonUri =
            "https://nfs.faireconomy.media/ff_calendar_thisweek.json";
        private const string EconomicNewsUserAgent =
            "CFIP-Indicator/1.0 (cTrader Algo)";

        private static readonly object EconomicNewsSharedSync =
            new object();

        private static string _economicNewsSharedUri =
            "";
        private static string _economicNewsSharedPayload =
            "";
        private static DateTime _economicNewsSharedLastAttemptUtc =
            DateTime.MinValue;
        private static DateTime _economicNewsSharedRequestStartedUtc =
            DateTime.MinValue;
        private static DateTime _economicNewsSharedLastSuccessUtc =
            DateTime.MinValue;
        private static string _economicNewsSharedLastError =
            "";
        private static int _economicNewsSharedGeneration;
        private static volatile bool _economicNewsSharedRequestInFlight;

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

        private string NormalizeEconomicNewsDataUri(
            string configuredUri)
        {
            string uri =
                string.IsNullOrWhiteSpace(configuredUri)
                    ? CanonicalEconomicNewsJsonUri
                    : configuredUri.Trim();

            if (string.Equals(
                    uri,
                    "https://nfs.faireconomy.media/ff_calendar_thisweek.xml",
                    StringComparison.OrdinalIgnoreCase))
                return CanonicalEconomicNewsJsonUri;

            return uri;
        }

        private int GetEffectiveNewsRefreshMinutes()
        {
            return Math.Max(
                EconomicNewsProviderMinimumRefreshMinutes,
                Math.Max(1, NewsRefreshMinutes));
        }

        private bool TryAdoptSharedEconomicNewsSnapshot(
            string uri,
            string[] relevantCurrencies)
        {
            string payload;
            DateTime successUtc;

            lock (EconomicNewsSharedSync)
            {
                if (!string.Equals(
                        _economicNewsSharedUri,
                        uri,
                        StringComparison.OrdinalIgnoreCase) ||
                    _economicNewsSharedLastSuccessUtc ==
                        DateTime.MinValue ||
                    _economicNewsSharedLastSuccessUtc <=
                        _economicNewsLastSuccessUtc)
                    return false;

                payload = _economicNewsSharedPayload;
                successUtc = _economicNewsSharedLastSuccessUtc;
            }

            List<CfipEconomicNewsEvent> next =
                DeserializeEconomicNews(
                    payload,
                    relevantCurrencies);

            lock (_economicNewsSync)
            {
                if (_economicNewsDisposed ||
                    successUtc <= _economicNewsLastSuccessUtc)
                    return false;

                _economicNewsEvents =
                    next.ToArray();
                _economicNewsLastSuccessUtc =
                    successUtc;
                _economicNewsLastFailureUtc =
                    DateTime.MinValue;
                _economicNewsLastError = "";
                _economicNewsFetchHealthy = true;
                _economicNewsLastAttemptUtc =
                    successUtc;
                UpdateEconomicNewsStatusUnsafe(
                    successUtc);
            }

            return true;
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
                NormalizeEconomicNewsDataUri(
                    EconomicNewsDataUri);

            string[] relevantCurrencies =
                InferNewsCurrencies();

            if (TryAdoptSharedEconomicNewsSnapshot(
                    uri,
                    relevantCurrencies))
            {
                UpdateEconomicNewsStatus(
                    normalizedUtc,
                    false);
                return false;
            }

            if (string.IsNullOrWhiteSpace(uri))
            {
                lock (_economicNewsSync)
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
                }
                return false;
            }

            int refreshMinutes =
                GetEffectiveNewsRefreshMinutes();

            int requestId =
                0;
            bool shouldStartRequest =
                false;

            lock (EconomicNewsSharedSync)
            {
                if (!string.Equals(
                        _economicNewsSharedUri,
                        uri,
                        StringComparison.OrdinalIgnoreCase))
                {
                    if (_economicNewsSharedRequestInFlight)
                        return false;

                    _economicNewsSharedUri = uri;
                    _economicNewsSharedPayload = "";
                    _economicNewsSharedLastAttemptUtc =
                        DateTime.MinValue;
                    _economicNewsSharedLastSuccessUtc =
                        DateTime.MinValue;
                    _economicNewsSharedLastError = "";
                }

                if (_economicNewsSharedRequestInFlight)
                {
                    if ((normalizedUtc -
                         _economicNewsSharedRequestStartedUtc).TotalSeconds <
                        EconomicNewsRequestTimeoutSeconds)
                        return false;

                    _economicNewsSharedRequestInFlight =
                        false;
                    _economicNewsSharedGeneration++;
                    _economicNewsSharedLastError =
                        "NEWS FEED REQUEST TIMEOUT";
                    _economicNewsSharedLastAttemptUtc =
                        normalizedUtc;
                }

                if (_economicNewsSharedLastAttemptUtc !=
                        DateTime.MinValue &&
                    (normalizedUtc -
                     _economicNewsSharedLastAttemptUtc).TotalMinutes <
                    refreshMinutes)
                    return false;

                _economicNewsSharedLastAttemptUtc =
                    normalizedUtc;
                _economicNewsSharedRequestStartedUtc =
                    normalizedUtc;
                _economicNewsSharedRequestInFlight =
                    true;
                _economicNewsSharedGeneration++;
                requestId =
                    _economicNewsSharedGeneration;
                shouldStartRequest = true;
            }

            if (!shouldStartRequest)
            {
                UpdateEconomicNewsStatus(
                    normalizedUtc,
                    false);
                return false;
            }

            try
            {
                var request =
                    new HttpRequest(
                        new Uri(uri));

                request.Headers.Add(
                    "Accept",
                    "application/json, text/plain, */*");
                request.Headers.Add(
                    "User-Agent",
                    EconomicNewsUserAgent);
                request.Timeout =
                    TimeSpan.FromSeconds(
                        EconomicNewsRequestTimeoutSeconds);

                Http.SendAsync(
                    request,
                    response =>
                        CompleteEconomicNewsRequest(
                            requestId,
                            normalizedUtc,
                            uri,
                            relevantCurrencies,
                            response));

                lock (_economicNewsSync)
                {
                    _economicNewsLastAttemptUtc =
                        normalizedUtc;
                    _economicNewsRequestInFlight =
                        true;
                    _economicNewsRequestStartedUtc =
                        normalizedUtc;
                    UpdateEconomicNewsStatusUnsafe(
                        normalizedUtc);
                }

                return true;
            }
            catch (Exception ex)
            {
                CompleteEconomicNewsRequest(
                    requestId,
                    normalizedUtc,
                    uri,
                    relevantCurrencies,
                    null,
                    ex.Message);

                return false;
            }
        }

        private void CompleteEconomicNewsRequest(
            int requestId,
            DateTime requestUtc,
            string uri,
            string[] relevantCurrencies,
            HttpResponse response,
            string transportError = null)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(
                        transportError))
                    throw new InvalidOperationException(
                        transportError);

                if (response == null)
                    throw new InvalidOperationException(
                        "NEWS FEED HTTP FAILURE | response=null");

                if (!response.IsSuccessful)
                {
                    string httpException =
                        response.Exception == null
                            ? ""
                            : response.Exception.Message ?? "";

                    throw new InvalidOperationException(
                        "NEWS FEED HTTP FAILURE | status=" +
                        response.StatusCode +
                        " | exception=" +
                        httpException);
                }

                string payload =
                    response.Body ?? "";

                if (string.IsNullOrWhiteSpace(payload))
                    throw new InvalidOperationException(
                        "NEWS FEED BODY EMPTY");

                ValidateEconomicNewsPayload(
                    payload);

                List<CfipEconomicNewsEvent> next =
                    DeserializeEconomicNews(
                        payload,
                        relevantCurrencies);

                lock (EconomicNewsSharedSync)
                {
                    if (requestId !=
                            _economicNewsSharedGeneration ||
                        !string.Equals(
                            _economicNewsSharedUri,
                            uri,
                            StringComparison.OrdinalIgnoreCase))
                        return;

                    _economicNewsSharedPayload =
                        payload;
                    _economicNewsSharedLastSuccessUtc =
                        requestUtc;
                    _economicNewsSharedLastError = "";
                    _economicNewsSharedRequestInFlight =
                        false;
                    _economicNewsSharedRequestStartedUtc =
                        DateTime.MinValue;
                }

                lock (_economicNewsSync)
                {
                    if (_economicNewsDisposed)
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
                lock (EconomicNewsSharedSync)
                {
                    if (requestId ==
                            _economicNewsSharedGeneration &&
                        string.Equals(
                            _economicNewsSharedUri,
                            uri,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        _economicNewsSharedLastError =
                            ex.Message ??
                            "UNKNOWN FEED ERROR";
                        _economicNewsSharedRequestInFlight =
                            false;
                        _economicNewsSharedRequestStartedUtc =
                            DateTime.MinValue;
                    }
                }

                lock (_economicNewsSync)
                {
                    if (_economicNewsDisposed)
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
            string payload,
            string[] relevantCurrencies)
        {
            List<CfipEconomicNewsEvent> next = new List<CfipEconomicNewsEvent>();
            if (string.IsNullOrWhiteSpace(payload))
                return next;

            string trimmed = payload.TrimStart();
            if (trimmed.StartsWith("[", StringComparison.Ordinal))
            {
                CfipEconomicCalendarEventJson[] parsed =
                    JsonSerializer.Deserialize<CfipEconomicCalendarEventJson[]>(
                        payload,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (parsed == null)
                    return next;

                for (int i = 0; i < parsed.Length; i++)
                {
                    CfipEconomicCalendarEventJson raw = parsed[i];
                    if (raw == null)
                        continue;

                    DateTimeOffset eventTime;
                    if (!DateTimeOffset.TryParse(
                            raw.UtcTimestamp,
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                            out eventTime))
                        continue;

                    AddEconomicNewsEvent(next, raw.Title, raw.Currency, raw.UtcTimestamp, "",
                        raw.Impact, raw.Previous, raw.Forecast, eventTime, relevantCurrencies);
                }
            }
            else
            {
                XmlSerializer serializer = new XmlSerializer(typeof(CfipEconomicCalendar));
                CfipEconomicCalendar parsed = serializer.Deserialize(new StringReader(payload))
                    as CfipEconomicCalendar;

                if (parsed == null || parsed.Events == null)
                    return next;

                for (int i = 0; i < parsed.Events.Count; i++)
                {
                    CfipEconomicCalendarEventXml raw = parsed.Events[i];
                    if (raw == null)
                        continue;

                    DateTimeOffset eventTime;
                    if (!TryParseEconomicEventTime(raw.UtcDate, raw.UtcTime, out eventTime))
                        continue;

                    AddEconomicNewsEvent(next, raw.Title, raw.Currency, raw.UtcDate, raw.UtcTime,
                        raw.Impact, raw.Previous, raw.Forecast, eventTime, relevantCurrencies);
                }
            }

            return next.OrderBy(x => x.TimeUtc).ThenByDescending(x => x.ImpactRank).ToList();
        }

        private void AddEconomicNewsEvent(
            List<CfipEconomicNewsEvent> target,
            string title,
            string currency,
            string utcDate,
            string utcTime,
            string impact,
            string previous,
            string forecast,
            DateTimeOffset eventTime,
            string[] relevantCurrencies)
        {
            CfipEconomicNewsEvent item = new CfipEconomicNewsEvent
            {
                Title = title ?? "",
                Currency = currency ?? "",
                UtcDate = utcDate ?? "",
                UtcTime = utcTime ?? "",
                Impact = impact ?? "",
                Previous = previous ?? "",
                Forecast = forecast ?? "",
                TimeUtc = eventTime
            };

            if (item.ImpactRank <= 0 || !IsNewsEventRelevant(item, relevantCurrencies))
                return;

            target.Add(item);
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
                _economicNewsRequestInFlight ||
                _economicNewsSharedRequestInFlight
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
            {
                if (_economicNewsRequestInFlight)
                    return true;

                return _economicNewsSharedRequestInFlight;
            }
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
                _economicNewsRequestStartedUtc =
                    DateTime.MinValue;
                _economicNewsRequestGeneration++;
            }
        }

        private void DisposeEconomicNewsClient()
        {
            lock (_economicNewsSync)
            {
                _economicNewsDisposed = true;
                _economicNewsRequestInFlight = false;
                _economicNewsRequestStartedUtc =
                    DateTime.MinValue;
                _economicNewsRequestGeneration++;
            }
        }

        private CfipEconomicNewsEvent[] GetEconomicNewsEvents()
        {
            return _economicNewsEvents;
        }
    }
}
