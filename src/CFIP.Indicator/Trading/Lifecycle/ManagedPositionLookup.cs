// CFIP Indicator — ManagedPositionLookup.cs
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
        private Position GetManagedPositionById(long positionId)
                                {
                                    if (positionId <= 0)
                                        return null;
                        
                                    foreach (Position position in Positions)
                                    {
                                        if (position != null &&
                                            position.Id == positionId &&
                                            IsManagedPosition(position))
                                            return position;
                                    }
                        
                                    return null;
                                }
    }
}
