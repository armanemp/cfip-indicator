// CFIP Indicator — DailyLossGuard.cs
// Single-responsibility account loss gate.

using System;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void OnAccountSwitched(
            AccountSwitchedEventArgs args)
        {
            _dailyLossBaselineDate =
                DateTime.MinValue;
            _dailyLossStartEquity = 0;
            _dailyLossBaselineUnrealizedNetProfit = 0;
            _dailyLossRealizedNetProfit = 0;
            _dailyLossNetCashFlow = 0;
            _dailyLossHistoryCount = -1;
            _dailyLossTransactionCount = -1;
            _dailyLossHistoryAvailable = false;
            _dailyLossTransactionsAvailable = false;
            _dailyLossDataReady = false;
            _dailyLossLocked = false;
            _dailyLossLimitAlerted = false;
            _dailyLossStateReason =
                "ACCOUNT SWITCHED";
            _lastDailyLossPersistUtc =
                DateTime.MinValue;
            _lastDailyLossSharedStateReloadUtc =
                DateTime.MinValue;

            try
            {
                RestoreDailyLossState(
                    AsUtc(TimeInUtc));
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP daily loss account-switch restore failed: {0}",
                    ex.Message);
            }
        }

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

                RefreshSharedDailyLossLock(
                    reference);

                RefreshDailyLossTradeFacts(
                    reference);

                double currentEquity =
                    Account.Equity;

                double currentUnrealized =
                    Account.UnrealizedNetProfit;

                if (!_dailyLossHistoryAvailable &&
                    !_dailyLossTransactionsAvailable)
                {
                    _dailyLossDataReady = false;
                    _dailyLossStateReason =
                        "DAILY LOSS DATA UNAVAILABLE";
                    reason =
                        "DAILY LOSS DATA UNAVAILABLE";
                    return true;
                }

                DailyLossRule.Evaluation evaluation =
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
                        _dailyLossLimitAlerted = true;

                        PersistDailyLossState(
                            reference);

                        SendUnifiedAlert(
                            "DAILYLOSS|" +
                            reference.Date.ToString(
                                "yyyyMMdd",
                                System.Globalization.CultureInfo.InvariantCulture),
                            "Daily loss limit reached (" +
                            evaluation.LossPercent.ToString(
                                "F2",
                                System.Globalization.CultureInfo.InvariantCulture) +
                            "% >= " +
                            MaximumDailyLossPercent.ToString(
                                "F2",
                                System.Globalization.CultureInfo.InvariantCulture) +
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
            _dailyLossTransactionsAvailable = false;
            _dailyLossLocked = false;
            _dailyLossLimitAlerted = false;
            _dailyLossDataReady = false;
            _dailyLossEvaluationUtc = DateTime.MinValue;
            _lastDailyLossPersistUtc =
                DateTime.MinValue;

            PersistDailyLossState(
                referenceUtc);
        }
    }
}
