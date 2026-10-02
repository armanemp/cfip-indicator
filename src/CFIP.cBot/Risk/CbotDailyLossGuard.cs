using System;
using System.Globalization;
using cAlgo.API;

namespace CFIP.cBot.Risk
{
    internal sealed class CbotDailyLossGuard
    {
        private const string Schema = "CFIP-CBOT-DL,1";

        public bool IsBlocked(
            Robot robot,
            bool enabled,
            double maximumDailyLossPercent,
            DateTime nowUtc,
            out double lossPercent,
            out string reason)
        {
            lossPercent = 0;
            reason = "DAILY LOSS DISABLED";

            if (!enabled)
                return false;

            if (robot == null ||
                robot.Account == null)
            {
                reason = "DAILY LOSS ACCOUNT UNAVAILABLE";
                return true;
            }

            if (maximumDailyLossPercent < 0 ||
                double.IsNaN(maximumDailyLossPercent) ||
                double.IsInfinity(maximumDailyLossPercent))
            {
                reason = "DAILY LOSS LIMIT INVALID";
                return true;
            }

            if (maximumDailyLossPercent == 0)
            {
                reason = "DAILY LOSS LIMIT DISABLED";
                return false;
            }

            DateTime dayStart =
                new DateTime(
                    nowUtc.Year,
                    nowUtc.Month,
                    nowUtc.Day,
                    0,
                    0,
                    0,
                    DateTimeKind.Utc);

            if (LoadLocked(robot, dayStart))
            {
                reason = "DAILY LOSS LIMIT LOCKED";
                lossPercent = maximumDailyLossPercent;
                return true;
            }

            if (!TryCalculateDailyNetPnl(
                    robot,
                    dayStart,
                    out double realized,
                    out double cashFlow,
                    out double currentUnrealized))
            {
                reason = "DAILY LOSS DATA UNAVAILABLE";
                return true;
            }

            double startBalance =
                robot.Account.Balance -
                realized -
                cashFlow;

            if (!FinitePositive(startBalance))
            {
                reason = "DAILY LOSS BASELINE UNAVAILABLE";
                return true;
            }

            double dailyNetPnl =
                realized +
                currentUnrealized;

            if (double.IsNaN(dailyNetPnl) ||
                double.IsInfinity(dailyNetPnl))
            {
                reason = "DAILY LOSS PNL UNAVAILABLE";
                return true;
            }

            double lossAmount =
                Math.Max(0, -dailyNetPnl);

            lossPercent =
                lossAmount /
                startBalance *
                100.0;

            if (lossPercent >= maximumDailyLossPercent)
            {
                PersistLocked(
                    robot,
                    dayStart,
                    true);

                reason =
                    "DAILY LOSS LIMIT REACHED • " +
                    lossPercent.ToString(
                        "F2",
                        CultureInfo.InvariantCulture) +
                    "%";

                return true;
            }

            reason =
                "DAILY LOSS OK • " +
                lossPercent.ToString(
                    "F2",
                    CultureInfo.InvariantCulture) +
                "%";

            return false;
        }

        private static bool TryCalculateDailyNetPnl(
            Robot robot,
            DateTime dayStart,
            out double realized,
            out double cashFlow,
            out double currentUnrealized)
        {
            realized = 0;
            cashFlow = 0;
            currentUnrealized =
                robot.Account.UnrealizedNetProfit;

            if (double.IsNaN(currentUnrealized) ||
                double.IsInfinity(currentUnrealized))
                return false;

            DateTime endExclusive =
                dayStart.AddDays(1);

            try
            {
                if (robot.History == null ||
                    robot.Transactions == null)
                    return false;

                foreach (HistoricalTrade trade in robot.History)
                {
                    if (trade == null)
                        continue;

                    DateTime close =
                        EnsureUtc(trade.ClosingTime);

                    if (close < dayStart ||
                        close >= endExclusive)
                        continue;

                    if (double.IsNaN(trade.NetProfit) ||
                        double.IsInfinity(trade.NetProfit))
                        continue;

                    realized += trade.NetProfit;
                }

                foreach (Transaction transaction in robot.Transactions)
                {
                    if (transaction == null)
                        continue;

                    DateTime time =
                        EnsureUtc(transaction.Time);

                    if (time < dayStart ||
                        time >= endExclusive)
                        continue;

                    if (double.IsNaN(transaction.Amount) ||
                        double.IsInfinity(transaction.Amount))
                        return false;

                    if (transaction.Type ==
                        TransactionType.Deposit)
                    {
                        cashFlow +=
                            Math.Abs(transaction.Amount);
                    }
                    else if (transaction.Type ==
                             TransactionType.Withdrawal)
                    {
                        cashFlow -=
                            Math.Abs(transaction.Amount);
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool LoadLocked(
            Robot robot,
            DateTime dayStart)
        {
            try
            {
                robot.LocalStorage.Reload(
                    LocalStorageScope.Device);

                string payload =
                    robot.LocalStorage.GetString(
                        Key(robot),
                        LocalStorageScope.Device);

                if (string.IsNullOrWhiteSpace(payload))
                    return false;

                string[] parts =
                    payload.Split('|');

                if (parts.Length < 3 ||
                    !string.Equals(
                        parts[0],
                        Schema,
                        StringComparison.Ordinal))
                    return false;

                long ticks =
                    long.Parse(
                        parts[1],
                        CultureInfo.InvariantCulture);

                bool locked =
                    parts[2] == "1";

                return
                    locked &&
                    new DateTime(
                        ticks,
                        DateTimeKind.Utc) == dayStart;
            }
            catch
            {
                return false;
            }
        }

        private static void PersistLocked(
            Robot robot,
            DateTime dayStart,
            bool locked)
        {
            try
            {
                robot.LocalStorage.SetString(
                    Key(robot),
                    Schema +
                    "|" +
                    dayStart.Ticks.ToString(
                        CultureInfo.InvariantCulture) +
                    "|" +
                    (locked ? "1" : "0"),
                    LocalStorageScope.Device);

                robot.LocalStorage.Flush(
                    LocalStorageScope.Device);
            }
            catch
            {
                // Failure to persist the lock does not grant execution.
            }
        }

        private static string Key(Robot robot)
        {
            string broker =
                robot.Account.BrokerName ?? "UNKNOWN";
            string user =
                robot.Account.UserId.ToString(
                    CultureInfo.InvariantCulture);
            string asset =
                robot.Account.Asset == null
                    ? "UNKNOWN"
                    : robot.Account.Asset.Name ?? "UNKNOWN";

            return
                "CFIP cBot DailyLoss " +
                broker +
                "|" +
                user +
                "|" +
                asset;
        }

        private static DateTime EnsureUtc(DateTime value)
        {
            return value.Kind == DateTimeKind.Utc
                ? value
                : value.ToUniversalTime();
        }

        private static bool FinitePositive(double value)
        {
            return !double.IsNaN(value) &&
                   !double.IsInfinity(value) &&
                   value > 0;
        }
    }
}
