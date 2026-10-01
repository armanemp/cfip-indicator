using System;

namespace cAlgo
{
    internal enum BrokerDistanceUnit
    {
        Unknown = 0,
        Pips = 1,
        Percentage = 2
    }

    internal sealed class CanonicalPriceSnapshot
    {
        public DateTime ObservedUtc { get; }
        public double Bid { get; }
        public double Ask { get; }
        public double Midpoint { get; }
        public double Spread { get; }
        public double SpreadPips { get; }
        public double ExecutableBuyPrice { get; }
        public double ExecutableSellPrice { get; }
        public double PipSize { get; }
        public double TickSize { get; }
        public int Digits { get; }
        public BrokerDistanceUnit BrokerDistanceUnit { get; }
        public double MinimumStopDistanceRaw { get; }
        public double MinimumTakeProfitDistanceRaw { get; }
        public bool IsQuoteValid { get; }
        public bool HasBrokerDistanceMetadata { get; }

        private CanonicalPriceSnapshot(
            DateTime observedUtc,
            double bid,
            double ask,
            double pipSize,
            double tickSize,
            int digits,
            BrokerDistanceUnit brokerDistanceUnit,
            double minimumStopDistanceRaw,
            double minimumTakeProfitDistanceRaw)
        {
            ObservedUtc = observedUtc;
            Bid = bid;
            Ask = ask;
            PipSize = pipSize;
            TickSize = tickSize;
            Digits = Math.Max(0, digits);
            BrokerDistanceUnit = brokerDistanceUnit;
            MinimumStopDistanceRaw = minimumStopDistanceRaw;
            MinimumTakeProfitDistanceRaw = minimumTakeProfitDistanceRaw;

            IsQuoteValid =
                IsFinitePositive(bid) &&
                IsFinitePositive(ask) &&
                ask >= bid;

            Midpoint =
                IsQuoteValid
                    ? (bid + ask) / 2.0
                    : 0;

            Spread =
                IsQuoteValid
                    ? ask - bid
                    : 0;

            SpreadPips =
                IsQuoteValid &&
                IsFinitePositive(pipSize)
                    ? Spread / pipSize
                    : 0;

            ExecutableBuyPrice =
                IsQuoteValid
                    ? ask
                    : 0;

            ExecutableSellPrice =
                IsQuoteValid
                    ? bid
                    : 0;

            HasBrokerDistanceMetadata =
                BrokerDistanceUnit != BrokerDistanceUnit.Unknown &&
                IsFiniteNonNegative(
                    MinimumStopDistanceRaw) &&
                IsFiniteNonNegative(
                    MinimumTakeProfitDistanceRaw) &&
                (BrokerDistanceUnit != BrokerDistanceUnit.Pips ||
                 IsFinitePositive(pipSize));
        }

        public static CanonicalPriceSnapshot Create(
            DateTime observedUtc,
            double bid,
            double ask,
            double pipSize,
            double tickSize,
            int digits,
            BrokerDistanceUnit brokerDistanceUnit,
            double minimumStopDistanceRaw,
            double minimumTakeProfitDistanceRaw)
        {
            return new CanonicalPriceSnapshot(
                observedUtc,
                bid,
                ask,
                pipSize,
                tickSize,
                digits,
                brokerDistanceUnit,
                minimumStopDistanceRaw,
                minimumTakeProfitDistanceRaw);
        }

        public double GetExecutablePrice(int direction)
        {
            if (!IsQuoteValid)
                return 0;

            return direction == 1
                ? ExecutableBuyPrice
                : direction == -1
                    ? ExecutableSellPrice
                    : 0;
        }

        public double GetMinimumStopDistancePrice(int direction)
        {
            return ResolveBrokerDistance(
                MinimumStopDistanceRaw,
                GetExecutablePrice(direction));
        }

        public double GetMinimumTakeProfitDistancePrice(int direction)
        {
            return ResolveBrokerDistance(
                MinimumTakeProfitDistanceRaw,
                GetExecutablePrice(direction));
        }

        private double ResolveBrokerDistance(
            double raw,
            double referencePrice)
        {
            if (!HasBrokerDistanceMetadata ||
                raw < 0 ||
                !IsFinitePositive(referencePrice))
                return 0;

            if (BrokerDistanceUnit == BrokerDistanceUnit.Pips)
                return PipSize > 0
                    ? raw * PipSize
                    : 0;

            if (BrokerDistanceUnit == BrokerDistanceUnit.Percentage)
                return referencePrice * raw / 100.0;

            return 0;
        }


        private static bool IsFinitePositive(double value)
        {
            return !double.IsNaN(value) &&
                   !double.IsInfinity(value) &&
                   value > 0;
        }

        private static bool IsFiniteNonNegative(double value)
        {
            return !double.IsNaN(value) &&
                   !double.IsInfinity(value) &&
                   value >= 0;
        }
    }
}
