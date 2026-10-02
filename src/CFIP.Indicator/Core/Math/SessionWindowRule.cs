using System;

namespace cAlgo
{
    internal static class SessionWindowRule
    {
        internal const int DefaultEndOfDayCloseWindowMinutes = 5;
        internal const int SessionResolutionMinutes = 60;

        internal static bool IsInside(
            DateTime utc,
            int sessionStartHour,
            int sessionEndHour)
        {
            int startMinute =
                NormalizeHour(sessionStartHour) * SessionResolutionMinutes;
            int endMinute =
                NormalizeHour(sessionEndHour) * SessionResolutionMinutes;
            int nowMinute =
                utc.Hour * 60 + utc.Minute;

            if (startMinute == endMinute)
                return true;

            return startMinute < endMinute
                ? nowMinute >= startMinute &&
                  nowMinute < endMinute
                : nowMinute >= startMinute ||
                  nowMinute < endMinute;
        }

        internal static bool TryResolveSessionWindow(
            DateTime utc,
            int sessionStartHour,
            int sessionEndHour,
            out DateTime startUtc,
            out DateTime endUtc)
        {
            startUtc = default(DateTime);
            endUtc = default(DateTime);

            DateTime reference =
                EnsureUtc(utc);

            int startHour =
                NormalizeHour(sessionStartHour);
            int endHour =
                NormalizeHour(sessionEndHour);

            DateTime dayStart =
                CanonicalTimeRule.UtcDayStart(
                    reference);

            int startMinute = startHour * SessionResolutionMinutes;
            int endMinute = endHour * SessionResolutionMinutes;
            int nowMinute =
                reference.Hour * 60 + reference.Minute;

            if (startMinute == endMinute)
            {
                if (nowMinute < endMinute)
                {
                    startUtc =
                        dayStart
                            .AddDays(-1)
                            .AddMinutes(startMinute);
                    endUtc =
                        dayStart.AddMinutes(endMinute);
                }
                else
                {
                    startUtc =
                        dayStart.AddMinutes(startMinute);
                    endUtc =
                        dayStart
                            .AddDays(1)
                            .AddMinutes(endMinute);
                }

                return true;
            }

            if (startMinute < endMinute)
            {
                if (nowMinute < startMinute)
                {
                    startUtc =
                        dayStart
                            .AddDays(-1)
                            .AddMinutes(startMinute);
                    endUtc =
                        dayStart
                            .AddDays(-1)
                            .AddMinutes(endMinute);
                }
                else
                {
                    startUtc =
                        dayStart.AddMinutes(startMinute);
                    endUtc =
                        dayStart.AddMinutes(endMinute);
                }

                return true;
            }

            if (nowMinute >= startMinute)
            {
                startUtc =
                    dayStart.AddMinutes(startMinute);
                endUtc =
                    dayStart
                        .AddDays(1)
                        .AddMinutes(endMinute);
            }
            else
            {
                startUtc =
                    dayStart
                        .AddDays(-1)
                        .AddMinutes(startMinute);
                endUtc =
                    dayStart.AddMinutes(endMinute);
            }

            return true;
        }

        internal static bool TryResolveEndOfDayBoundary(
            DateTime utc,
            int sessionStartHour,
            int sessionEndHour,
            int closeWindowMinutes,
            out DateTime boundaryUtc)
        {
            boundaryUtc = default(DateTime);

            DateTime reference =
                EnsureUtc(utc);

            int startHour =
                NormalizeHour(sessionStartHour);
            int endHour =
                NormalizeHour(sessionEndHour);

            DateTime dayStart =
                new DateTime(
                    reference.Year,
                    reference.Month,
                    reference.Day,
                    0,
                    0,
                    0,
                    DateTimeKind.Utc);

            int startMinute = startHour * SessionResolutionMinutes;
            int endMinute = endHour * SessionResolutionMinutes;
            int nowMinute =
                reference.Hour * 60 + reference.Minute;

            DateTime candidate;

            if (startMinute == endMinute)
            {
                candidate =
                    dayStart.AddMinutes(endMinute);

                DateTime closeWindowEnd =
                    candidate.AddMinutes(
                        Math.Max(0, closeWindowMinutes));

                if (reference > closeWindowEnd)
                {
                    candidate =
                        candidate.AddDays(1);
                }
            }
            else if (startMinute < endMinute)
            {
                if (nowMinute < startMinute)
                {
                    candidate =
                        dayStart
                            .AddDays(-1)
                            .AddMinutes(endMinute);

                    if (reference >
                        candidate.AddMinutes(
                            Math.Max(0, closeWindowMinutes)))
                    {
                        candidate =
                            dayStart.AddMinutes(endMinute);
                    }
                }
                else
                {
                    candidate =
                        dayStart.AddMinutes(endMinute);

                    if (reference >
                        candidate.AddMinutes(
                            Math.Max(0, closeWindowMinutes)))
                    {
                        candidate =
                            candidate.AddDays(1);
                    }
                }
            }
            else
            {
                candidate =
                    nowMinute >= startMinute
                        ? dayStart
                            .AddDays(1)
                            .AddMinutes(endMinute)
                        : dayStart.AddMinutes(endMinute);

                if (reference >
                    candidate.AddMinutes(
                        Math.Max(0, closeWindowMinutes)))
                {
                    candidate =
                        candidate.AddDays(1);
                }
            }

            boundaryUtc = candidate;
            return true;
        }

        internal static bool IsWithinPreBoundaryWindow(
            DateTime utc,
            DateTime boundaryUtc,
            int minutesBefore)
        {
            DateTime reference =
                EnsureUtc(utc);
            DateTime boundary =
                EnsureUtc(boundaryUtc);

            int window =
                Math.Max(0, minutesBefore);

            DateTime start =
                boundary.AddMinutes(-window);

            return reference >= start &&
                   reference <= boundary;
        }

        internal static bool IsWithinPostBoundaryWindow(
            DateTime utc,
            DateTime boundaryUtc,
            int windowMinutes)
        {
            DateTime reference =
                EnsureUtc(utc);
            DateTime boundary =
                EnsureUtc(boundaryUtc);

            int window =
                Math.Max(0, windowMinutes);

            return reference >= boundary &&
                   reference <= boundary.AddMinutes(window);
        }

        private static int NormalizeHour(int hour)
        {
            return Math.Max(0, Math.Min(23, hour));
        }

        private static DateTime EnsureUtc(DateTime value)
        {
            return CanonicalTimeRule.EnsureUtc(value);
        }
    }
}
