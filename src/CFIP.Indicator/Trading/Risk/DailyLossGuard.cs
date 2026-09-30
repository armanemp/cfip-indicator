// CFIP Indicator — DailyLossGuard.cs
// Single-responsibility account loss guard.
// The deterministic loss calculation lives in Core/Math/DailyLossRule.cs.

using System;
using System.Collections.Generic;
using System.Globalization;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private const string DailyLossStateSchema =
            "CFIP-DL,2";

        private bool DailyLossLimitHit(
            DateTime nowUtc)
        {
            string reason;
            return DailyLossLimitHit(
                nowUtc,
                out reason);
        }

        private bool DailyLossLimitHit(
            DateTime nowUtc,
            out string reason)
        {
            reason = "";

            if (!EnableDailyLossLimit)
            {
                _dailyLossStateReason =
                    "DISABLED";
                return false;
            }

            try
            {
                DateTime reference =
                    AsUtc(nowUtc);

                EnsureDailyLossBaseline(
                    reference);

                RefreshDailyLossTradeFacts(
                    reference);

                double currentEquity =
                    Account.Equity;

                double currentUnrealized =
                    Account.UnrealizedNetProfit;

                DailyLossEvaluation evaluation =
                    DailyLossRule.Evaluate(
                        _dailyLossStartEquity,
                        _dailyLossBaselineUnrealizedNetProfit,
                        currentEquity,
                        currentUnrealized,
                        _dailyLossRealizedNetProfit,
                        _dailyLossNetCashFlow,
                        MaximumDailyLossPercent,
                        _dailyLossHistoryAvailable,
                        _dailyLossLocked);

                _dailyLossEvaluation =
                    evaluation;
                _dailyLossDataReady =
                    evaluation.DataReady;
                _dailyLossStateReason =
                    evaluation.Reason;

                if (!evaluation.DataReady)
                {
                    reason =
                        "DAILY LOSS DATA UNAVAILABLE";
                    return true;
                }

                if (evaluation.LimitHit)
                {
                    if (!_dailyLossLocked)
                    {
                        _dailyLossLocked = true;
                        _dailyLossLimitAlerted = false;
                    }

                    if (!_dailyLossLimitAlerted)
                    {
                        _dailyLossLimitAlerted =
                            true;

                        PersistDailyLossState(
                            reference);

                        SendUnifiedAlert(
                            "DAILYLOSS|" +
                            reference.Date.ToString(
                                "yyyyMMdd",
                                CultureInfo.InvariantCulture),
                            "Daily loss limit reached (" +
                            evaluation.LossPercent.ToString(
                                "F2",
                                CultureInfo.InvariantCulture) +
                            "% >= " +
                            MaximumDailyLossPercent.ToString(
                                "F2",
                                CultureInfo.InvariantCulture) +
                            "%) - new automatic entries are blocked for the " +
                            "rest of the UTC trading day. Open positions are left untouched.",
                            0,
                            true);
                    }
                    else
                    {
                        PersistDailyLossStateIfNeeded(
                            reference,
                            false);
                    }

                    reason =
                        "DAILY LOSS LIMIT REACHED";

                    return true;
                }

                if (_dailyLossLocked)
                {
                    reason =
                        "DAILY LOSS LIMIT LOCKED";
                    return true;
                }

                reason =
                    evaluation.UsedEquityFallback
                        ? "DAILY LOSS OK • EQUITY FALLBACK"
                        : "DAILY LOSS OK";

                return false;
            }
            catch (Exception ex)
            {
                _dailyLossDataReady = false;
                _dailyLossStateReason =
                    "DAILY LOSS DATA UNAVAILABLE";

                reason =
                    "DAILY LOSS DATA UNAVAILABLE";

                Print(
                    "CFIP daily loss evaluation failed: {0}",
                    ex.ToString());

                return true;
            }
        }

        private void EnsureDailyLossBaseline(
            DateTime referenceUtc)
        {
            if (_dailyLossBaselineDate.Date ==
                    referenceUtc.Date &&
                IsFinitePositive(
                    _dailyLossStartEquity))
                return;

            if (RestoreDailyLossState(
                    referenceUtc))
                return;

            _dailyLossBaselineDate =
                referenceUtc.Date;

            _dailyLossStartEquity =
                Account.Equity;

            _dailyLossBaselineUnrealizedNetProfit =
                Account.UnrealizedNetProfit;

            _dailyLossRealizedNetProfit = 0;
            _dailyLossNetCashFlow = 0;
            _dailyLossHistoryCount = -1;
            _dailyLossTransactionCount = -1;
            _dailyLossHistoryAvailable = false;
            _dailyLossLocked = false;
            _dailyLossLimitAlerted = false;
            _dailyLossEvaluationUtc = DateTime.MinValue;

            PersistDailyLossState(
                referenceUtc);
        }

        private void RefreshDailyLossTradeFacts(
            DateTime referenceUtc)
        {
            if (_dailyLossBaselineDate.Date !=
                    referenceUtc.Date)
                return;

            bool historyNeedsRefresh =
                _dailyLossHistoryCount < 0 ||
                History == null ||
                History.Count !=
                    _dailyLossHistoryCount;

            bool transactionsNeedRefresh =
                _dailyLossTransactionCount < 0 ||
                Transactions == null ||
                Transactions.Count !=
                    _dailyLossTransactionCount;

            if (historyNeedsRefresh)
                RefreshDailyRealizedNetProfit(
                    referenceUtc);

            if (transactionsNeedRefresh)
                RefreshDailyCashFlows(
                    referenceUtc);

            _dailyLossEvaluationUtc =
                referenceUtc;
        }

        private void RefreshDailyRealizedNetProfit(
            DateTime referenceUtc)
        {
            _dailyLossRealizedNetProfit = 0;
            _dailyLossHistoryAvailable = false;

            if (History == null)
                return;

            DateTime startUtc =
                new DateTime(
                    referenceUtc.Year,
                    referenceUtc.Month,
                    referenceUtc.Day,
                    0,
                    0,
                    0,
                    DateTimeKind.Utc);

            DateTime endExclusive =
                startUtc.AddDays(1);

            try
            {
                foreach (HistoricalTrade trade in History)
                {
                    if (trade == null)
                        continue;

                    DateTime closeUtc =
                        AsUtc(trade.ClosingTime);

                    if (closeUtc < startUtc ||
                        closeUtc >= endExclusive)
                        continue;

                    double netProfit =
                        trade.NetProfit;

                    if (double.IsNaN(netProfit) ||
                        double.IsInfinity(netProfit))
                        continue;

                    _dailyLossRealizedNetProfit +=
                        netProfit;
                }

                _dailyLossHistoryCount =
                    History.Count;

                _dailyLossHistoryAvailable = true;
            }
            catch (Exception ex)
            {
                _dailyLossHistoryAvailable = false;

                Print(
                    "CFIP daily realized history refresh failed: {0}",
                    ex.Message);
            }
        }

        private void RefreshDailyCashFlows(
            DateTime referenceUtc)
        {
            _dailyLossNetCashFlow = 0;

            if (Transactions == null)
            {
                _dailyLossTransactionCount = 0;
                return;
            }

            DateTime startUtc =
                new DateTime(
                    referenceUtc.Year,
                    referenceUtc.Month,
                    referenceUtc.Day,
                    0,
                    0,
                    0,
                    DateTimeKind.Utc);

            DateTime endExclusive =
                startUtc.AddDays(1);

            try
            {
                foreach (Transaction transaction
                         in Transactions)
                {
                    if (transaction == null)
                        continue;

                    DateTime transactionUtc =
                        AsUtc(transaction.Time);

                    if (transactionUtc < startUtc ||
                        transactionUtc >= endExclusive)
                        continue;

                    if (transaction.Type ==
                        TransactionType.Deposit)
                    {
                        _dailyLossNetCashFlow +=
                            Math.Abs(
                                transaction.Amount);
                    }
                    else if (transaction.Type ==
                             TransactionType.Withdrawal)
                    {
                        _dailyLossNetCashFlow -=
                            Math.Abs(
                                transaction.Amount);
                    }
                }

                _dailyLossTransactionCount =
                    Transactions.Count;
            }
            catch (Exception ex)
            {
                _dailyLossTransactionCount = -1;

                Print(
                    "CFIP daily cash-flow refresh failed: {0}",
                    ex.Message);
            }
        }

        private bool RestoreDailyLossState(
            DateTime referenceUtc)
        {
            try
            {
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
                _dailyLossLocked = locked;
                _dailyLossLimitAlerted = alerted;
                _dailyLossDataReady = false;
                _dailyLossEvaluationUtc =
                    DateTime.MinValue;
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
                _lastDailyLossPersistUtc =
                    referenceUtc;
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
