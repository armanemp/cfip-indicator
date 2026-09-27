// ============================================================================
// CFIP Indicator — TextUtilities.cs
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
        private string CompactText(
                            string value,
                            int maxLength)
                        {
                            if (string.IsNullOrEmpty(value))
                                return string.Empty;
                
                            int limit =
                                Math.Max(
                                    1,
                                    maxLength);
                
                            if (value.Length <=
                                limit)
                                return value;
                
                            if (limit <= 3)
                                return value.Substring(
                                    0,
                                    limit);
                
                            return
                                value.Substring(
                                    0,
                                    limit - 3) +
                                "...";
                        }
    }
}
