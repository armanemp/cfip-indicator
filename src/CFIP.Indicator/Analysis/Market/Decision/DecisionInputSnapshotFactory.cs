using System;

namespace cAlgo
{
    internal sealed class DecisionInputSnapshotFactory
    {
        private readonly FrameDecisionContributionAdapter _contributionCalculator;

        public DecisionInputSnapshotFactory()
        {
            _contributionCalculator =
                new FrameDecisionContributionAdapter();
        }

        public DecisionInputSnapshot Create(
            DecisionInputBuildRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            return new DecisionInputSnapshot(
                request.M1Frame,
                request.M5Frame,
                request.M15Frame,
                request.M30Frame,
                request.H1Frame,
                request.H4Frame,
                request.D1Frame,
                request.W1Frame,
                _contributionCalculator.Calculate(
                    request.M5Frame,
                    request.M5Weight),
                _contributionCalculator.Calculate(
                    request.M15Frame,
                    request.M15Weight),
                _contributionCalculator.Calculate(
                    request.M30Frame,
                    request.M30Weight),
                _contributionCalculator.Calculate(
                    request.H1Frame,
                    request.H1Weight),
                _contributionCalculator.Calculate(
                    request.H4Frame,
                    request.H4Weight),
                _contributionCalculator.Calculate(
                    request.D1Frame,
                    request.D1Weight),
                _contributionCalculator.Calculate(
                    request.W1Frame,
                    request.W1Weight),
                request.SmartWeeklyContext,
                request.UseAdvancedConfluence,
                request.AdvancedConfluenceBuy,
                request.AdvancedConfluenceSell,
                request.UsePremiumDiscount,
                request.PremiumDiscountBias,
                request.UseM1Trigger,
                request.Regime,
                request.AdaptiveRegimeWeighting,
                request.UseHistoricalChoppinessGuard,
                request.SmartScoreTemperature,
                request.MinimumSmartDirectionShare,
                request.Reference,
                request.ClosedM5,
                request.Evidence);
        }
    }
}
