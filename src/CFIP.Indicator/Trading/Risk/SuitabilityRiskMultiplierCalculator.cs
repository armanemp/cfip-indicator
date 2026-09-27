// CFIP Indicator — SuitabilityRiskMultiplierCalculator.cs
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
private double SuitabilityRiskMultiplier()
                                {
                                    if (!UseSmartRiskScaling)
                                        return 1.0;
                        
                                    double floor =
                                        ClampDouble(
                                            MinimumSmartRiskMultiplier,
                                            0.25,
                                            1.0);
                        
                                    double confidence =
                                        _decision == null
                                            ? 0
                                            : ClampDouble(
                                                _decision.Confidence,
                                                0,
                                                100);
                        
                                    double confidenceScale =
                                        ClampDouble(
                                            (confidence - 70.0) /
                                            Math.Max(
                                                1.0,
                                                Math.Max(
                                                    70,
                                                    FullRiskConfidenceThreshold) - 70.0),
                                            0,
                                            1);
                        
                                    double suitabilityScale =
                                        ClampDouble(
                                            _marketSuitabilityScore /
                                            (double)Math.Max(
                                                60,
                                                FullRiskSuitabilityThreshold),
                                            0,
                                            1);
                        
                                    double scale =
                                        floor +
                                        (1.0 - floor) *
                                        (0.60 * confidenceScale +
                                         0.40 * suitabilityScale);
                        
                                    if (PenalizeChoppyRegimeRisk &&
                                        _m5Frame != null &&
                                        _m15Frame != null &&
                                        (_m5Frame.Choppy ||
                                         _m15Frame.Choppy))
                                        scale *= 0.82;
                        
                                    return ClampDouble(
                                        scale,
                                        floor,
                                        1.0);
                                }
    }
}
