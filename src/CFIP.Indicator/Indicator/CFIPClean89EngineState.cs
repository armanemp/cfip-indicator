// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        internal sealed class CFIPClean89EngineState
        {
            public CFIPClean89RuntimeSnapshot Runtime { get; private set; }
            public CFIPClean89MtfSnapshot Mtf { get; private set; }
            public CFIPClean89MarketModel Market { get; private set; }
            public CFIPClean89StructureSnapshot Structure { get; private set; }
            public CFIPClean89DecisionSnapshot Decision { get; private set; }
            public CFIPClean89EntrySnapshot Entry { get; private set; }
            public CFIPClean89TradePlan Plan { get; private set; }
            public CFIPClean89ExecutionIntent Intent { get; private set; }
            public CFIPClean89ExecutionResult Execution { get; private set; }
            public CFIPClean89BrokerStateSnapshot Broker { get; private set; }
            public CFIPClean89PresentationState Presentation { get; private set; }
    
            public void SetRuntime(CFIPClean89RuntimeSnapshot value)
            {
                Runtime = value;
            }
    
            public void SetMtf(CFIPClean89MtfSnapshot value)
            {
                Mtf = value;
            }
    
            public void SetMarket(CFIPClean89MarketModel value)
            {
                Market = value;
            }
    
            public void SetStructure(CFIPClean89StructureSnapshot value)
            {
                Structure = value;
            }
    
            public void SetDecision(CFIPClean89DecisionSnapshot value)
            {
                Decision = value;
            }
    
            public void SetEntry(CFIPClean89EntrySnapshot value)
            {
                Entry = value;
            }
    
            public void SetPlan(CFIPClean89TradePlan value)
            {
                Plan = value;
            }
    
            // The cycle state is a downstream data carrier. Later phases extend
            // this with immutable cycle snapshots and explicit orchestration.
            public void ResetCycleOutputs()
            {
                // MTF is rebuilt on every Calculate cycle. Market and Structure
                // snapshots are intentionally retained until the closed-bar
                // reference changes, because their builders are reference-gated.
                Decision = null;
                Entry = null;
                Plan = null;
                Intent = null;
                Execution = null;
                Broker = null;
                Presentation = null;
            }
        }
}
