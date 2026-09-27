// Migrated from CFIP-PRO v89; now isolated from the cTrader host namespace.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CFIP.Indicator.Core
{
        public sealed class CFIPClean89Provenance
        {
            public string Source { get; private set; }
            public string Rule { get; private set; }
            public CFIPClean89FallbackKind Fallback { get; private set; }
            public string Detail { get; private set; }
    
            public CFIPClean89Provenance(
                string source,
                string rule,
                CFIPClean89FallbackKind fallback,
                string detail)
            {
                Source = source ?? string.Empty;
                Rule = rule ?? string.Empty;
                Fallback = fallback;
                Detail = detail ?? string.Empty;
            }
    
            public static CFIPClean89Provenance Direct(
                string source,
                string rule)
            {
                return new CFIPClean89Provenance(
                    source,
                    rule,
                    CFIPClean89FallbackKind.None,
                    string.Empty);
            }
    
            public static CFIPClean89Provenance FallbackFrom(
                string source,
                string rule,
                CFIPClean89FallbackKind fallback,
                string detail)
            {
                return new CFIPClean89Provenance(
                    source,
                    rule,
                    fallback,
                    detail);
            }
        }
}
