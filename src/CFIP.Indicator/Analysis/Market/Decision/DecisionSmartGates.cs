using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private DecisionFilterResult EvaluateDecisionSmartGates(
            Decision decision,
            int closedM5,
            int adaptiveQualityThreshold,
            int adaptiveShareThreshold)
        {
            DecisionFilterResult consensusResult =
                new DecisionSmartConsensusFilterEvaluator().Evaluate(
                    new DecisionSmartConsensusFilterInput(
                        EnableSmartDecisionEngine,
                        RequireSmartConsensus,
                        Math.Max(decision.BuyShare, decision.SellShare),
                        SmartConsensusThreshold,
                        adaptiveShareThreshold,
                        AllowSmartSoftGate,
                        decision.SmartQuality,
                        SmartStrongSetupQuality,
                        decision.Edge,
                        SmartStrongSetupEdge,
                        decision.IndependentEvidence,
                        SmartMinimumIndependentEvidence,
                        decision.TimeframeAgreement,
                        SmartMinimumTimeframeAgreement));

            if (!consensusResult.Allowed)
                return consensusResult;

            if (UseSmartEntryQualityFilter &&
                decision.SmartQuality <
                Math.Max(
                    SmartQualityThreshold,
                    Math.Max(
                        adaptiveQualityThreshold,
                        EnableSmartDecisionEngine
                            ? SmartMinimumConsensusFloor()
                            : 0)))
                return new DecisionFilterResult(false, "SMART QUALITY");

            if (UseProxyExpectedValueGate &&
                ProxyExpectedValue(
                    decision.SmartQuality,
                    Math.Max(1.0, SmartTargetMinimumRR)) <
                MinimumProxyExpectedValue)
                return new DecisionFilterResult(false, "EXPECTED VALUE");

            if (UseStrictRegimeQualityGate)
            {
                MarketRegimeSnapshot regime =
                    GetActiveM5Regime(
                        closedM5);

                if (regime != null)
                {
                    if (regime.Regime == "COMPRESSION" ||
                        regime.Regime == "RANGE")
                        return new DecisionFilterResult(
                            false,
                            "REGIME " +
                            regime.Regime);

                    if (regime.Quality <
                        Math.Max(
                            MinimumDirectionalRegimeQuality,
                            adaptiveQualityThreshold))
                    {
                        return new DecisionFilterResult(
                            false,
                            "REGIME QUALITY");
                    }

                    if ((regime.Regime == "TRANSITION" ||
                         regime.Regime == "HIGH_VOLATILITY") &&
                        (regime.Quality <
                            MinimumDirectionalRegimeQuality + 5 ||
                         decision.Edge <
                            Math.Max(
                                SmartStrongSetupEdge - 2,
                                0)))
                    {
                        return new DecisionFilterResult(
                            false,
                            "REGIME TRANSITION");
                    }

                    if (decision.Direction != 0 &&
                        regime.Direction != 0 &&
                        regime.Direction != decision.Direction &&
                        regime.Quality <
                            Math.Max(
                                72,
                                MinimumDirectionalRegimeQuality + 8))
                    {
                        return new DecisionFilterResult(
                            false,
                            "REGIME DIRECTION CONFLICT");
                    }
                }
            }

            if (UseRegimeNoTradeGuard &&
                NoTradeRegimeBlocked(
                    decision.Regime,
                    decision.SmartQuality))
                return new DecisionFilterResult(false, "REGIME NO-TRADE");

            if (UseHistoricalChoppinessGuard &&
                _m5Frame != null &&
                _m15Frame != null &&
                _m5Frame.Choppy &&
                _m15Frame.Choppy &&
                decision.SmartQuality <
                Math.Max(
                    SmartRegimeQualityFloor + 5,
                    NoTradeMinimumSmartQuality + 5))
                return new DecisionFilterResult(false, "CHOP");

            return new DecisionFilterResult(true, string.Empty);
        }
    }
}