using System;
using System.Collections.Generic;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private HistoricalOutcomeAggregate AggregateHistoricalOutcome(
            Position position)
        {
            if (position == null ||
                position.Id <= 0 ||
                position.Id > int.MaxValue)
            {
                return HistoricalOutcomeAggregationRule
                    .FromPositionFallback(
                        position == null
                            ? 0
                            : position.NetProfit,
                        position == null
                            ? 0
                            : position.Pips);
            }

            try
            {
                HistoricalTrade[] historicalTrades =
                    History.FindByPositionId(
                        (int)position.Id);

                if (historicalTrades == null ||
                    historicalTrades.Length == 0)
                {
                    return HistoricalOutcomeAggregationRule
                        .FromPositionFallback(
                            position.NetProfit,
                            position.Pips);
                }

                List<HistoricalOutcomeRecord> records =
                    new List<HistoricalOutcomeRecord>(
                        historicalTrades.Length);

                for (int i = 0;
                     i < historicalTrades.Length;
                     i++)
                {
                    HistoricalTrade trade =
                        historicalTrades[i];

                    if (trade == null)
                        continue;

                    records.Add(
                        new HistoricalOutcomeRecord
                        {
                            NetProfit =
                                trade.NetProfit,
                            GrossProfit =
                                trade.GrossProfit,
                            Swap =
                                trade.Swap,
                            Commissions =
                                trade.Commissions,
                            Pips =
                                trade.Pips,
                            ClosingTime =
                                trade.ClosingTime
                        });
                }

                HistoricalOutcomeAggregate aggregate =
                    HistoricalOutcomeAggregationRule
                        .AggregateHistoricalOutcomeRecords(
                            records);

                return aggregate.Available
                    ? aggregate
                    : HistoricalOutcomeAggregationRule
                        .FromPositionFallback(
                            position.NetProfit,
                            position.Pips);
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP historical outcome aggregation failed for #{0}: {1}",
                    position.Id,
                    ex.Message);

                return HistoricalOutcomeAggregationRule
                    .FromPositionFallback(
                        position.NetProfit,
                        position.Pips);
            }
        }
    }
}
