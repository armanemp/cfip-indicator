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
                
                            if (Symbol.TickSize > 0)
                            {
                                price =
                                    Math.Round(
                                        price /
                                        Symbol.TickSize,
                                        MidpointRounding.AwayFromZero) *
                                    Symbol.TickSize;
                            }
                
                            return Math.Round(
                                price,
                                Symbol.Digits);
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
