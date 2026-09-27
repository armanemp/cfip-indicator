// Partial cTrader host orchestration module migrated from v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
                            "CFIP89|",
                            StringComparison.Ordinal) >= 0;
                }
        
        
    }
}
