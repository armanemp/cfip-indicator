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
