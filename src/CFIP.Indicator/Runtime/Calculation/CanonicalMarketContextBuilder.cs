using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private CalculationMarketContext BuildCalculationMarketContext(
            int hostIndex,
            DateTime signalReference,
            MtfClosedContext closedBars)
        {
            if (closedBars == null ||
                Symbol == null)
                return null;

            DateTime quoteObservedUtc =
                Server.TimeInUtc;

            BrokerDistanceUnit distanceUnit =
                Symbol.MinDistanceType ==
                    SymbolMinDistanceType.Pips
                    ? BrokerDistanceUnit.Pips
                    : Symbol.MinDistanceType ==
                        SymbolMinDistanceType.Percentage
                        ? BrokerDistanceUnit.Percentage
                        : BrokerDistanceUnit.Unknown;

            CanonicalPriceSnapshot price =
                CanonicalPriceSnapshot.CreateCanonicalSnapshot(
                    quoteObservedUtc,
                    Symbol.Bid,
                    Symbol.Ask,
                    Symbol.PipSize,
                    Symbol.TickSize,
                    Symbol.Digits,
                    distanceUnit,
                    Symbol.MinStopLossDistance,
                    Symbol.MinTakeProfitDistance);

            return new CalculationMarketContext(
                hostIndex,
                signalReference,
                quoteObservedUtc,
                price,
                closedBars);
        }

        private void RefreshCanonicalMarketQuote()
        {
            if (_calculationMarketContext == null ||
                _lastMtfClosedContext == null)
                return;

            _calculationMarketContext =
                BuildCalculationMarketContext(
                    Bars == null
                        ? -1
                        : Bars.Count - 1,
                    _calculationMarketContext.SignalReferenceUtc,
                    _lastMtfClosedContext);
        }

        private CanonicalPriceSnapshot GetCanonicalPriceSnapshot()
        {
            if (_calculationMarketContext != null &&
                _calculationMarketContext.Price != null)
                return _calculationMarketContext.Price;

            MtfClosedContext closedBars =
                _lastMtfClosedContext;

            if (closedBars == null)
                return null;

            _calculationMarketContext =
                BuildCalculationMarketContext(
                    Bars == null
                        ? -1
                        : Bars.Count - 1,
                    Server.TimeInUtc,
                    closedBars);

            return _calculationMarketContext == null
                ? null
                : _calculationMarketContext.Price;
        }
    }
}