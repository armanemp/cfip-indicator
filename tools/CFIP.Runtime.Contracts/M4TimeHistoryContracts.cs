using System;

namespace cAlgo
{
    internal static class M4TimeHistoryContracts
    {
        internal static void Run()
        {
            VerifyUtcNormalization();
            VerifyUtcDayBoundaries();
            VerifyHalfOpenIntervals();
            VerifyRollingArchivePeriod();
            VerifySessionBoundaries();
            VerifyEndOfDayBoundaries();
            VerifyDstInvariant();
            VerifyAccountScopedDailyLossIdentity();

            Console.WriteLine(
                "M4 time/session/history/persistence contracts PASS");
        }

        private static void VerifyUtcNormalization()
        {
            DateTime unspecified =
                new DateTime(
                    2026,
                    10,
                    2,
                    23,
                    59,
                    59,
                    DateTimeKind.Unspecified);

            DateTime normalized =
                CanonicalTimeRule.EnsureUtc(
                    unspecified);

            Assert(
                normalized.Kind == DateTimeKind.Utc &&
                normalized.Date == unspecified.Date &&
                normalized.TimeOfDay == unspecified.TimeOfDay,
                "unspecified algorithm timestamps are normalized without machine-local timezone shifts");
        }

        private static void VerifyUtcDayBoundaries()
        {
            DateTime beforeMidnight =
                new DateTime(
                    2026,
                    10,
                    2,
                    23,
                    59,
                    59,
                    999,
                    DateTimeKind.Utc);

            DateTime midnight =
                new DateTime(
                    2026,
                    10,
                    3,
                    0,
                    0,
                    0,
                    DateTimeKind.Utc);

            Assert(
                CanonicalTimeRule.UtcDayStart(beforeMidnight) ==
                    new DateTime(
                        2026,
                        10,
                        2,
                        0,
                        0,
                        0,
                        DateTimeKind.Utc) &&
                CanonicalTimeRule.UtcNextDayStart(beforeMidnight) ==
                    midnight &&
                !CanonicalTimeRule.IsSameUtcDay(
                    beforeMidnight,
                    midnight),
                "UTC trading-day boundary is exact at midnight");
        }

        private static void VerifyHalfOpenIntervals()
        {
            DateTime start =
                new DateTime(
                    2026,
                    10,
                    2,
                    6,
                    0,
                    0,
                    DateTimeKind.Utc);

            DateTime end =
                new DateTime(
                    2026,
                    10,
                    2,
                    20,
                    0,
                    0,
                    DateTimeKind.Utc);

            Assert(
                CanonicalTimeRule.IsInsideHalfOpenUtcInterval(
                    start,
                    start,
                    end) &&
                CanonicalTimeRule.IsInsideHalfOpenUtcInterval(
                    end.AddMinutes(-1),
                    start,
                    end) &&
                !CanonicalTimeRule.IsInsideHalfOpenUtcInterval(
                    end,
                    start,
                    end),
                "time ranges are start-inclusive/end-exclusive");
        }

        private static void VerifyRollingArchivePeriod()
        {
            DateTime sample =
                new DateTime(
                    2026,
                    10,
                    2,
                    12,
                    0,
                    0,
                    DateTimeKind.Utc);

            DateTime start =
                CanonicalTimeRule.RollingPeriodStart(
                    sample,
                    CanonicalTimeRule.OutcomeArchivePeriodDays);

            DateTime end =
                CanonicalTimeRule.RollingPeriodEndExclusive(
                    sample,
                    CanonicalTimeRule.OutcomeArchivePeriodDays);

            Assert(
                end > start &&
                (end - start).TotalDays ==
                    CanonicalTimeRule.OutcomeArchivePeriodDays &&
                CanonicalTimeRule.RollingPeriodStart(
                    start,
                    CanonicalTimeRule.OutcomeArchivePeriodDays) ==
                    start &&
                CanonicalTimeRule.RollingPeriodStart(
                    end.AddTicks(-1),
                    CanonicalTimeRule.OutcomeArchivePeriodDays) ==
                    start &&
                CanonicalTimeRule.RollingPeriodStart(
                    end,
                    CanonicalTimeRule.OutcomeArchivePeriodDays) !=
                    start,
                "90-day archive period is deterministic and boundary-stable");
        }

        private static void VerifySessionBoundaries()
        {
            DateTime sameDayStart =
                Utc(2026, 10, 2, 6);

            DateTime sameDayEnd =
                Utc(2026, 10, 2, 20);

            Assert(
                SessionWindowRule.IsInside(
                    sameDayStart,
                    6,
                    20) &&
                !SessionWindowRule.IsInside(
                    sameDayEnd,
                    6,
                    20) &&
                SessionWindowRule.IsInside(
                    new DateTime(
                        2026,
                        10,
                        2,
                        12,
                        0,
                        0,
                        DateTimeKind.Unspecified),
                    6,
                    20),
                "same-day session is inclusive at start and exclusive at end");

            Assert(
                SessionWindowRule.IsInside(
                    Utc(2026, 10, 2, 23),
                    22,
                    6) &&
                SessionWindowRule.IsInside(
                    Utc(2026, 10, 3, 5),
                    22,
                    6) &&
                !SessionWindowRule.IsInside(
                    Utc(2026, 10, 3, 12),
                    22,
                    6),
                "overnight session remains symmetric across midnight");

            Assert(
                SessionWindowRule.IsInside(
                    Utc(2026, 3, 29, 12),
                    6,
                    6),
                "start==end session remains the explicit full-day mode");
        }

        private static void VerifyEndOfDayBoundaries()
        {
            DateTime boundary;

            Assert(
                SessionWindowRule.TryResolveEndOfDayBoundary(
                    Utc(2026, 10, 2, 19, 58),
                    6,
                    20,
                    5,
                    out boundary) &&
                boundary ==
                    Utc(2026, 10, 2, 20),
                "same-day EOD boundary resolves to the intended UTC close");

            Assert(
                SessionWindowRule.IsWithinPreBoundaryWindow(
                    Utc(2026, 10, 2, 19, 55),
                    boundary,
                    5) &&
                SessionWindowRule.IsWithinPostBoundaryWindow(
                    Utc(2026, 10, 2, 20, 4),
                    boundary,
                    5),
                "EOD alert/cleanup windows share the same canonical boundary");
        }

        private static void VerifyDstInvariant()
        {
            DateTime beforeUsDst =
                new DateTime(
                    2026,
                    3,
                    8,
                    6,
                    0,
                    0,
                    DateTimeKind.Utc);

            DateTime afterUsDst =
                new DateTime(
                    2026,
                    3,
                    9,
                    6,
                    0,
                    0,
                    DateTimeKind.Utc);

            DateTime beforeEuDst =
                new DateTime(
                    2026,
                    10,
                    25,
                    6,
                    0,
                    0,
                    DateTimeKind.Utc);

            DateTime afterEuDst =
                new DateTime(
                    2026,
                    10,
                    26,
                    6,
                    0,
                    0,
                    DateTimeKind.Utc);

            Assert(
                CanonicalTimeRule.UtcDayKey(beforeUsDst) == "20260308" &&
                CanonicalTimeRule.UtcDayKey(afterUsDst) == "20260309" &&
                CanonicalTimeRule.UtcDayKey(beforeEuDst) == "20261025" &&
                CanonicalTimeRule.UtcDayKey(afterEuDst) == "20261026",
                "UTC trading-day identity is invariant across DST calendar transitions");

            Assert(
                SessionWindowRule.IsInside(
                    beforeUsDst,
                    6,
                    20) &&
                SessionWindowRule.IsInside(
                    afterUsDst,
                    6,
                    20) &&
                SessionWindowRule.IsInside(
                    beforeEuDst,
                    6,
                    20) &&
                SessionWindowRule.IsInside(
                    afterEuDst,
                    6,
                    20),
                "UTC session membership does not change because the machine/platform enters or leaves DST");
        }

        private static void VerifyAccountScopedDailyLossIdentity()
        {
            string first =
                OutcomeMemoryIdentityRule.BuildAccountScopeToken(
                    "BrokerA",
                    100,
                    "HEDGING",
                    true);

            string second =
                OutcomeMemoryIdentityRule.BuildAccountScopeToken(
                    "BrokerB",
                    100,
                    "HEDGING",
                    true);

            string third =
                OutcomeMemoryIdentityRule.BuildAccountScopeToken(
                    "BrokerA",
                    100,
                    "HEDGING",
                    false);

            Assert(
                first != second &&
                first != third &&
                first ==
                    OutcomeMemoryIdentityRule.BuildAccountScopeToken(
                        "BrokerA",
                        100,
                        "HEDGING",
                        true),
                "account-scoped persistence identity is stable and broker/live-state separated");
        }

        private static DateTime Utc(
            int year,
            int month,
            int day,
            int hour)
        {
            return new DateTime(
                year,
                month,
                day,
                hour,
                0,
                0,
                DateTimeKind.Utc);
        }

        private static void Assert(
            bool condition,
            string message)
        {
            if (!condition)
                throw new InvalidOperationException(
                    "M4 contract failed: " + message);
        }
    }
}
