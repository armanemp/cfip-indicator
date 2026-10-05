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
        public bool IsPriceMetadataValid { get; }
        public bool IsObservationTimeValid { get; }
        public bool IsUsableForExecution { get; }
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
            ObservedUtc =
                observedUtc.Kind == DateTimeKind.Utc
                    ? observedUtc
                    : DateTime.SpecifyKind(
                        observedUtc,
                        DateTimeKind.Utc);
            Bid = bid;
            Ask = ask;
            PipSize = pipSize;
            TickSize = tickSize;
            Digits = Math.Max(0, digits);
            BrokerDistanceUnit = brokerDistanceUnit;
            MinimumStopDistanceRaw = minimumStopDistanceRaw;
            MinimumTakeProfitDistanceRaw = minimumTakeProfitDistanceRaw;

            IsPriceMetadataValid =
                IsFinitePositiveCanonicalPrice(pipSize) &&
                IsFinitePositiveCanonicalPrice(tickSize) &&
                Digits >= 0;

            IsObservationTimeValid =
                ObservedUtc != DateTime.MinValue &&
                ObservedUtc.Kind == DateTimeKind.Utc;

            IsQuoteValid =
                IsPriceMetadataValid &&
                IsObservationTimeValid &&
                IsFinitePositiveCanonicalPrice(bid) &&
                IsFinitePositiveCanonicalPrice(ask) &&
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
                IsFinitePositiveCanonicalPrice(pipSize)
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

            IsUsableForExecution =
                IsQuoteValid &&
                IsPriceMetadataValid &&
                IsObservationTimeValid;

            HasBrokerDistanceMetadata =
                BrokerDistanceUnit != BrokerDistanceUnit.Unknown &&
                IsFiniteNonNegativeCanonicalDistance(
                    MinimumStopDistanceRaw) &&
                IsFiniteNonNegativeCanonicalDistance(
                    MinimumTakeProfitDistanceRaw) &&
                (BrokerDistanceUnit != BrokerDistanceUnit.Pips ||
                 IsFinitePositiveCanonicalPrice(pipSize));
        }

        public static CanonicalPriceSnapshot CreateCanonicalSnapshot(
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
            if (!IsUsableForExecution)
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
                !IsFinitePositiveCanonicalPrice(referencePrice))
                return 0;

            if (BrokerDistanceUnit == BrokerDistanceUnit.Pips)
                return PipSize > 0
                    ? raw * PipSize
                    : 0;

            if (BrokerDistanceUnit == BrokerDistanceUnit.Percentage)
                return referencePrice * raw / 100.0;

            return 0;
        }


        private static bool IsFinitePositiveCanonicalPrice(double value)
        {
            return !double.IsNaN(value) &&
                   !double.IsInfinity(value) &&
                   value > 0;
        }

        private static bool IsFiniteNonNegativeCanonicalDistance(double value)
        {
            return !double.IsNaN(value) &&
                   !double.IsInfinity(value) &&
                   value >= 0;
        }
    }
}