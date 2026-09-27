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
            public PredictionSnapshot Prediction { get; private set; }
            public MarketSuitabilitySnapshot Suitability { get; private set; }
    
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

            public void SetPrediction(PredictionSnapshot value)
            {
                Prediction = value;
            }

            public void SetSuitability(MarketSuitabilitySnapshot value)
            {
                Suitability = value;
            }
    
            public void ResetCycleOutputs()
            {
                Decision = null;
                Entry = null;
                Plan = null;
                Intent = null;
                Execution = null;
                Broker = null;
                Presentation = null;
                Prediction = null;
                Suitability = null;
            }
        }
}
