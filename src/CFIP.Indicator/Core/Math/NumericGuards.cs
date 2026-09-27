// ============================================================================
// CFIP Indicator — NumericGuards.cs
// One responsibility per module. Behavioral parity with v73 is preserved.
// ============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool IsFiniteValue(
                            double value)
                        {
                            return
                                !double.IsNaN(value) &&
                                !double.IsInfinity(value);
                        }
        
        private bool IsFinitePositive(
                            double value)
                        {
                            return
                                !double.IsNaN(value) &&
                                !double.IsInfinity(value) &&
                                value > 0;
                        }
        
        private double SafePositive(
                            double value)
                        {
                            return
                                IsFinitePositive(value)
                                    ? value
                                    : 0;
                        }
        
        private double ClampDouble(
                            double value,
                            double min,
                            double max)
                        {
                            if (double.IsNaN(value) ||
                                double.IsInfinity(value))
                                return min;
                
                            if (value < min)
                                return min;
                
                            if (value > max)
                                return max;
                
                            return value;
                        }
        
        private double Clamp(
                            double value,
                            double min,
                            double max)
                        {
                            if (value < min)
                                return min;
                
                            if (value > max)
                                return max;
                
                            return value;
                        }
        
        private int ClampInt(
                            int value,
                            int min,
                            int max)
                        {
                            if (value < min)
                                return min;
                
                            if (value > max)
                                return max;
                
                            return value;
                        }
    }
}
