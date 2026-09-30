using System;

namespace cAlgo
{
    internal enum EconomicNewsFeedState
    {
        Disabled = 0,
        NeverLoaded = 1,
        Healthy = 2,
        Stale = 3,
        BlockingEvent = 4
    }

    internal static class EconomicNewsFeedStateRule
    {
        internal static EconomicNewsFeedState Resolve(
            bool enabled,
            DateTime lastSuccessUtc,
            DateTime nowUtc,
            double maximumAgeMinutes,
            bool blockingEvent)
        {
            if (!enabled)
                return EconomicNewsFeedState.Disabled;

            if (blockingEvent)
                return EconomicNewsFeedState.BlockingEvent;

            if (lastSuccessUtc == DateTime.MinValue)
                return EconomicNewsFeedState.NeverLoaded;

            DateTime now =
                nowUtc.Kind == DateTimeKind.Utc
                    ? nowUtc
                    : nowUtc.ToUniversalTime();

            DateTime success =
                lastSuccessUtc.Kind == DateTimeKind.Utc
                    ? lastSuccessUtc
                    : lastSuccessUtc.ToUniversalTime();

            double ageMinutes =
                (now - success).TotalMinutes;

            double maximumAge =
                Math.Max(
                    1,
                    maximumAgeMinutes);

            return ageMinutes <= maximumAge
                ? EconomicNewsFeedState.Healthy
                : EconomicNewsFeedState.Stale;
        }
    }
}
