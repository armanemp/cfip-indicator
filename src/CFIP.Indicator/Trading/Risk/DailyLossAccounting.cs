// CFIP Indicator — DailyLossAccounting.cs
// Account-history and transaction data acquisition for Daily Loss.

using System;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void RefreshDailyLossTradeFacts(
            DateTime referenceUtc)
        {
            if (!CanonicalTimeRule.IsSameUtcDay(
                    _dailyLossBaselineDate,
                    referenceUtc))
                return;

            bool historyNeedsRefresh =
                _dailyLossHistoryCount < 0 ||
                History == null ||
                History.Count !=
                    _dailyLossHistoryCount;

            bool transactionsNeedRefresh =
                _dailyLossTransactionCount < 0 ||
                !_dailyLossTransactionsAvailable ||
                Transactions == null ||
                Transactions.Count !=
                    _dailyLossTransactionCount;

            if (historyNeedsRefresh)
                RefreshDailyRealizedNetProfit(
                    referenceUtc);

            if (transactionsNeedRefresh)
                RefreshDailyCashFlows(
                    referenceUtc);
        }

        private void RefreshDailyRealizedNetProfit(
            DateTime referenceUtc)
        {
            _dailyLossRealizedNetProfit = 0;
            _dailyLossHistoryAvailable = false;

            if (History == null)
                return;

            DateTime startUtc =
                CanonicalTimeRule.UtcDayStart(
                    referenceUtc);

            DateTime endExclusive =
                CanonicalTimeRule.UtcNextDayStart(
                    referenceUtc);

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
            _dailyLossTransactionsAvailable = false;

            if (Transactions == null)
            {
                _dailyLossTransactionCount = -1;
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
                _dailyLossTransactionsAvailable =
                    true;
            }
            catch (Exception ex)
            {
                _dailyLossTransactionCount = -1;

                Print(
                    "CFIP daily cash-flow refresh failed: {0}",
                    ex.Message);
            }
        }
    }
}
