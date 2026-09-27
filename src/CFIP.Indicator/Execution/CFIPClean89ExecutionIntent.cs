// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public sealed class CFIPClean89ExecutionIntent
        {
            public CFIPClean89ExecutionIdentity Identity { get; private set; }
            public CFIPClean89TradeIdentity TradeIdentity { get; private set; }
            public CFIPClean89Direction Direction { get; private set; }
            public CFIPClean89ExecutionKind Kind { get; private set; }
            public CFIPClean89EntryMode EntryMode { get; private set; }
    
            // Exact value requested from the broker.
            public CFIPClean89PriceLevel RequestedEntry { get; private set; }
    
            // Structural activation threshold, when applicable.
            public CFIPClean89PriceLevel Trigger { get; private set; }
    
            // Protection requested at execution time.
            public CFIPClean89PriceLevel StopLoss { get; private set; }
    
            // This is the requested active broker target, not the whole ladder.
            public CFIPClean89PriceLevel EffectiveTarget { get; private set; }
    
            public double VolumeInUnits { get; private set; }
            public CFIPClean89RiskRequest Risk { get; private set; }
            public CFIPClean89DecisionPolicyMode PolicyMode { get; private set; }
            public DateTime CreatedUtc { get; private set; }
            public DateTime? ExpiryUtc { get; private set; }
    
            public CFIPClean89ExecutionIntent(
                CFIPClean89ExecutionIdentity identity,
                CFIPClean89TradeIdentity tradeIdentity,
                CFIPClean89Direction direction,
                CFIPClean89ExecutionKind kind,
                CFIPClean89EntryMode entryMode,
                CFIPClean89PriceLevel requestedEntry,
                CFIPClean89PriceLevel trigger,
                CFIPClean89PriceLevel stopLoss,
                CFIPClean89PriceLevel effectiveTarget,
                double volumeInUnits,
                CFIPClean89RiskRequest risk,
                CFIPClean89DecisionPolicyMode policyMode,
                DateTime createdUtc,
                DateTime? expiryUtc)
            {
                Identity =
                    identity ??
                    throw new ArgumentNullException("identity");
                TradeIdentity =
                    tradeIdentity ??
                    throw new ArgumentNullException("tradeIdentity");
                Direction = direction;
                Kind = kind;
                EntryMode = entryMode;
                RequestedEntry = requestedEntry;
                Trigger = trigger;
                StopLoss =
                    stopLoss ??
                    throw new ArgumentNullException("stopLoss");
                EffectiveTarget = effectiveTarget;
                VolumeInUnits =
                    Math.Max(0, volumeInUnits);
                Risk =
                    risk ??
                    throw new ArgumentNullException("risk");
                PolicyMode = policyMode;
                CreatedUtc = createdUtc;
                ExpiryUtc = expiryUtc;
            }
        }
}
