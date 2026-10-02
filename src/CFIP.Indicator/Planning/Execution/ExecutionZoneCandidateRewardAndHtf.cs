using System;
using System.Collections.Generic;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void AddHigherTimeframeStructureCandidates(
            List<ExecutionZoneSelectionCandidate> candidates,
            double market,
            int direction,
            int index,
            double frameAtr,
            string timeframe,
            int sourceQuality)
        {
            if (candidates == null ||
                _m5Bars == null ||
                index < 10 ||
                !IsFinitePositive(frameAtr) ||
                (direction != 1 &&
                 direction != -1))
                return;

            Bars bars =
                string.Equals(
                    timeframe,
                    "M15",
                    StringComparison.OrdinalIgnoreCase)
                    ? _m15Bars
                    : _h1Bars;

            double structuralLevel =
                direction == 1
                    ? FindSwingLowBelow(
                        bars,
                        index,
                        market)
                    : FindSwingHighAbove(
                        bars,
                        index,
                        market);

            if (!IsFinitePositive(structuralLevel))
                return;

            double width =
                Math.Max(
                    Symbol.TickSize,
                    frameAtr * 0.15);

            double low =
                direction == 1
                    ? structuralLevel
                    : structuralLevel - width;

            double high =
                direction == 1
                    ? structuralLevel + width
                    : structuralLevel;

            int quality =
                Math.Max(
                    55,
                    Math.Min(
                        100,
                        sourceQuality));

            AddRawStructureCandidate(
                candidates,
                market,
                Math.Max(
                    Symbol.TickSize,
                    frameAtr),
                low,
                high,
                timeframe + " STRUCTURE",
                true,
                quality);
        }

        private void AddRawStructureCandidate(
            List<ExecutionZoneSelectionCandidate> candidates,
            double market,
            double atr,
            double low,
            double high,
            string source,
            bool primary,
            int quality)
        {
            if (candidates == null ||
                !IsFinitePositive(atr) ||
                !IsFinitePositive(low) ||
                !IsFinitePositive(high) ||
                high <= low)
                return;

            double distance =
                DistanceToRawZone(
                    market,
                    low,
                    high);

            ExecutionZoneSelectionResult score =
                ExecutionZoneSelectionRule.Evaluate(
                    new ExecutionZoneSelectionInput(
                        quality,
                        distance /
                        Math.Max(
                            Symbol.TickSize,
                            atr),
                        0,
                        primary,
                        false,
                        false,
                        false,
                        false),
                    Math.Max(
                        1,
                        MaximumZoneAgeBars),
                    3.0);

            if (!score.Valid)
                return;

            candidates.Add(
                new ExecutionZoneSelectionCandidate
                {
                    Low = low,
                    High = high,
                    Source = source,
                    Quality = quality,
                    Score = score.Score,
                    Primary = primary,
                    Fvg = false,
                    OrderBlock = false,
                    Confluence = false
                });
        }

        private double DistanceToRawZone(
            double market,
            double low,
            double high)
        {
            if (market < low)
                return low - market;

            if (market > high)
                return market - high;

            return 0;
        }

        private void ApplyRewardPathPreferences(
            List<ExecutionZoneSelectionCandidate> candidates,
            int closedM5,
            int direction,
            double atr)
        {
            if (candidates == null ||
                candidates.Count == 0 ||
                !IsFinitePositive(atr))
                return;

            double requiredRR =
                Math.Max(
                    Tp1MinimumRR,
                    MinimumRequiredRRForRegime(
                        _decision == null
                            ? "UNKNOWN"
                            : _decision.Regime));

            for (int i = 0;
                 i < candidates.Count;
                 i++)
            {
                ExecutionZoneSelectionCandidate candidate =
                    candidates[i];

                if (candidate == null)
                    continue;

                double ideal =
                    NormalizePrice(
                        (candidate.Low + candidate.High) * 0.50);

                double stop =
                    BuildStructuralStop(
                        closedM5,
                        direction,
                        ideal,
                        atr,
                        out _,
                        out int stopQuality);

                double rewardPathRR =
                    IsValidStop(
                        direction,
                        ideal,
                        stop)
                        ? EstimateBestTp1RRForStop(
                            closedM5,
                            direction,
                            ideal,
                            stop,
                            atr)
                        : 0;

                candidate.StopQuality =
                    Math.Max(
                        0,
                        Math.Min(
                            100,
                            stopQuality));

                candidate.RewardPathRR =
                    Math.Max(
                        0,
                        rewardPathRR);

                candidate.Score =
                    ExecutionZoneSelectionRule.ApplyRewardPathPreference(
                        candidate.Score,
                        candidate.RewardPathRR,
                        requiredRR,
                        candidate.StopQuality);
            }
        }
    }
}
