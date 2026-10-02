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
                CanonicalTimeRule.EnsureUtc(
                    nowUtc);

            DateTime last =
                CanonicalTimeRule.EnsureUtc(
                    lastRefreshUtc);

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
