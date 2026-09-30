using System;

namespace cAlgo
{
    internal static class BrokerStateRefreshRule
    {
        internal static bool IsRefreshDue(
            bool dirty,
            DateTime lastRefreshUtc,
            DateTime nowUtc,
            int minimumIntervalMilliseconds)
        {
            if (dirty ||
                lastRefreshUtc == DateTime.MinValue)
                return true;

            DateTime now =
                nowUtc.Kind == DateTimeKind.Utc
                    ? nowUtc
                    : nowUtc.ToUniversalTime();

            DateTime last =
                lastRefreshUtc.Kind == DateTimeKind.Utc
                    ? lastRefreshUtc
                    : lastRefreshUtc.ToUniversalTime();

            if (now < last)
                return true;

            return
                (now - last).TotalMilliseconds >=
                Math.Max(
                    1,
                    minimumIntervalMilliseconds);
        }
    }
}
