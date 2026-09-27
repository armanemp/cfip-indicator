// CFIP Indicator — ManagedPositionCounter.cs
// Single-responsibility risk module.

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
private int ManagedPositionCount()
                        {
                            int count = 0;
                
                            foreach (Position position in Positions)
                            {
                                if (IsManagedPosition(position))
                                    count++;
                            }
                
                            return count;
                        }
    }
}
