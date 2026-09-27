// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public sealed class CFIPClean89PriceZone
        {
            public double Lower { get; private set; }
            public double Upper { get; private set; }
            public string Name { get; private set; }
            public CFIPClean89Provenance Provenance { get; private set; }
    
            public CFIPClean89PriceZone(
                double lower,
                double upper,
                string name,
                CFIPClean89Provenance provenance)
            {
                if (lower <= 0 || upper <= 0 || upper < lower)
                    throw new ArgumentException("Invalid price zone.");
    
                Lower = lower;
                Upper = upper;
                Name = name ?? string.Empty;
                Provenance = provenance ??
                             CFIPClean89Provenance.Direct(
                                 "UNKNOWN",
                                 "UNSPECIFIED");
            }
    
            public bool Contains(double price)
            {
                return price >= Lower && price <= Upper;
            }
    
            public double Midpoint
            {
                get { return (Lower + Upper) / 2.0; }
            }
        }
}
