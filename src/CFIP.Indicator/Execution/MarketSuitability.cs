using System;
using System.Globalization;

namespace CFIP.Indicator
{
    public enum SuitabilityReason
    {
        None = 0,
        DataUnavailable = 1,
        TradingDisabled = 2,
        MarketClosed = 3,
        SessionBlocked = 4,
        SessionUnsuitable = 5,
        FridayCutoff = 6,
        NewsBlackout = 7,
        VolatilityShock = 8,
        AtrUnavailable = 9,
        ScoreBelowThreshold = 10
    }

    public sealed class MarketSuitabilitySnapshot
    {
        public int Score { get; private set; }
        public bool Eligible { get; private set; }
        public SuitabilityReason ReasonCode { get; private set; }
        public string Reason { get; private set; }
        public double RiskMultiplier { get; private set; }
        public DateTime GeneratedUtc { get; private set; }

        public MarketSuitabilitySnapshot(
            int score,
            bool eligible,
            SuitabilityReason reasonCode,
            string reason,
            double riskMultiplier,
            DateTime generatedUtc)
        {
            Score = Math.Max(0, Math.Min(100, score));
            Eligible = eligible;
            ReasonCode = reasonCode;
            Reason = reason ?? string.Empty;
            RiskMultiplier = Math.Max(0.05, Math.Min(1.0, riskMultiplier));
            GeneratedUtc = generatedUtc;
        }
    }

    public sealed class MarketSuitabilityEngine
    {
        public MarketSuitabilitySnapshot Evaluate(
            RuntimeSnapshot runtime,
            MarketModel market,
            DecisionSnapshot decision,
            ConfigSnapshot configuration)
        {
            DateTime now = runtime == null
                ? DateTime.MinValue
                : runtime.ServerUtc;

            if (runtime == null ||
                market == null ||
                market.M5 == null ||
                market.M15 == null ||
                configuration == null)
                return Create(
                    0,
                    false,
                    SuitabilityReason.DataUnavailable,
                    "MARKET DATA",
                    0.05,
                    now,
                    configuration);

            if (!runtime.SymbolTradingEnabled)
                return Create(
                    0,
                    false,
                    SuitabilityReason.TradingDisabled,
                    "TRADING DISABLED",
                    0.05,
                    now,
                    configuration);

            if (!runtime.MarketOpen)
                return Create(
                    0,
                    false,
                    SuitabilityReason.MarketClosed,
                    "MARKET CLOSED",
                    0.05,
                    now,
                    configuration);

            if (IsSessionBlocked(now, configuration))
                return Create(
                    25,
                    false,
                    SuitabilityReason.SessionBlocked,
                    "SESSION FILTER",
                    0.25,
                    now,
                    configuration);

            bool insideSession = IsInsideSessionWindow(now, configuration);

            if (configuration.Get("RequireSessionSuitability", false) &&
                !insideSession)
                return Create(
                    30,
                    false,
                    SuitabilityReason.SessionUnsuitable,
                    "SESSION SUITABILITY",
                    0.30,
                    now,
                    configuration);

            if (IsFridayCutoff(now, configuration))
                return Create(
                    25,
                    false,
                    SuitabilityReason.FridayCutoff,
                    "FRIDAY CUTOFF",
                    0.25,
                    now,
                    configuration);

            string newsReason;
            if (NewsBlocked(now, configuration, out newsReason))
                return Create(
                    20,
                    false,
                    SuitabilityReason.NewsBlackout,
                    newsReason,
                    0.20,
                    now,
                    configuration);

            MarketFrame m5 = market.M5;
            MarketFrame m15 = market.M15;

            double atr = m5.Atr;
            if (atr <= 0)
                return Create(
                    0,
                    false,
                    SuitabilityReason.AtrUnavailable,
                    "ATR UNAVAILABLE",
                    0.05,
                    now,
                    configuration);

            if (IsVolatilityShock(m5, configuration))
                return Create(
                    25,
                    false,
                    SuitabilityReason.VolatilityShock,
                    "EVENT SHOCK / VOLATILITY",
                    0.25,
                    now,
                    configuration);

            Direction direction = decision == null
                ? m5.BiasDirection
                : decision.Direction;

            int score = 50;

            score += insideSession ? 10 : -8;

            double spreadRatio =
                Math.Max(0, runtime.Ask - runtime.Bid) /
                Math.Max(atr, runtime.PipSize);

            double maximumSpreadAtr = Math.Max(
                0,
                configuration.Get("MaximumSpreadAtr", 0.20));

            if (spreadRatio <= maximumSpreadAtr * 0.50)
                score += 18;
            else if (spreadRatio <= maximumSpreadAtr)
                score += 8;
            else
                score -= 22;

            double volatilityRatio = m5.AtrRatio;
            if (volatilityRatio >=
                    Math.Max(0.75, configuration.Get("HealthyAtrMinimumRatio", 0.85)) &&
                volatilityRatio <=
                    Math.Max(1.10, configuration.Get("HealthyAtrMaximumRatio", 1.80)))
                score += 12;
            else if (volatilityRatio < 0.60 || volatilityRatio > 2.25)
                score -= 15;
            else
                score += 4;

            if (direction != Direction.Wait)
            {
                score += DirectionContribution(m5.BiasDirection, direction, 9, -10);
                score += DirectionContribution(m15.BiasDirection, direction, 9, -10);

                MarketFrame m30 = market.FindFrame("M30");
                MarketFrame h1 = market.FindFrame("H1");
                MarketFrame h4 = market.FindFrame("H4");

                if (m30 != null)
                    score += DirectionContribution(m30.BiasDirection, direction, 5, -6);
                if (h1 != null)
                    score += DirectionContribution(h1.BiasDirection, direction, 4, -5);
                if (h4 != null)
                    score += DirectionContribution(h4.BiasDirection, direction, 3, -4);
            }

            double averageAdx = (m5.Adx + m15.Adx) / 2.0;
            if (averageAdx >= 25)
                score += 12;
            else if (averageAdx >= 20)
                score += 7;
            else if (averageAdx < 15)
                score -= 8;

            if (m5.Choppy && m15.Choppy)
                score -= 18;
            else if (m5.Choppy || m15.Choppy)
                score -= 7;

            if (decision != null)
            {
                if (decision.MtfAgreement >=
                    Math.Max(50, configuration.Get("MinimumTimeframeAgreement", 72)))
                    score += 8;
                else if (decision.MtfAgreement < 60)
                    score -= 8;

                if (decision.IndependentEvidence >=
                    Math.Max(1, configuration.Get("MinimumIndependentEvidence", 4)))
                    score += 5;
            }

            if (configuration.Get("UseDailyPivots", true))
                score += PivotContribution(runtime, configuration, direction);

            score = Math.Max(0, Math.Min(100, score));

            int minimum = Math.Max(
                50,
                configuration.Get("MinimumMarketSuitability", 68));

            bool hardGate = configuration.Get("HardMarketSuitabilityGate", true);
            bool guardEnabled = configuration.Get("EnableMarketSuitabilityGuard", true);
            bool eligible = !guardEnabled || !hardGate || score >= minimum;

            double riskMultiplier = CalculateRiskMultiplier(
                decision,
                m5,
                minimum,
                configuration,
                score);

            return new MarketSuitabilitySnapshot(
                score,
                eligible,
                score >= minimum ? SuitabilityReason.None : SuitabilityReason.ScoreBelowThreshold,
                score >= minimum
                    ? "SUITABILITY " + score
                    : "SUITABILITY " + score + " < " + minimum,
                riskMultiplier,
                now);
        }

        private MarketSuitabilitySnapshot Create(
            int score,
            bool eligible,
            SuitabilityReason reasonCode,
            string reason,
            double riskMultiplier,
            DateTime now,
            ConfigSnapshot configuration)
        {
            return new MarketSuitabilitySnapshot(
                score,
                eligible || configuration != null &&
                !configuration.Get("EnableMarketSuitabilityGuard", true),
                reasonCode,
                reason,
                riskMultiplier,
                now);
        }

        private static int DirectionContribution(
            Direction actual,
            Direction target,
            int aligned,
            int opposed)
        {
            if (actual == target)
                return aligned;
            if (actual == DirectionRules.Opposite(target))
                return opposed;
            return 0;
        }

        private static bool IsSessionBlocked(
            DateTime utc,
            ConfigSnapshot configuration)
        {
            return configuration.Get("UseSessionFilter", false) &&
                   !IsInsideSessionWindow(utc, configuration);
        }

        private static bool IsInsideSessionWindow(
            DateTime utc,
            ConfigSnapshot configuration)
        {
            int start =
                Math.Max(0, Math.Min(23, configuration.Get("SessionStartUtc", 6))) * 60;
            int end =
                Math.Max(0, Math.Min(23, configuration.Get("SessionEndUtc", 20))) * 60;
            int current = utc.Hour * 60 + utc.Minute;

            if (start == end)
                return true;

            return start < end
                ? current >= start && current < end
                : current >= start || current < end;
        }

        private static bool IsFridayCutoff(
            DateTime utc,
            ConfigSnapshot configuration)
        {
            return configuration.Get("AvoidFridayLateEntry", false) &&
                   utc.DayOfWeek == DayOfWeek.Friday &&
                   utc.Hour >=
                   Math.Max(0, Math.Min(23, configuration.Get("FridayCutoffUtc", 18)));
        }

        private static bool NewsBlocked(
            DateTime utc,
            ConfigSnapshot configuration,
            out string reason)
        {
            reason = string.Empty;

            if (!configuration.Get("UseNewsEventGuard", true))
                return false;

            string source = configuration.Get("NewsBlackoutUtc", string.Empty);
            if (string.IsNullOrWhiteSpace(source))
                return false;

            int current = utc.Hour * 60 + utc.Minute;
            string[] items = source.Split(
                new[] { ',', ';', '|' },
                StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < items.Length; i++)
            {
                string[] parts = items[i].Trim().Split('-');
                if (parts.Length != 2)
                    continue;

                int start;
                int end;
                if (!TryParseMinutes(parts[0], out start) ||
                    !TryParseMinutes(parts[1], out end))
                    continue;

                bool blocked = start <= end
                    ? current >= start && current <= end
                    : current >= start || current <= end;

                if (blocked)
                {
                    reason = "NEWS BLACKOUT";
                    return true;
                }
            }

            return false;
        }

        private static bool TryParseMinutes(string value, out int minutes)
        {
            minutes = 0;
            if (string.IsNullOrWhiteSpace(value))
                return false;

            string[] parts = value.Trim().Split(':');
            if (parts.Length != 2)
                return false;

            int hour;
            int minute;

            if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out hour) ||
                !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out minute))
                return false;

            if (hour < 0 || hour > 23 || minute < 0 || minute > 59)
                return false;

            minutes = hour * 60 + minute;
            return true;
        }

        private static bool IsVolatilityShock(
            MarketFrame m5,
            ConfigSnapshot configuration)
        {
            if (!configuration.Get("UseVolatilityEventGuard", true))
                return false;

            double shockRange = Math.Max(
                0,
                configuration.Get("EventShockRangeAtr", 2.20));
            double expansion = Math.Max(
                0,
                configuration.Get("EventShockAtrExpansion", 1.55));

            bool rangeShock =
                shockRange > 0 &&
                m5.RangeAtr >= shockRange;

            bool atrExpansion =
                expansion > 0 &&
                m5.AtrRatio >= expansion;

            return rangeShock || atrExpansion;
        }

        private static int PivotContribution(
            RuntimeSnapshot runtime,
            ConfigSnapshot configuration,
            Direction direction)
        {
            return 0;
        }

        private static double CalculateRiskMultiplier(
            DecisionSnapshot decision,
            MarketFrame m5,
            int minimumSuitability,
            ConfigSnapshot configuration,
            int score)
        {
            if (!configuration.Get("UseSmartRiskScaling", true))
                return 1.0;

            double floor = Math.Max(
                0.25,
                Math.Min(
                    1.0,
                    configuration.Get("MinimumSmartRiskMultiplier", 0.55)));

            double confidence = decision == null
                ? 0
                : Math.Max(0, Math.Min(100, decision.Confidence));

            double confidenceThreshold = Math.Max(
                70,
                configuration.Get("FullRiskConfidenceThreshold", 94));

            double suitabilityThreshold = Math.Max(
                60,
                configuration.Get("FullRiskSuitabilityThreshold", 88));

            double confidenceScale = Clamp01(
                (confidence - 70.0) /
                Math.Max(1.0, confidenceThreshold - 70.0));

            double suitabilityScale = Clamp01(
                score /
                suitabilityThreshold);

            double scale = floor +
                (1.0 - floor) *
                (0.60 * confidenceScale +
                 0.40 * suitabilityScale);

            if (configuration.Get("PenalizeChoppyRegimeRisk", true) &&
                m5.Choppy)
                scale *= 0.82;

            return Math.Max(floor, Math.Min(1.0, scale));
        }

        private static double Clamp01(double value)
        {
            return Math.Max(0, Math.Min(1, value));
        }
    }
}
