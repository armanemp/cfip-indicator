// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public sealed class CFIPClean89RuntimeSnapshot
        {
            public DateTime ServerUtc { get; private set; }
            public string Symbol { get; private set; }
            public double Bid { get; private set; }
            public double Ask { get; private set; }
            public double PipSize { get; private set; }
            public double SpreadPips { get; private set; }
            public bool SymbolTradingEnabled { get; private set; }
            public double Equity { get; private set; }
            public double FreeMargin { get; private set; }
            public double Balance { get; private set; }
            public double Margin { get; private set; }
            public double MarginLevel { get; private set; }
            public double DailyRealizedNetProfit { get; private set; }
            public DateTime TradingDayStartUtc { get; private set; }
            public CFIPClean89BrokerConstraints BrokerConstraints { get; private set; }
            public int ManagedPositionCount { get; private set; }
            public int ManagedPendingOrderCount { get; private set; }
    
            public CFIPClean89RuntimeSnapshot(
                DateTime serverUtc,
                string symbol,
                double bid,
                double ask,
                double pipSize,
                double spreadPips,
                bool symbolTradingEnabled,
                double equity,
                double freeMargin,
                double balance,
                double margin,
                double marginLevel,
                double dailyRealizedNetProfit,
                DateTime tradingDayStartUtc,
                CFIPClean89BrokerConstraints brokerConstraints,
                int managedPositionCount,
                int managedPendingOrderCount)
            {
                if (bid < 0 || ask < 0)
                    throw new ArgumentOutOfRangeException("bid");
    
                ServerUtc = serverUtc;
                Symbol = symbol ?? string.Empty;
                Bid = bid;
                Ask = ask;
                PipSize = Math.Max(0, pipSize);
                SpreadPips = Math.Max(0, spreadPips);
                SymbolTradingEnabled = symbolTradingEnabled;
                Equity = Math.Max(0, equity);
                FreeMargin = Math.Max(0, freeMargin);
                Balance = Math.Max(0, balance);
                Margin = Math.Max(0, margin);
                MarginLevel = Math.Max(0, marginLevel);
                DailyRealizedNetProfit = dailyRealizedNetProfit;
                TradingDayStartUtc = tradingDayStartUtc;
                BrokerConstraints =
                    brokerConstraints ??
                    throw new ArgumentNullException("brokerConstraints");
                ManagedPositionCount =
                    Math.Max(0, managedPositionCount);
                ManagedPendingOrderCount =
                    Math.Max(0, managedPendingOrderCount);
            }
        }
}
