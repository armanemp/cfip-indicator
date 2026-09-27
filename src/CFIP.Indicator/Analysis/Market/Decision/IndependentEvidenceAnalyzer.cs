// CFIP Indicator — IndependentEvidenceAnalyzer.cs
// Single-responsibility decision evidence module.

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
        private int IndependentEvidence(
                                    int direction)
                                {
                                    if (_m5Frame == null ||
                                        direction == 0)
                                        return 0;
                        
                                    int count = 0;
                        
                                    if (direction == 1)
                                    {
                                        if (_m5Frame.StructureBull) count++;
                                        if (_m5Frame.LiquidityBull) count++;
                                        if (_m5Frame.FvgBull) count++;
                                        if (_m5Frame.ObBull) count++;
                                        if (_m5Frame.DisplacementBull) count++;
                                        if (_m5Frame.MomentumBull) count++;
                                        if (_m5Frame.VolumeBull) count++;
                                        if (_m5Frame.MacdBull) count++;
                                        if (_m5Frame.VwapBull) count++;
                                        if (_m5Frame.VolatilityBull) count++;
                                    }
                                    else
                                    {
                                        if (_m5Frame.StructureBear) count++;
                                        if (_m5Frame.LiquidityBear) count++;
                                        if (_m5Frame.FvgBear) count++;
                                        if (_m5Frame.ObBear) count++;
                                        if (_m5Frame.DisplacementBear) count++;
                                        if (_m5Frame.MomentumBear) count++;
                                        if (_m5Frame.VolumeBear) count++;
                                        if (_m5Frame.MacdBear) count++;
                                        if (_m5Frame.VwapBear) count++;
                                        if (_m5Frame.VolatilityBear) count++;
                                    }
                        
                                    return count;
                                }
    }
}
