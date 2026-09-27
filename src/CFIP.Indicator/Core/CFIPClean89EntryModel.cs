// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public sealed class CFIPClean89EntryModel
        {
            public CFIPClean89Direction Direction { get; private set; }
    
            // Preferred price inside the structural execution area.
            public CFIPClean89PriceLevel IdealEntry { get; private set; }
    
            // Allowed structural retest region.
            public CFIPClean89PriceZone EntryZone { get; private set; }
    
            // Structural activation threshold for breakout/continuation.
            public CFIPClean89PriceLevel Trigger { get; private set; }
    
            // Exact strategy/broker protection boundary.
            public CFIPClean89PriceLevel Invalidation { get; private set; }
    
            public CFIPClean89EntryModel(
                CFIPClean89Direction direction,
                CFIPClean89PriceLevel idealEntry,
                CFIPClean89PriceZone entryZone,
                CFIPClean89PriceLevel trigger,
                CFIPClean89PriceLevel invalidation)
            {
                if (!CFIPClean89DirectionRules.IsDirectional(direction))
                    throw new ArgumentException("A directional entry model is required.");
    
                if (idealEntry == null)
                    throw new ArgumentNullException("idealEntry");
    
                if (entryZone == null)
                    throw new ArgumentNullException("entryZone");
    
                if (invalidation == null)
                    throw new ArgumentNullException("invalidation");
    
                // Retest entries do not require an activation threshold.
                // Breakout/continuation entries must provide Trigger.
                Direction = direction;
                IdealEntry = idealEntry;
                EntryZone = entryZone;
                Trigger = trigger;
                Invalidation = invalidation;
            }
        }
}
