using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private DecisionFilterResult EvaluateDecisionSmartGates(
            int closedM5,
            Decision decision,
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