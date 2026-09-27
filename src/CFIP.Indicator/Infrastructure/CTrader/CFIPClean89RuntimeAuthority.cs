// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public sealed class CFIPClean89RuntimeAuthority
        {
            public bool AutoTradingEnabled { get; private set; }
            public bool AutomaticOrdersEnabled { get; private set; }
    
            public void InitializeFromConfiguration(
                CFIPClean89ConfigSnapshot configuration)
            {
                if (configuration == null)
                    throw new ArgumentNullException("configuration");
    
                AutoTradingEnabled =
                    configuration.Get(
                        "EnableAutoTrading",
                        false);
    
                AutomaticOrdersEnabled =
                    configuration.Get(
                        "EnableAutomaticOrders",
                        false);
            }
    
            public void SetAutoTradingEnabled(bool enabled)
            {
                AutoTradingEnabled = enabled;
            }
    
            public void SetAutomaticOrdersEnabled(bool enabled)
            {
                AutomaticOrdersEnabled = enabled;
            }
        }
}
