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

                string stored =
                    LocalStorage.GetString(
                        DailyLossStorageKey(),
                        LocalStorageScope.Type);

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

                if (baselineDate.Date !=
                    referenceUtc.Date)
                    return;

                if (parts[4] == "1")
                    _dailyLossLocked = true;

                if (parts[5] == "1")
                    _dailyLossLimitAlerted = true;
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP shared daily loss lock reload failed: {0}",
                    ex.Message);
            }
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

                if (baselineDate.Date !=
                        referenceUtc.Date ||
                    !IsFinitePositive(
                        startEquity) ||
                    double.IsNaN(startFloating) ||
                    double.IsInfinity(startFloating))
                    return false;

                _dailyLossBaselineDate =
                    baselineDate.Date;

                _dailyLossStartEquity =
                    startEquity;

                _dailyLossBaselineUnrealizedNetProfit =
                    startFloating;

                _dailyLossRealizedNetProfit = 0;
                _dailyLossNetCashFlow = 0;
                _dailyLossHistoryCount = -1;
                _dailyLossTransactionCount = -1;
                _dailyLossHistoryAvailable = false;
                _dailyLossTransactionsAvailable = false;
                _dailyLossLocked = locked;
                _dailyLossLimitAlerted = alerted;
                _dailyLossDataReady = false;
                _dailyLossEvaluationUtc =
                    DateTime.MinValue;
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
            DateTime referenceUtc)
        {
            try
            {
                string payload =
                    DailyLossStateSchema +
                    "|" +
                    new DateTime(
                        referenceUtc.Year,
                        referenceUtc.Month,
                        referenceUtc.Day,
                        0,
                        0,
                        0,
                        DateTimeKind.Utc).Ticks.ToString(
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

                LocalStorage.Flush(
                    LocalStorageScope.Type);

                _lastDailyLossPersistUtc =
                    AsUtc(referenceUtc);
                _lastDailyLossSharedStateReloadUtc =
                    AsUtc(referenceUtc);
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP daily loss state persist failed: {0}",
                    ex.Message);
            }
        }

        private void PersistDailyLossStateIfNeeded(
            DateTime referenceUtc,
            bool force)
        {
            if (force ||
                _dailyLossBaselineDate.Date !=
                    referenceUtc.Date ||
                (referenceUtc -
                 _lastDailyLossPersistUtc).TotalSeconds >=
                    60)
            {
                PersistDailyLossState(
                    referenceUtc);
            }
        }

        private string DailyLossStorageKey()
        {
            return
                "CFIP DailyLoss " +
                Account.Number.ToString(
                    CultureInfo.InvariantCulture);
        }

        private static DateTime AsUtc(
            DateTime value)
        {
            return value.Kind == DateTimeKind.Utc
                ? value
                : value.ToUniversalTime();
        }
    }
}
