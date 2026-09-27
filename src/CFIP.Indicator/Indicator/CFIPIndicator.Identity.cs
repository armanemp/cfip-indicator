using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CFIP.Indicator;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
                        private bool HasCurrentIdentity(string comment)
                {
                    return
                        comment != null &&
                        comment.IndexOf(
                            "CFIP|",
                            StringComparison.Ordinal) >= 0;
                }
        
        
    }
}
