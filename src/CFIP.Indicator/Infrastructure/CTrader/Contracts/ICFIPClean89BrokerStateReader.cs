// Service contract migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public interface ICFIPClean89BrokerStateReader
        {
            CFIPClean89BrokerStateSnapshot ReadManagedState(
                string symbol,
                string strategyId);
        }
}
