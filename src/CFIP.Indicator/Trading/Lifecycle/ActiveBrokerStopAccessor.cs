// CFIP Indicator — ActiveBrokerStopAccessor.cs
// Single-responsibility lifecycle module.

using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private double GetActiveBrokerStopPrice()
                                {
                                    if (_plan != null &&
                                        _plan.IsLivePosition &&
                                        IsFinitePositive(_activeBrokerStop))
                                        return _activeBrokerStop;
                        
                                    return 0;
                                }
    }
}
