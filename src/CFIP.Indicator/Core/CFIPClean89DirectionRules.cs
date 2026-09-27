// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public static class CFIPClean89DirectionRules
        {
            public static bool IsDirectional(CFIPClean89Direction direction)
            {
                return direction != CFIPClean89Direction.Wait;
            }
    
            public static CFIPClean89Direction Opposite(CFIPClean89Direction direction)
            {
                if (direction == CFIPClean89Direction.Buy)
                    return CFIPClean89Direction.Sell;
    
                if (direction == CFIPClean89Direction.Sell)
                    return CFIPClean89Direction.Buy;
    
                return CFIPClean89Direction.Wait;
            }
    
            public static bool IsProtectiveMove(
                CFIPClean89Direction direction,
                double currentStop,
                double candidateStop)
            {
                if (!IsDirectional(direction) ||
                    currentStop <= 0 ||
                    candidateStop <= 0)
                    return false;
    
                return direction == CFIPClean89Direction.Buy
                    ? candidateStop > currentStop
                    : candidateStop < currentStop;
            }
        }
}
