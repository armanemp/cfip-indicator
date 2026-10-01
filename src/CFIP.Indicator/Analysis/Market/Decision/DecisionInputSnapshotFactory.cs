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

            ValidateClosedBarAlignment(
                request);

            ValidateMarketStateSnapshot(
                request);

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
                request.MarketStateSnapshot,
                request.Evidence);
        }

        private static void ValidateMarketStateSnapshot(
            DecisionInputBuildRequest request)
        {
            MarketStateSnapshot snapshot =
                request.MarketStateSnapshot;

            if (snapshot == null ||
                !snapshot.MatchesReference(request.Reference) ||
                !snapshot.IsAlignedWithClosedIndices(
                    request.ClosedContext.M1,
                    request.ClosedContext.M5,
                    request.ClosedContext.M15,
                    request.ClosedContext.M30,
                    request.ClosedContext.H1,
                    request.ClosedContext.H4,
                    request.ClosedContext.D1,
                    request.ClosedContext.W1))
                throw new InvalidOperationException(
                    "Decision input is not bound to the canonical market-state snapshot.");
        }
        private static void ValidateClosedBarAlignment(
            DecisionInputBuildRequest request)
        {
            MtfClosedContext context =
                request.ClosedContext;

            if (context == null ||
                context.Reference != request.Reference ||
                context.M5 != request.ClosedM5 ||
                !context.HasPrimaryDecisionHistory)
                throw new InvalidOperationException(
                    "Decision input is not bound to the canonical closed-bar context.");

            ValidateRequiredFrame(
                "M5",
                request.M5Frame,
                context.M5);

            ValidateRequiredFrame(
                "M15",
                request.M15Frame,
                context.M15);

            ValidateRequiredFrame(
                "M30",
                request.M30Frame,
                context.M30);

            ValidateRequiredFrame(
                "H1",
                request.H1Frame,
                context.H1);

            ValidateRequiredFrame(
                "H4",
                request.H4Frame,
                context.H4);

            ValidateOptionalFrame(
                "M1",
                request.M1Frame,
                context.M1);

            ValidateOptionalFrame(
                "D1",
                request.D1Frame,
                context.D1);

            ValidateOptionalFrame(
                "W1",
                request.W1Frame,
                context.W1);
        }

        private static void ValidateRequiredFrame(
            string name,
            Frame frame,
            int closedIndex)
        {
            if (frame == null ||
                frame.Index != closedIndex)
                throw new InvalidOperationException(
                    "Decision input " +
                    name +
                    " is not aligned to its closed-bar index.");
        }

        private static void ValidateOptionalFrame(
            string name,
            Frame frame,
            int closedIndex)
        {
            if (frame != null &&
                frame.Index != closedIndex)
                throw new InvalidOperationException(
                    "Decision input " +
                    name +
                    " is not aligned to its closed-bar index.");
        }
    }
}
