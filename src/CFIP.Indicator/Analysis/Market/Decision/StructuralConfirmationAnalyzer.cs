// CFIP Indicator — StructuralConfirmationAnalyzer.cs
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
        private int StructuralConfirmations(
                                    int direction)
                                {
                                    if (_m5Frame == null ||
                                        direction == 0)
                                        return 0;
                        
                                    int count = 0;
                        
                                    if (direction == 1)
                                    {
                                        if (_m5Frame.StructureBull) count++;
                                        if (_m5Frame.MssBull ||
                                            _m5Frame.ChochBull) count++;
                                        if (_m5Frame.DisplacementBull) count++;
                                        if (_m15Frame != null &&
                                            _m15Frame.StructureBull) count++;
                                        if (_h1Frame != null &&
                                            _h1Frame.StructureBull) count++;
                                        if (_h4Frame != null &&
                                            _h4Frame.StructureBull) count++;
                                    }
                                    else
                                    {
                                        if (_m5Frame.StructureBear) count++;
                                        if (_m5Frame.MssBear ||
                                            _m5Frame.ChochBear) count++;
                                        if (_m5Frame.DisplacementBear) count++;
                                        if (_m15Frame != null &&
                                            _m15Frame.StructureBear) count++;
                                        if (_h1Frame != null &&
                                            _h1Frame.StructureBear) count++;
                                        if (_h4Frame != null &&
                                            _h4Frame.StructureBear) count++;
                                    }
                        
                                    if (direction == 1)
                                    {
                                        if (_m5Frame.FvgBull && _m5Frame.FvgBullQuality >= 75)
                                            count++;
                                        if (_m5Frame.ObBull && _m5Frame.ObBullQuality >= 75)
                                            count++;
                                        if (_m5Frame.FvgObBullConfluence &&
                                            Math.Min(_m5Frame.FvgBullQuality, _m5Frame.ObBullQuality) >= 80)
                                            count++;
                                    }
                                    else
                                    {
                                        if (_m5Frame.FvgBear && _m5Frame.FvgBearQuality >= 75)
                                            count++;
                                        if (_m5Frame.ObBear && _m5Frame.ObBearQuality >= 75)
                                            count++;
                                        if (_m5Frame.FvgObBearConfluence &&
                                            Math.Min(_m5Frame.FvgBearQuality, _m5Frame.ObBearQuality) >= 80)
                                            count++;
                                    }

                                    return Math.Min(8, count);
                                }
    }
}
