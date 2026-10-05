// CFIP Indicator — DailyLossPersistence.cs
// Persistent account-scoped Daily Loss state.

using System;
using System.Globalization;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private const string DailyLossStateSchema =
            "CFIP-DL,2";

        private void RefreshSharedDailyLossLock(
            DateTime referenceUtc)
        {
            try
            {
                string stored =
                    LocalStorage.GetString(
                        DailyLossStorageKey(),
                        LocalStorageScope.Type);

                ApplySharedDailyLossLock(
                    referenceUtc,
                    stored);
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP shared daily loss lock read failed: {0}",
                    ex.Message);
            }
        }

        private void ReloadSharedDailyLossLockFromStorage(
            DateTime referenceUtc)
        {
            if ((referenceUtc -
                 _lastDailyLossSharedStateReloadUtc).TotalSeconds <
                1)
                return;

            try
            {
                LocalStorage.Reload(
                    LocalStorageScope.Type);

                _lastDailyLossSharedStateReloadUtc =
                    referenceUtc;

                RefreshSharedDailyLossLock(
                    referenceUtc);
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP shared daily loss lock reload failed: {0}",
                    ex.Message);
            }
        }

        private void ApplySharedDailyLossLock(
            DateTime referenceUtc,
            string stored)
        {
            if (string.IsNullOrWhiteSpace(stored))
                return;

            string[] parts =
                stored.Split('|');

            if (parts.Length < 6 ||
                !string.Equals(
                    parts[0],
                    DailyLossStateSchema,
                    StringComparison.Ordinal))
                return;

            DateTime baselineDate =
                new DateTime(
                    long.Parse(
                        parts[1],
                        CultureInfo.InvariantCulture),
                    DateTimeKind.Utc);

            if (!CanonicalTimeRule.IsSameUtcDay(
                    baselineDate,
                    referenceUtc))
                return;

            if (parts[4] == "1")
                _dailyLossLocked = true;

            if (parts[5] == "1")
                _dailyLossLimitAlerted = true;
        }

        private bool RestoreDailyLossState(
            DateTime referenceUtc)
        {
            try
            {
                LocalStorage.Reload(
                    LocalStorageScope.Type);

                string stored =
                    LocalStorage.GetString(
                        DailyLossStorageKey(),
                        LocalStorageScope.Type);

                if (string.IsNullOrWhiteSpace(stored))
                    return false;

                string[] parts =
                    stored.Split('|');

                if (parts.Length < 6 ||
                    !string.Equals(
                        parts[0],
                        DailyLossStateSchema,
                        StringComparison.Ordinal))
                    return false;

                DateTime baselineDate =
                    new DateTime(
                        long.Parse(
                            parts[1],
                            CultureInfo.InvariantCulture),
                        DateTimeKind.Utc);

                double startEquity =
                    double.Parse(
                        parts[2],
                        CultureInfo.InvariantCulture);

                double startFloating =
                    double.Parse(
                        parts[3],
                        CultureInfo.InvariantCulture);

                bool locked =
                    parts[4] == "1";

                bool alerted =
                    parts[5] == "1";

                if (!CanonicalTimeRule.IsSameUtcDay(
                        baselineDate,
                        referenceUtc) ||
                    !IsFinitePositive(
                        startEquity) ||
                    double.IsNaN(startFloating) ||
                    double.IsInfinity(startFloating))
                    return false;

                _dailyLossBaselineDate =
                    CanonicalTimeRule.UtcDayStart(
                        baselineDate);

                _dailyLossStartEquity =
                    startEquity;

                _dailyLossBaselineUnrealizedNetProfit =
                    startFloating;

                _dailyLossRealizedNetProfit = 0;
                _dailyLossNetCashFlow = 0;
                _dailyLossHistoryCount = -1;
                _dailyLossTransactionCount = -1;
                _dailyLossTransactionsAvailable = false;
                _dailyLossLocked = locked;
                _dailyLossLimitAlerted = alerted;
                _lastDailyLossPersistUtc =
                    DateTime.MinValue;
                _lastDailyLossSharedStateReloadUtc =
                    referenceUtc;

                _dailyLossStateReason =
                    locked
                        ? "DAILY LOSS LIMIT LOCKED"
                        : "RESTORED";

                return true;
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP daily loss state restore failed: {0}",
                    ex.Message);
                return false;
            }
        }

        private void PersistDailyLossState(
            DateTime referenceUtc,
            bool forceFlush = false)
        {
            try
            {
                string payload =
                    DailyLossStateSchema +
                    "|" +
                    CanonicalTimeRule.UtcDayStart(
                        referenceUtc).Ticks.ToString(
                            CultureInfo.InvariantCulture) +
                    "|" +
                    _dailyLossStartEquity.ToString(
                        "R",
                        CultureInfo.InvariantCulture) +
                    "|" +
                    _dailyLossBaselineUnrealizedNetProfit.ToString(
                        "R",
                        CultureInfo.InvariantCulture) +
                    "|" +
                    (_dailyLossLocked ? "1" : "0") +
                    "|" +
                    (_dailyLossLimitAlerted ? "1" : "0");

                LocalStorage.SetString(
                    DailyLossStorageKey(),
                    payload,
                    LocalStorageScope.Type);

                // SetString is intentionally kept in memory here. Explicit disk
                // flushing is owned by the Timer heartbeat, with forceFlush used
                // for a newly acquired daily-loss lock.
                MarkDailyLossPersistenceDirty(
                    forceFlush);

                _lastDailyLossPersistUtc =
                    AsUtc(referenceUtc);
                _lastDailyLossSharedStateReloadUtc =
                    AsUtc(referenceUtc);
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP daily loss state persist queue failed: {0}",
                    ex.Message);
            }
        }

        private void PersistDailyLossStateIfNeeded(
            DateTime referenceUtc,
            bool force)
        {
            if (force ||
                !CanonicalTimeRule.IsSameUtcDay(
                    _dailyLossBaselineDate,
                    referenceUtc) ||
                (referenceUtc -
                 _lastDailyLossPersistUtc).TotalSeconds >=
                    60)
            {
                PersistDailyLossState(
                    referenceUtc,
                    force);
            }
        }

        private string DailyLossStorageKey()
        {
            return
                "CFIP DailyLoss " +
                MemoryAccountScopeToken();
        }

        private static DateTime AsUtc(
            DateTime value)
        {
            return CanonicalTimeRule.EnsureUtc(
                value);
        }
    }
}
