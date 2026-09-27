// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
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
