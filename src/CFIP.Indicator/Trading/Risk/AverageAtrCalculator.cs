// CFIP Indicator — AverageAtrCalculator.cs
// Single-responsibility market risk/suitability module.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
private double AverageAtr(Bars bars, int index, int lookback)
                                {
                                    if (bars == null || index < 1)
                                        return 0;
                        
                                    int count =
                                        Math.Max(2, Math.Min(lookback, index));
                                    int start =
                                        Math.Max(1, index - count + 1);
                        
                                    double sum = 0;
                                    int samples = 0;
                        
                                    for (int i = start; i <= index; i++)
                                    {
                                        double value = Atr(bars, i);
                        
                                        if (!NumericGuards.IsFinitePositive(value))
                                            continue;
                        
                                        sum += value;
                                        samples++;
                                    }
                        
                                    return samples > 0 ? sum / samples : 0;
                                }
    }
}
