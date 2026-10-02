using System;
using System.Globalization;

namespace cAlgo
{
    // Single owner for timezone-invariant UTC normalization and period boundaries.
    internal static class CanonicalTimeRule
    {
        internal const int OutcomeArchivePeriodDays = 90;

        internal static DateTime EnsureUtc(DateTime value)
        {
            if (value == DateTime.MinValue)
                return DateTime.MinValue;

            if (value.Kind == DateTimeKind.Utc)
                return value;

            // The production Indicator and cBot are explicitly UTC-based.
            // Treat unspecified API timestamps as already expressed in the
            // algorithm timezone rather than consulting the machine-local zone.
            return DateTime.SpecifyKind(
                value,
                DateTimeKind.Utc);
        }

        internal static DateTime UtcDayStart(DateTime value)
        {
            DateTime utc = EnsureUtc(value);

            if (utc == DateTime.MinValue)
                return DateTime.MinValue;

            return new DateTime(
                utc.Year,
                utc.Month,
                utc.Day,
                0,
                0,
                0,
                DateTimeKind.Utc);
        }

        internal static DateTime UtcNextDayStart(DateTime value)
        {
            DateTime start = UtcDayStart(value);

            return start == DateTime.MinValue
                ? DateTime.MinValue
                : start.AddDays(1);
        }

        internal static bool IsSameUtcDay(
            DateTime left,
            DateTime right)
        {
            DateTime leftStart = UtcDayStart(left);
            DateTime rightStart = UtcDayStart(right);

            return leftStart != DateTime.MinValue &&
                   leftStart == rightStart;
        }

        internal static string UtcDayKey(
            DateTime value)
        {
            DateTime utc = EnsureUtc(value);

            return utc == DateTime.MinValue
                ? ""
                : utc.ToString(
                    "yyyyMMdd",
                    CultureInfo.InvariantCulture);
        }

        internal static DateTime RollingPeriodStart(
            DateTime observedUtc,
            int periodDays)
        {
            DateTime utc = EnsureUtc(observedUtc);

            if (utc == DateTime.MinValue)
                return DateTime.MinValue;

            int daysPerPeriod =
                Math.Max(
                    1,
                    periodDays);

            DateTime epoch =
                new DateTime(
                    1970,
                    1,
                    1,
                    0,
                    0,
                    0,
                    DateTimeKind.Utc);

            long days =
                (long)Math.Floor(
                    (UtcDayStart(utc) - epoch).TotalDays);

            long periodOffset =
                days -
                (days % daysPerPeriod);

            return epoch.AddDays(
                periodOffset);
        }

        internal static DateTime RollingPeriodEndExclusive(
            DateTime observedUtc,
            int periodDays)
        {
            DateTime start =
                RollingPeriodStart(
                    observedUtc,
                    periodDays);

            return start == DateTime.MinValue
                ? DateTime.MinValue
                : start.AddDays(
                    Math.Max(
                        1,
                        periodDays));
        }

        internal static bool IsInsideHalfOpenUtcInterval(
            DateTime value,
            DateTime startInclusive,
            DateTime endExclusive)
        {
            DateTime utc = EnsureUtc(value);
            DateTime start = EnsureUtc(startInclusive);
            DateTime end = EnsureUtc(endExclusive);

            return utc != DateTime.MinValue &&
                   start != DateTime.MinValue &&
                   end != DateTime.MinValue &&
                   start < end &&
                   utc >= start &&
                   utc < end;
        }
    }
}
