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
                                        count +=
                                            StructuralEvidenceRule.CanonicalEventCount(
                                                _m5Frame.StructureBull,
                                                _m5Frame.MssBull,
                                                _m5Frame.ChochBull);
                                        if (_m5Frame.DisplacementBull) count++;
                                        count +=
                                            StructuralConfirmationRule.CountDirectionalStructureContribution(
                                                direction,
                                                _m15Frame == null ? 0 : _m15Frame.Direction,
                                                _m15Frame != null &&
                                                _m15Frame.StructureBull);
                                        count +=
                                            StructuralConfirmationRule.CountDirectionalStructureContribution(
                                                direction,
                                                _h1Frame == null ? 0 : _h1Frame.Direction,
                                                _h1Frame != null &&
                                                _h1Frame.StructureBull);
                                        count +=
                                            StructuralConfirmationRule.CountDirectionalStructureContribution(
                                                direction,
                                                _h4Frame == null ? 0 : _h4Frame.Direction,
                                                _h4Frame != null &&
                                                _h4Frame.StructureBull);
                                    }
                                    else
                                    {
                                        count +=
                                            StructuralEvidenceRule.CanonicalEventCount(
                                                _m5Frame.StructureBear,
                                                _m5Frame.MssBear,
                                                _m5Frame.ChochBear);
                                        if (_m5Frame.DisplacementBear) count++;
                                        count +=
                                            StructuralConfirmationRule.CountDirectionalStructureContribution(
                                                direction,
                                                _m15Frame == null ? 0 : _m15Frame.Direction,
                                                _m15Frame != null &&
                                                _m15Frame.StructureBear);
                                        count +=
                                            StructuralConfirmationRule.CountDirectionalStructureContribution(
                                                direction,
                                                _h1Frame == null ? 0 : _h1Frame.Direction,
                                                _h1Frame != null &&
                                                _h1Frame.StructureBear);
                                        count +=
                                            StructuralConfirmationRule.CountDirectionalStructureContribution(
                                                direction,
                                                _h4Frame == null ? 0 : _h4Frame.Direction,
                                                _h4Frame != null &&
                                                _h4Frame.StructureBear);
                                    }
                        
                                    return count;
                                }
    }
}
