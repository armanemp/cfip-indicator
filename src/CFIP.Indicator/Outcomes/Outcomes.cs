using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CFIP.Indicator
{
        public sealed class OutcomeEvent
        {
            public TradeIdentity TradeIdentity { get; private set; }
            public Direction Direction { get; private set; }
            public double EntryPrice { get; private set; }
            public double ExitPrice { get; private set; }
            public double ResultAmount { get; private set; }
            public double ResultR { get; private set; }
            public double MaxFavorableExcursion { get; private set; }
            public double MaxAdverseExcursion { get; private set; }
            public TargetStage HighestTargetStageReached { get; private set; }
            public string ExitReason { get; private set; }
            public DateTime EntryUtc { get; private set; }
            public DateTime ExitUtc { get; private set; }
    
            public OutcomeEvent(
                TradeIdentity tradeIdentity,
                Direction direction,
                double entryPrice,
                double exitPrice,
                double resultAmount,
                double resultR,
                double maxFavorableExcursion,
                double maxAdverseExcursion,
                TargetStage highestTargetStageReached,
                string exitReason,
                DateTime entryUtc,
                DateTime exitUtc)
            {
                TradeIdentity =
                    tradeIdentity ??
                    throw new ArgumentNullException("tradeIdentity");
                if (direction == Direction.Wait)
                    throw new ArgumentException("Outcome direction must be directional.", "direction");
                Direction = direction;
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
    
        public interface IOutcomeRecorder
        {
            void Record(
                OutcomeEvent outcome);
        }
    
    
}
