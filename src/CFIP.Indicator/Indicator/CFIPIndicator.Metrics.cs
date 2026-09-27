using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CFIP.Indicator;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
                        private int CountManagedPositions()
                {
                    if (_configuration == null)
                        return 0;
        
                    string label =
                        _configuration.Get(
                            "AutoTradeLabel",
                            "CFIP-SMART");
        
                    int count = 0;
                    foreach (var position in Positions)
                        if (string.Equals(position.Label, label, StringComparison.Ordinal) &&
                            string.Equals(position.SymbolName, SymbolName, StringComparison.Ordinal) &&
                            HasCurrentIdentity(position.Comment))
                            count++;
                    return count;
                }
        
                private int CountManagedPendingOrders()
                {
                    if (_configuration == null)
                        return 0;
        
                    string label =
                        _configuration.Get(
                            "AutoTradeLabel",
                            "CFIP-SMART");
        
                    int count = 0;
                    foreach (var order in PendingOrders)
                        if (string.Equals(order.Label, label, StringComparison.Ordinal) &&
                            string.Equals(order.SymbolName, SymbolName, StringComparison.Ordinal) &&
                            HasCurrentIdentity(order.Comment))
                            count++;
                    return count;
                }
        
                private double CalculateDailyRealizedNetProfit(DateTime dayStartUtc)
                {
                    if (_configuration == null)
                        return 0;
        
                    string label =
                        _configuration.Get(
                            "AutoTradeLabel",
                            "CFIP-SMART");
        
                    double total = 0;
                    HistoricalTrade[] trades =
                        History.FindAll(
                            label,
                            SymbolName);
        
                    if (trades == null)
                        return 0;
        
                    for (int i = 0; i < trades.Length; i++)
                    {
                        HistoricalTrade trade = trades[i];
        
                        if (trade.ClosingTime >= dayStartUtc &&
                            trade.ClosingTime < dayStartUtc.AddDays(1) &&
                            HasCurrentIdentity(trade.Comment))
                            total += trade.NetProfit;
                    }
        
                    return total;
                }
        
                private bool IsDailyLossLimitBreached()
                {
                    if (_configuration == null ||
                        _state == null ||
                        _state.Runtime == null ||
                        !_configuration.Get(
                            "EnableDailyLossLimit",
                            true))
                        return false;
        
                    double baseline =
                        _state.Runtime.Balance -
                        _state.Runtime.DailyRealizedNetProfit;
        
                    double limit =
                        Math.Max(
                            0,
                            baseline) *
                        Math.Max(
                            0,
                            _configuration.Get(
                                "MaximumDailyLossPercent",
                                3.0)) /
                        100.0;
        
                    return
                        _state.Runtime.DailyRealizedNetProfit < -limit;
                }
        
        
    }
}
