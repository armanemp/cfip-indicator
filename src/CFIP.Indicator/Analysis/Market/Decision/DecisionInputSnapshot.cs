using System;

namespace cAlgo
{
    internal sealed class DecisionInputSnapshot
    {
        public Frame M1Frame { get; }
        public Frame M5Frame { get; }
        public Frame M15Frame { get; }
        public Frame M30Frame { get; }
        public Frame H1Frame { get; }
        public Frame H4Frame { get; }
        public Frame D1Frame { get; }
        public Frame W1Frame { get; }

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
        public bool UseM1Trigger { get; }

        public string Regime { get; }
        public bool AdaptiveRegimeWeighting { get; }
        public bool UseHistoricalChoppinessGuard { get; }
        public double SmartScoreTemperature { get; }
        public int MinimumSmartDirectionShare { get; }

        public DateTime Reference { get; }
        public int ClosedM5 { get; }
        public MarketStateSnapshot MarketStateSnapshot { get; }

        public DecisionEvidenceSnapshot Evidence { get; }

        internal DecisionScoreInput ToDecisionScoreInput()
        {
            return new DecisionScoreInput(
                M5Contribution,
                M15Contribution,
                M30Contribution,
                H1Contribution,
                H4Contribution,
                D1Contribution,
                W1Contribution,
                SmartWeeklyContext,
                UseAdvancedConfluence,
                AdvancedConfluenceBuy,
                AdvancedConfluenceSell,
                UsePremiumDiscount,
                PremiumDiscountBias,
                AdaptiveRegimeWeighting,
                UseHistoricalChoppinessGuard,
                M5Frame == null ? 0 : M5Frame.Direction,
                M5Frame == null ? 0 : M5Frame.IndicatorConfluenceQuality,
                M5Frame == null ? 0 : M5Frame.IndicatorConflict,
                M5Frame != null && M5Frame.Choppy,
                M15Frame != null && M15Frame.Choppy);
        }

        public DecisionInputSnapshot(
            Frame m1Frame,
            Frame m5Frame,
            Frame m15Frame,
            Frame m30Frame,
            Frame h1Frame,
            Frame h4Frame,
            Frame d1Frame,
            Frame w1Frame,
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
            bool useM1Trigger,
            string regime,
            bool adaptiveRegimeWeighting,
            bool useHistoricalChoppinessGuard,
            double smartScoreTemperature,
            int minimumSmartDirectionShare,
            DateTime reference,
            int closedM5,
            MarketStateSnapshot marketStateSnapshot,
            DecisionEvidenceSnapshot evidence)
        {
            M1Frame = m1Frame;
            M5Frame = m5Frame;
            M15Frame = m15Frame;
            M30Frame = m30Frame;
            H1Frame = h1Frame;
            H4Frame = h4Frame;
            D1Frame = d1Frame;
            W1Frame = w1Frame;

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
            UseM1Trigger = useM1Trigger;

            Regime =
                string.IsNullOrWhiteSpace(regime)
                    ? "UNKNOWN"
                    : regime;

            AdaptiveRegimeWeighting = adaptiveRegimeWeighting;
            UseHistoricalChoppinessGuard = useHistoricalChoppinessGuard;
            SmartScoreTemperature = smartScoreTemperature;
            MinimumSmartDirectionShare = minimumSmartDirectionShare;
            Reference = reference;
            ClosedM5 = closedM5;
            MarketStateSnapshot =
                marketStateSnapshot ??
                throw new ArgumentNullException(nameof(marketStateSnapshot));

            Evidence = evidence ?? throw new ArgumentNullException(nameof(evidence));
        }
    }
}