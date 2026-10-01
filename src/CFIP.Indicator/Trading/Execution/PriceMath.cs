using cAlgo.API;

// ============================================================================
// CFIP Indicator — PriceMath.cs
// ============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private double NormalizePrice(
                            double price)
                        {
                            if (!IsFinitePositive(price))
                                return 0;

                            CanonicalPriceSnapshot market =
                                GetCanonicalPriceSnapshot();

                            double tickSize =
                                market == null
                                    ? Symbol.TickSize
                                    : market.TickSize;

                            int digits =
                                market == null
                                    ? Symbol.Digits
                                    : market.Digits;

                            if (tickSize > 0)
                            {
                                price =
                                    Math.Round(
                                        price /
                                        tickSize,
                                        MidpointRounding.AwayFromZero) *
                                    tickSize;
                            }

                            return Math.Round(
                                price,
                                digits);
                        }
        
        private string Price(
                            double value)
                        {
                            return
                                NormalizePrice(
                                    value)
                                .ToString(
                                    "F" +
                                    Symbol.Digits);
                        }
    }
}
