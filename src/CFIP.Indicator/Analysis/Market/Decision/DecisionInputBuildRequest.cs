using System;

namespace cAlgo
{
    internal sealed class DecisionInputBuildRequest
    {
        public Frame M1Frame { get; set; }
        public Frame M5Frame { get; set; }
        public Frame M15Frame { get; set; }
        public Frame M30Frame { get; set; }
        public Frame H1Frame { get; set; }
        public Frame H4Frame { get; set; }
        public Frame D1Frame { get; set; }
        public Frame W1Frame { get; set; }

        public double M5Weight { get; set; }
        public double M15Weight { get; set; }
        public double M30Weight { get; set; }
        public double H1Weight { get; set; }
        public double H4Weight { get; set; }
        public double D1Weight { get; set; }
        public double W1Weight { get; set; }

        public bool SmartWeeklyContext { get; set; }
        public bool UseAdvancedConfluence { get; set; }
        public double AdvancedConfluenceBuy { get; set; }
        public double AdvancedConfluenceSell { get; set; }
        public bool UsePremiumDiscount { get; set; }
        public int PremiumDiscountBias { get; set; }
        public bool UseM1Trigger { get; set; }

        public string Regime { get; set; }
        public bool AdaptiveRegimeWeighting { get; set; }
        public bool UseHistoricalChoppinessGuard { get; set; }
        public double SmartScoreTemperature { get; set; }
        public int MinimumSmartDirectionShare { get; set; }

        public DateTime Reference { get; set; }
        public int ClosedM5 { get; set; }
        public MtfClosedContext ClosedContext { get; set; }
        public MarketStateSnapshot MarketStateSnapshot { get; set; }

        public DecisionEvidenceSnapshot Evidence { get; set; }
    }
}
