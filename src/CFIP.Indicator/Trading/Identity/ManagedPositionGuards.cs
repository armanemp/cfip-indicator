// ============================================================================
// CFIP Indicator — ManagedPositionGuards.cs
// Single responsibility: managed-position existence checks.
// ============================================================================

using System;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool HasManagedOpenPosition()
        {
            foreach (Position position in Positions)
            {
                if (position == null ||
                    position.SymbolName != SymbolName)
                    continue;

                if (!IsManagedPosition(position))
                    continue;

                return true;
            }

            return false;
        }
    }
}
