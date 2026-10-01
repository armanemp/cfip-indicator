namespace cAlgo
{
    internal readonly struct DecisionScoreSnapshot
    {
        public double Buy { get; }
        public double Sell { get; }

        public double M5BullContribution { get; }
        public double M5BearContribution { get; }
        public double M15BullContribution { get; }
        public double M15BearContribution { get; }
        public double M30BullContribution { get; }
        public double M30BearContribution { get; }
        public double H1BullContribution { get; }
        public double H1BearContribution { get; }
        public double H4BullContribution { get; }
        public double H4BearContribution { get; }
        public double D1BullContribution { get; }
        public double D1BearContribution { get; }
        public double W1BullContribution { get; }
        public double W1BearContribution { get; }
        public double AdvancedConfluenceBuy { get; }
        public double AdvancedConfluenceSell { get; }
        public double PremiumDiscountBuy { get; }
        public double PremiumDiscountSell { get; }
        public double AdaptiveRegimeBuy { get; }
        public double AdaptiveRegimeSell { get; }
        public double ConflictPenaltyBuy { get; }
        public double ConflictPenaltySell { get; }
        public double ChoppinessFactor { get; }

        public DecisionScoreSnapshot(
            double buy,
            double sell,
            double m5BullContribution,
            double m5BearContribution,
            double m15BullContribution,
            double m15BearContribution,
            double m30BullContribution,
            double m30BearContribution,
            double h1BullContribution,
            double h1BearContribution,
            double h4BullContribution,
            double h4BearContribution,
            double d1BullContribution,
            double d1BearContribution,
            double w1BullContribution,
            double w1BearContribution,
            double advancedConfluenceBuy,
            double advancedConfluenceSell,
            double premiumDiscountBuy,
            double premiumDiscountSell,
            double adaptiveRegimeBuy,
            double adaptiveRegimeSell,
            double conflictPenaltyBuy,
            double conflictPenaltySell,
            double choppinessFactor)
        {
            Buy = buy;
            Sell = sell;
            M5BullContribution = m5BullContribution;
            M5BearContribution = m5BearContribution;
            M15BullContribution = m15BullContribution;
            M15BearContribution = m15BearContribution;
            M30BullContribution = m30BullContribution;
            M30BearContribution = m30BearContribution;
            H1BullContribution = h1BullContribution;
            H1BearContribution = h1BearContribution;
            H4BullContribution = h4BullContribution;
            H4BearContribution = h4BearContribution;
            D1BullContribution = d1BullContribution;
            D1BearContribution = d1BearContribution;
            W1BullContribution = w1BullContribution;
            W1BearContribution = w1BearContribution;
            AdvancedConfluenceBuy = advancedConfluenceBuy;
            AdvancedConfluenceSell = advancedConfluenceSell;
            PremiumDiscountBuy = premiumDiscountBuy;
            PremiumDiscountSell = premiumDiscountSell;
            AdaptiveRegimeBuy = adaptiveRegimeBuy;
            AdaptiveRegimeSell = adaptiveRegimeSell;
            ConflictPenaltyBuy = conflictPenaltyBuy;
            ConflictPenaltySell = conflictPenaltySell;
            ChoppinessFactor = choppinessFactor;
        }
    }
}
