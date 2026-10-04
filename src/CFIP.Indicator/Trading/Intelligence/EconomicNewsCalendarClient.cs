using System;
using cAlgo.API;

namespace cAlgo
{
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
                EconomicNewsCalendarParser.NormalizeUri(
                    EconomicNewsDataUri);

            string[] relevantCurrencies =
                InferNewsCurrencies();

            DateTime sharedLastAttemptUtc;

            if (TryAdoptSharedEconomicNewsSnapshot(
                    uri,
                    relevantCurrencies,
                    normalizedUtc,
                    out sharedLastAttemptUtc))
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

            if (sharedLastAttemptUtc != DateTime.MinValue &&
                (normalizedUtc -
                 sharedLastAttemptUtc).TotalMinutes <
                    EconomicNewsFeedCoordinator.MinimumRefreshMinutes)
            {
                UpdateEconomicNewsStatus(
                    normalizedUtc,
                    false);
                return false;
            }

            int requestId;
            if (!EconomicNewsFeedCoordinator.TryStart(
                    uri,
                    normalizedUtc,
                    NewsRefreshMinutes,
                    out requestId))
            {
                UpdateEconomicNewsStatus(
                    normalizedUtc,
                    false);
                return false;
            }

            int localRequestId;
            lock (_economicNewsSync)
            {
                _economicNewsLastAttemptUtc =
                    normalizedUtc;
                _economicNewsRequestStartedUtc =
                    normalizedUtc;
                _economicNewsRequestInFlight =
                    true;
                _economicNewsRequestGeneration++;
                localRequestId =
                    _economicNewsRequestGeneration;
                UpdateEconomicNewsStatusUnsafe(
                    normalizedUtc);
            }

            PersistSharedEconomicNewsState(
                uri,
                normalizedUtc,
                _economicNewsLastSuccessUtc,
                null);

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
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/154 Safari/537.36");
                request.Timeout =
                    TimeSpan.FromSeconds(
                        EconomicNewsFeedCoordinator.TimeoutSeconds);

                Http.SendAsync(
                    request,
                    response =>
                        CompleteEconomicNewsRequest(
                            requestId,
                            localRequestId,
                            normalizedUtc,
                            uri,
                            relevantCurrencies,
                            response));

                return true;
            }
            catch (Exception ex)
            {
                CompleteEconomicNewsRequest(
                    requestId,
                    localRequestId,
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
            int localRequestId,
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

                CfipEconomicNewsEvent[] next =
                    EconomicNewsCalendarParser.Parse(
                        payload,
                        relevantCurrencies);

                if (!EconomicNewsFeedCoordinator.CompleteSuccess(
                        uri,
                        requestId,
                        requestUtc,
                        payload))
                    return;

                lock (_economicNewsSync)
                {
                    if (_economicNewsDisposed ||
                        localRequestId !=
                        _economicNewsRequestGeneration)
                        return;

                    _economicNewsEvents =
                        next;
                    _economicNewsLastSuccessUtc =
                        requestUtc;
                    _economicNewsFetchHealthy =
                        true;
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

                PersistSharedEconomicNewsState(
                    uri,
                    requestUtc,
                    requestUtc,
                    payload);

                Print(
                    "CFIP ECONOMIC NEWS FETCH SUCCESS | source=HTTP | events={0} | successUtc={1}",
                    next.Length,
                    requestUtc.ToString(
                        "O",
                        System.Globalization.CultureInfo.InvariantCulture));
            }
            catch (Exception ex)
            {
                EconomicNewsFeedCoordinator.CompleteFailure(
                    uri,
                    requestId,
                    ex.Message);

                lock (_economicNewsSync)
                {
                    if (_economicNewsDisposed ||
                        localRequestId !=
                        _economicNewsRequestGeneration)
                        return;

                    _economicNewsFetchHealthy =
                        false;
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

                PersistSharedEconomicNewsState(
                    uri,
                    requestUtc,
                    _economicNewsLastSuccessUtc,
                    null);

                Print(
                    "CFIP economic news refresh failed: {0}",
                    ex.Message);
            }
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
                EconomicNewsFeedCoordinator.RequestInFlight
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
            }

            return EconomicNewsFeedCoordinator.RequestInFlight;
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
