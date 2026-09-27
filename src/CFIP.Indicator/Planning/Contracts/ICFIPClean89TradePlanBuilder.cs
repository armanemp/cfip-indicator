// Service contract migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public interface ICFIPClean89TradePlanBuilder
        {
            CFIPClean89TradePlan Build(
                CFIPClean89DecisionSnapshot decision,
                CFIPClean89EntrySnapshot entry,
                CFIPClean89MarketModel market,
                CFIPClean89StructureSnapshot structure,
                CFIPClean89MtfSnapshot mtf,
                CFIPClean89RuntimeSnapshot runtime,
                CFIPClean89ConfigSnapshot configuration);
        }
}
