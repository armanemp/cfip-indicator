using System;

namespace cAlgo
{
    internal static class EconomicNewsFeedCoordinator
    {
        private const int RequestTimeoutSeconds = 30;
        private const int ProviderMinimumRefreshMinutes = 60;

        private static readonly object Sync =
            new object();

        private static string _uri =
            "";
        private static string _payload =
            "";
        private static DateTime _lastAttemptUtc =
            DateTime.MinValue;
        private static DateTime _requestStartedUtc =
            DateTime.MinValue;
        private static DateTime _lastSuccessUtc =
            DateTime.MinValue;
        private static string _lastError =
            "";
        private static int _generation;
        private static volatile bool _requestInFlight;

        public static int TimeoutSeconds
        {
            get { return RequestTimeoutSeconds; }
        }

        public static int MinimumRefreshMinutes
        {
            get { return ProviderMinimumRefreshMinutes; }
        }

        public static bool RequestInFlight
        {
            get { return _requestInFlight; }
        }

        public static bool TryReadEconomicNewsSnapshot(
            string uri,
            DateTime localSuccessUtc,
            out string payload,
            out DateTime successUtc)
        {
            lock (Sync)
            {
                if (!string.Equals(
                        _uri,
                        uri,
                        StringComparison.OrdinalIgnoreCase) ||
                    _lastSuccessUtc ==
                        DateTime.MinValue ||
                    _lastSuccessUtc <=
                        localSuccessUtc)
                {
                    payload = null;
                    successUtc = DateTime.MinValue;
                    return false;
                }

                payload = _payload;
                successUtc = _lastSuccessUtc;
                return true;
            }
        }

        public static bool TryStart(
            string uri,
            DateTime nowUtc,
            int requestedRefreshMinutes,
            out int requestId)
        {
            requestId = 0;

            int refreshMinutes =
                Math.Max(
                    ProviderMinimumRefreshMinutes,
                    Math.Max(1, requestedRefreshMinutes));

            lock (Sync)
            {
                if (!string.Equals(
                        _uri,
                        uri,
                        StringComparison.OrdinalIgnoreCase))
                {
                    if (_requestInFlight)
                        return false;

                    _uri = uri;
                    _payload = "";
                    _lastAttemptUtc = DateTime.MinValue;
                    _lastSuccessUtc = DateTime.MinValue;
                    _lastError = "";
                }

                if (_requestInFlight)
                {
                    if ((nowUtc -
                         _requestStartedUtc).TotalSeconds <
                        RequestTimeoutSeconds)
                        return false;

                    _requestInFlight = false;
                    _generation++;
                    _lastError =
                        "NEWS FEED REQUEST TIMEOUT";
                    _lastAttemptUtc =
                        nowUtc;
                }

                if (_lastAttemptUtc != DateTime.MinValue &&
                    (nowUtc -
                     _lastAttemptUtc).TotalMinutes <
                    refreshMinutes)
                    return false;

                _lastAttemptUtc = nowUtc;
                _requestStartedUtc = nowUtc;
                _requestInFlight = true;
                _generation++;
                requestId = _generation;
                return true;
            }
        }

        public static bool CompleteSuccess(
            string uri,
            int requestId,
            DateTime requestUtc,
            string payload)
        {
            lock (Sync)
            {
                if (requestId != _generation ||
                    !string.Equals(
                        _uri,
                        uri,
                        StringComparison.OrdinalIgnoreCase))
                    return false;

                _payload = payload ?? "";
                _lastSuccessUtc = requestUtc;
                _lastError = "";
                _requestInFlight = false;
                _requestStartedUtc = DateTime.MinValue;
                return true;
            }
        }

        public static bool CompleteFailure(
            string uri,
            int requestId,
            string error)
        {
            lock (Sync)
            {
                if (requestId != _generation ||
                    !string.Equals(
                        _uri,
                        uri,
                        StringComparison.OrdinalIgnoreCase))
                    return false;

                _lastError =
                    error ?? "UNKNOWN FEED ERROR";
                _requestInFlight = false;
                _requestStartedUtc = DateTime.MinValue;
                return true;
            }
        }

        public static string GetLastError(
            string uri)
        {
            lock (Sync)
            {
                if (!string.Equals(
                        _uri,
                        uri,
                        StringComparison.OrdinalIgnoreCase))
                    return "";

                return _lastError ?? "";
            }
        }
    }
}
