// Migrated from CFIP-PRO v89; now isolated from the cTrader host namespace.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CFIP.Indicator.Core
{
        public sealed class CFIPClean89PriceLevel
        {
            public double Price { get; private set; }
            public string Name { get; private set; }
            public CFIPClean89Provenance Provenance { get; private set; }
    
            public CFIPClean89PriceLevel(
                double price,
                string name,
                CFIPClean89Provenance provenance)
            {
                if (price <= 0)
                    throw new ArgumentOutOfRangeException("price");
    
                Price = price;
                Name = name ?? string.Empty;
                Provenance = provenance ??
                             CFIPClean89Provenance.Direct(
                                 "UNKNOWN",
                                 "UNSPECIFIED");
            }
        }
}
