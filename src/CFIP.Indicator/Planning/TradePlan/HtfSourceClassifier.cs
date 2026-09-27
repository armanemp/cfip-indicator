// CFIP Indicator — HtfSourceClassifier.cs
// Single-responsibility planning module.

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
private bool IsHtfSource(
                                    string source)
                                {
                                    if (string.IsNullOrWhiteSpace(
                                            source))
                                        return false;
                        
                                    return
                                        source.IndexOf(
                                            "@M15",
                                            StringComparison.OrdinalIgnoreCase) >= 0 ||
                                        source.IndexOf(
                                            "@M30",
                                            StringComparison.OrdinalIgnoreCase) >= 0 ||
                                        source.IndexOf(
                                            "@H1",
                                            StringComparison.OrdinalIgnoreCase) >= 0 ||
                                        source.IndexOf(
                                            "@H4",
                                            StringComparison.OrdinalIgnoreCase) >= 0 ||
                                        source.IndexOf(
                                            "@D1",
                                            StringComparison.OrdinalIgnoreCase) >= 0 ||
                                        source.IndexOf(
                                            "@W1",
                                            StringComparison.OrdinalIgnoreCase) >= 0;
                                }
    }
}
