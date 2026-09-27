using System;
using System.Collections.Generic;

namespace CFIP.Indicator
{
    public sealed class PredictionSnapshot
    {
        public Direction Direction { get; private set; }
        public int Confidence { get; private set; }
        public bool Eligible { get; private set; }
        public PriceLevel Entry { get; private set; }
        public PriceZone Zone { get; private set; }
        public PriceLevel Trigger { get; private set; }
        public PriceLevel Stop { get; private set; }
        public TargetLadder Targets { get; private set; }
        public int LookaheadBars { get; private set; }
        public Provenance Provenance { get; private set; }

        public PredictionSnapshot(
            Direction direction, int confidence, bool eligible,
            PriceLevel entry, PriceZone zone, PriceLevel trigger,
            PriceLevel stop, TargetLadder targets, int lookaheadBars,
            Provenance provenance)
        {
            Direction = direction;
            Confidence = Math.Max(0, Math.Min(100, confidence));
            Eligible = eligible;
            Entry = entry;
            Zone = zone;
            Trigger = trigger;
            Stop = stop;
            Targets = targets;
            LookaheadBars = Math.Max(0, lookaheadBars);
            Provenance = provenance ?? Provenance.Direct("PREDICTION", "UNSPECIFIED");
        }
    }

    public sealed class PredictionEngine
    {
        public PredictionSnapshot Evaluate(
            MarketModel market,
            EntrySnapshot entry,
            TradePlan plan,
            ConfigSnapshot configuration)
        {
            if (market == null || market.M5 == null || market.M15 == null ||
                !configuration.Get("EnableEarlyPrediction", true))
                return Empty(configuration);

            var m5 = market.M5;
            var m15 = market.M15;

            double buy = m5.BullScore * 0.55 + m15.BullScore * 0.45;
            double sell = m5.BearScore * 0.55 + m15.BearScore * 0.45;

            if (configuration.Get("UseLiquidityForecast", true))
            {
                if (m5.LiquidityBull) buy += 8;
                if (m5.LiquidityBear) sell += 8;
            }

            if (m5.VolumeRatio > 1.0)
            {
                if (m5.BiasDirection == Direction.Buy) buy += 2;
                if (m5.BiasDirection == Direction.Sell) sell += 2;
            }

            if (m5.Vwap > 0)
            {
                if (m5.Close > m5.Vwap) buy += 1;
                if (m5.Close < m5.Vwap) sell += 1;
            }

            var direction = buy >= sell ? Direction.Buy : Direction.Sell;
            double total = Math.Max(1, buy + sell);
            int confidence = Clamp((int)Math.Round(100.0 * Math.Max(buy, sell) / total), 0, 100);
            int minimum = Math.Max(
                configuration.Get("MinimumEarlyConfidence", 56),
                configuration.Get("EarlySetupConfidence", 52));

            if (confidence < minimum)
                return new PredictionSnapshot(
                    direction, confidence, false, null, null, null, null, null,
                    configuration.Get("PredictionLookaheadBars", 10),
                    Provenance.Direct("PREDICTION", "CONFIDENCE_BELOW_THRESHOLD"));

            if (plan != null && plan.IsValid)
                return new PredictionSnapshot(
                    direction, confidence, true,
                    plan.Entry.IdealEntry, plan.Entry.EntryZone, plan.Entry.Trigger,
                    plan.StructuralStop, plan.TargetLadder,
                    configuration.Get("PredictionLookaheadBars", 10),
                    Provenance.Direct("PREDICTION", "PLAN_ALIGNED"));

            if (entry != null && entry.Model != null)
                return new PredictionSnapshot(
                    direction, confidence, true,
                    entry.Model.IdealEntry, entry.Model.EntryZone, entry.Model.Trigger,
                    null, null, configuration.Get("PredictionLookaheadBars", 10),
                    Provenance.Direct("PREDICTION", "ENTRY_ALIGNED"));

            double atr = Math.Max(m5.Atr, 0);
            double center = m5.Close;
            var zone = atr > 0
                ? new PriceZone(
                    Math.Max(0, center - atr * 0.30),
                    center + atr * 0.30,
                    "PREDICTION_ZONE",
                    Provenance.Direct("PREDICTION", "ATR_ENVELOPE"))
                : null;
            var level = center > 0
                ? new PriceLevel(center, "PREDICTION_ENTRY", Provenance.Direct("PREDICTION", "CLOSE"))
                : null;
            var trigger = atr > 0
                ? new PriceLevel(
                    direction == Direction.Buy
                        ? zone.Upper + atr * configuration.Get("EntryBufferAtr", 0.05)
                        : zone.Lower - atr * configuration.Get("EntryBufferAtr", 0.05),
                    "PREDICTION_TRIGGER",
                    Provenance.Direct("PREDICTION", "ATR_TRIGGER"))
                : null;

            return new PredictionSnapshot(
                direction, confidence, level != null, level, zone, trigger, null, null,
                configuration.Get("PredictionLookaheadBars", 10),
                Provenance.Direct("PREDICTION", "MARKET_FORECAST"));
        }

        private PredictionSnapshot Empty(ConfigSnapshot configuration)
        {
            return new PredictionSnapshot(
                Direction.Wait, 0, false, null, null, null, null, null,
                configuration == null ? 0 : configuration.Get("PredictionLookaheadBars", 10),
                Provenance.Direct("PREDICTION", "NOT_READY"));
        }

        private int Clamp(int value, int min, int max)
        {
            return Math.Max(min, Math.Min(max, value));
        }
    }

    public sealed class MarketSuitabilitySnapshot
    {
        public int Score { get; private set; }
        public bool Eligible { get; private set; }
        public string Reason { get; private set; }
        public DateTime GeneratedUtc { get; private set; }

        public MarketSuitabilitySnapshot(int score, bool eligible, string reason, DateTime generatedUtc)
        {
            Score = Math.Max(0, Math.Min(100, score));
            Eligible = eligible;
            Reason = reason ?? string.Empty;
            GeneratedUtc = generatedUtc;
        }
    }

    public sealed class MarketSuitabilityEngine
    {
        public MarketSuitabilitySnapshot Evaluate(
            RuntimeSnapshot runtime,
            MarketModel market,
            ConfigSnapshot configuration)
        {
            DateTime now = runtime == null ? DateTime.MinValue : runtime.ServerUtc;
            if (runtime == null || market == null || market.M5 == null || market.M15 == null)
                return new MarketSuitabilitySnapshot(0, false, "MARKET DATA", now);

            if (!runtime.SymbolTradingEnabled)
                return new MarketSuitabilitySnapshot(0, false, "TRADING DISABLED", now);

            int score = 50;
            var m5 = market.M5;
            var m15 = market.M15;

            if (m5.DataValid && m15.DataValid) score += 10;
            if (m5.HealthyVolatility && m15.HealthyVolatility) score += 10;
            if (!m5.Choppy && !m15.Choppy) score += 10;
            if (m5.RegimeQuality >= 60 && m15.RegimeQuality >= 60) score += 10;

            if (runtime.SpreadPips > 0 && m5.Atr > 0)
            {
                double spreadPrice = runtime.SpreadPips * runtime.PipSize;
                if (spreadPrice <= m5.Atr * 0.10) score += 10;
                else if (spreadPrice >= m5.Atr * configuration.Get("MaximumSpreadAtr", 0.20))
                    score -= 20;
            }

            score = Math.Max(0, Math.Min(100, score));
            int minimum = configuration.Get("MinimumMarketSuitability", 68);
            bool enabled = configuration.Get("EnableMarketSuitabilityGuard", true);
            bool eligible = !enabled || score >= minimum;
            return new MarketSuitabilitySnapshot(score, eligible, eligible ? "OK" : "SUITABILITY", now);
        }
    }
}
