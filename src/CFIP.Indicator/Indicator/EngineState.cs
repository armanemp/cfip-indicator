using System;

namespace CFIP.Indicator
{
            internal sealed class EngineState
        {
            public RuntimeSnapshot Runtime { get; private set; }
            public MtfSnapshot Mtf { get; private set; }
            public MarketModel Market { get; private set; }
            public StructureSnapshot Structure { get; private set; }
            public DecisionSnapshot Decision { get; private set; }
            public EntrySnapshot Entry { get; private set; }
            public TradePlan Plan { get; private set; }
            public ExecutionIntent Intent { get; private set; }
            public ExecutionResult Execution { get; private set; }
            public BrokerStateSnapshot Broker { get; private set; }
            public PresentationState Presentation { get; private set; }
    
            public void SetRuntime(RuntimeSnapshot value)
            {
                Runtime = value;
            }
    
            public void SetMtf(MtfSnapshot value)
            {
                Mtf = value;
            }
    
            public void SetMarket(MarketModel value)
            {
                Market = value;
            }
    
            public void SetStructure(StructureSnapshot value)
            {
                Structure = value;
            }
    
            public void SetDecision(DecisionSnapshot value)
            {
                Decision = value;
            }
    
            public void SetEntry(EntrySnapshot value)
            {
                Entry = value;
            }
    
            public void SetPlan(TradePlan value)
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
