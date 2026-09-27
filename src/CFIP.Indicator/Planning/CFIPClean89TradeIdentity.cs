// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public sealed class CFIPClean89TradeIdentity
        {
            public string StrategyId { get; private set; }
            public string Symbol { get; private set; }
            public string PlanId { get; private set; }
            public string SignalId { get; private set; }
    
            public CFIPClean89TradeIdentity(
                string strategyId,
                string symbol,
                string planId,
                string signalId)
            {
                StrategyId = strategyId ?? string.Empty;
                Symbol = symbol ?? string.Empty;
                PlanId = planId ?? string.Empty;
                SignalId = signalId ?? string.Empty;
            }
        }
}
