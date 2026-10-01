namespace cAlgo
{
    internal readonly struct DecisionScoreInput
    {
        public DecisionFrameContribution M5Contribution { get; }
        public DecisionFrameContribution M15Contribution { get; }
        public DecisionFrameContribution M30Contribution { get; }
        public DecisionFrameContribution H1Contribution { get; }
        public DecisionFrameContribution H4Contribution { get; }
        public DecisionFrameContribution D1Contribution { get; }
        public DecisionFrameContribution W1Contribution { get; }

        public bool SmartWeeklyContext { get; }
        public bool UseAdvancedConfluence { get; }
        public double AdvancedConfluenceBuy { get; }
        public double AdvancedConfluenceSell { get; }
        public bool UsePremiumDiscount { get; }
        public int PremiumDiscountBias { get; }
        public bool AdaptiveRegimeWeighting { get; }
        public bool UseHistoricalChoppinessGuard { get; }
        public int M5Direction { get; }
        public int M5IndicatorConfluenceQuality { get; }
        public int M5IndicatorConflict { get; }
        public bool M5Choppy { get; }
        public bool M15Choppy { get; }

        public DecisionScoreInput(
            DecisionFrameContribution m5Contribution,
            DecisionFrameContribution m15Contribution,
            DecisionFrameContribution m30Contribution,
            DecisionFrameContribution h1Contribution,
            DecisionFrameContribution h4Contribution,
            DecisionFrameContribution d1Contribution,
            DecisionFrameContribution w1Contribution,
            bool smartWeeklyContext,
            bool useAdvancedConfluence,
            double advancedConfluenceBuy,
            double advancedConfluenceSell,
            bool usePremiumDiscount,
            int premiumDiscountBias,
            bool adaptiveRegimeWeighting,
            bool useHistoricalChoppinessGuard,
            int m5Direction,
            int m5IndicatorConfluenceQuality,
            int m5IndicatorConflict,
            bool m5Choppy,
            bool m15Choppy)
        {
            M5Contribution = m5Contribution;
            M15Contribution = m15Contribution;
            M30Contribution = m30Contribution;
            H1Contribution = h1Contribution;
            H4Contribution = h4Contribution;
            D1Contribution = d1Contribution;
            W1Contribution = w1Contribution;
            SmartWeeklyContext = smartWeeklyContext;
            UseAdvancedConfluence = useAdvancedConfluence;
            AdvancedConfluenceBuy = advancedConfluenceBuy;
            AdvancedConfluenceSell = advancedConfluenceSell;
            UsePremiumDiscount = usePremiumDiscount;
            PremiumDiscountBias = premiumDiscountBias;
            AdaptiveRegimeWeighting = adaptiveRegimeWeighting;
            UseHistoricalChoppinessGuard = useHistoricalChoppinessGuard;
            M5Direction = m5Direction;
            M5IndicatorConfluenceQuality = m5IndicatorConfluenceQuality;
            M5IndicatorConflict = m5IndicatorConflict;
            M5Choppy = m5Choppy;
            M15Choppy = m15Choppy;
        }
    }
}
