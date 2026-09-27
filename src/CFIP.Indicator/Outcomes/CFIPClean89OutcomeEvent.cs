// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public sealed class CFIPClean89OutcomeEvent
        {
            public CFIPClean89TradeIdentity TradeIdentity { get; private set; }
            public double EntryPrice { get; private set; }
            public double ExitPrice { get; private set; }
            public double ResultAmount { get; private set; }
            public double ResultR { get; private set; }
            public double MaxFavorableExcursion { get; private set; }
            public double MaxAdverseExcursion { get; private set; }
            public CFIPClean89TargetStage HighestTargetStageReached { get; private set; }
            public string ExitReason { get; private set; }
            public DateTime EntryUtc { get; private set; }
            public DateTime ExitUtc { get; private set; }
    
            public CFIPClean89OutcomeEvent(
                CFIPClean89TradeIdentity tradeIdentity,
                double entryPrice,
                double exitPrice,
                double resultAmount,
                double resultR,
                double maxFavorableExcursion,
                double maxAdverseExcursion,
                CFIPClean89TargetStage highestTargetStageReached,
                string exitReason,
                DateTime entryUtc,
                DateTime exitUtc)
            {
                TradeIdentity =
                    tradeIdentity ??
                    throw new ArgumentNullException("tradeIdentity");
                EntryPrice = entryPrice;
                ExitPrice = exitPrice;
                ResultAmount = resultAmount;
                ResultR = resultR;
                MaxFavorableExcursion =
                    Math.Max(0, maxFavorableExcursion);
                MaxAdverseExcursion =
                    Math.Max(0, maxAdverseExcursion);
                HighestTargetStageReached =
                    highestTargetStageReached;
                ExitReason = exitReason ?? string.Empty;
                EntryUtc = entryUtc;
                ExitUtc = exitUtc;
            }
        }
}
