// ============================================================================
// CFIP Indicator — TimeWindowParser.cs
// One responsibility per module. Behavioral parity with v73 is preserved.
// ============================================================================

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
        private bool TryParseMinutes(
                            string value,
                            out int minutes)
                        {
                            minutes = 0;
                
                            if (string.IsNullOrWhiteSpace(
                                    value))
                                return false;
                
                            string[] parts =
                                value.Trim().Split(':');
                
                            if (parts.Length != 2)
                                return false;
                
                            int h;
                            int m;
                
                            if (!int.TryParse(
                                    parts[0],
                                    out h) ||
                                !int.TryParse(
                                    parts[1],
                                    out m))
                                return false;
                
                            if (h < 0 ||
                                h > 23 ||
                                m < 0 ||
                                m > 59)
                                return false;
                
                            minutes =
                                h * 60 +
                                m;
                
                            return true;
                        }
    }
}
