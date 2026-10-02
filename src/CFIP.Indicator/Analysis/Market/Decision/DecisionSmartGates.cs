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
                RewardQualityFloorRule.Calculate(
                    decision.SmartQuality,
                    Math.Max(1.0, SmartTargetMinimumRR)) <
                MinimumProxyExpectedValue)
                return new DecisionFilterResult(false, "REWARD QUALITY FLOOR");

            RangeSignalQualityResult rangeQuality =
                EvaluateRangeSignalQuality(
                    closedM5,
                    decision.Direction,
                    decision.Confidence,
                    decision.SmartQuality,
                    decision.Edge,
                    decision.IndependentEvidence,
                    decision.StructuralConfirmations);

            if (!rangeQuality.Allowed)
                return new DecisionFilterResult(
                    false,
                    rangeQuality.Reason);

            MarketRegimeSnapshot activeRegime =
                UseStrictRegimeQualityGate ||
                UseRegimeNoTradeGuard
                    ? GetActiveM5Regime(closedM5)
                    : null;

            if (UseStrictRegimeQualityGate &&
                _m5Frame != null)
            {
                bool directionBull =
                    decision.Direction == 1;
                bool directionBear =
                    decision.Direction == -1;

                bool displacement =
                    directionBull
                        ? _m5Frame.DisplacementBull
                        : directionBear &&
                          _m5Frame.DisplacementBear;

                bool liquidity =
                    directionBull
                        ? _m5Frame.LiquidityBull
                        : directionBear &&
                          _m5Frame.LiquidityBear;

                bool volume =
                    directionBull
                        ? _m5Frame.VolumeBull
                        : directionBear &&
                          _m5Frame.VolumeBear;

                bool trend =
                    directionBull
                        ? _m5Frame.TrendBull
                        : directionBear &&
                          _m5Frame.TrendBear;

                bool structure =
                    directionBull
                        ? (_m5Frame.StructureBull ||
                           _m5Frame.MssBull ||
                           _m5Frame.ChochBull)
                        : directionBear &&
                          (_m5Frame.StructureBear ||
                           _m5Frame.MssBear ||
                           _m5Frame.ChochBear);

                MarketRegimeSnapshot regime =
                    activeRegime;

                if (regime != null &&
                    regime.Regime == "TREND" &&
                    (!trend ||
                     !structure ||
                     decision.TimeframeAgreement <
                     Math.Max(
                         SmartMinimumTimeframeAgreement,
                         MinimumTimeframeAgreement) ||
                     decision.StructuralConfirmations <
                     Math.Max(
                         MinimumStructuralConfirmations,
                         MinimumStructuralSequence)))
                {
                    bool tacticalRetestPath =
                        decision.TacticalOpportunityAllowed &&
                        decision.TacticalOpportunityQuality >=
                        TacticalOpportunityMinimumQuality &&
                        decision.TacticalOpportunityRR >=
                        TacticalOpportunityMinimumRR &&
                        decision.Direction == _m5Frame.Direction;

                    if (!tacticalRetestPath)
                        return new DecisionFilterResult(
                            false,
                            "TREND EVIDENCE");
                }

                if (regime != null &&
                    regime.Regime == "EXPANSION" &&
                    !(displacement &&
                      (volume ||
                       liquidity ||
                       structure)))
                {
                    return new DecisionFilterResult(
                        false,
                        "EXPANSION EVIDENCE");
                }

                if (regime != null &&
                    regime.Regime == "HIGH_VOLATILITY")
                {
                    int expansionEvidence =
                        (displacement ? 1 : 0) +
                        (volume ? 1 : 0) +
                        (liquidity ? 1 : 0) +
                        (structure ? 1 : 0);

                    if (expansionEvidence < 3 ||
                        decision.Edge <
                        Math.Max(
                            SmartStrongSetupEdge,
                            16))
                    {
                        return new DecisionFilterResult(
                            false,
                            "HIGH VOL EVIDENCE");
                    }
                }
            }

            if (UseStrictRegimeQualityGate)
            {
                MarketRegimeSnapshot regime =
                    activeRegime;

                if (regime != null)
                {
                    if (regime.Regime == "COMPRESSION")
                        return new DecisionFilterResult(
                            false,
                            "REGIME COMPRESSION");

                    if (regime.Quality <
                        Math.Max(
                            MinimumDirectionalRegimeQuality,
                            adaptiveQualityThreshold) &&
                        regime.Regime != "RANGE")
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

                    if (regime.Regime != "RANGE" &&
                        decision.Direction != 0 &&
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

            if (UseStrictRegimeQualityGate &&
                RequireRegimeStability)
            {
                MarketRegimeSnapshot regime =
                    GetActiveM5Regime(
                        closedM5);

                if (regime != null &&
                    (regime.Regime == "TREND" ||
                     regime.Regime == "EXPANSION") &&
                    regime.Stability <
                    Math.Max(
                        1,
                        MinimumRegimeStability))
                {
                    if (regime.Quality <
                        Math.Max(
                            MinimumDirectionalRegimeQuality + 5,
                            adaptiveQualityThreshold + 5))
                    {
                        return new DecisionFilterResult(
                            false,
                            "REGIME UNSTABLE");
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