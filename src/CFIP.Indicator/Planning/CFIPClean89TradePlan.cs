// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public sealed class CFIPClean89TradePlan
        {
            public CFIPClean89TradeIdentity Identity { get; private set; }
            public CFIPClean89Direction Direction { get; private set; }
            public CFIPClean89EntryMode EntryMode { get; private set; }
            public CFIPClean89EntryModel Entry { get; private set; }
            public CFIPClean89PriceLevel ExecutionAnchor { get; private set; }
            public CFIPClean89PriceLevel StructuralStop { get; private set; }
            public CFIPClean89TargetLadder TargetLadder { get; private set; }
            public double RiskRewardToTp1 { get; private set; }
            public double RiskRewardToFinalTarget { get; private set; }
            public int LevelQuality { get; private set; }
            public bool IsValid { get; private set; }
            public DateTime CreatedUtc { get; private set; }
            public int ReferenceBarIndex { get; private set; }
            public CFIPClean89Provenance Provenance { get; private set; }
    
            public CFIPClean89TradePlan(
                CFIPClean89TradeIdentity identity,
                CFIPClean89Direction direction,
                CFIPClean89EntryMode entryMode,
                CFIPClean89EntryModel entry,
                CFIPClean89PriceLevel executionAnchor,
                CFIPClean89PriceLevel structuralStop,
                CFIPClean89TargetLadder targetLadder,
                double riskRewardToTp1,
                double riskRewardToFinalTarget,
                int levelQuality,
                bool isValid,
                DateTime createdUtc,
                int referenceBarIndex,
                CFIPClean89Provenance provenance)
            {
                Identity =
                    identity ??
                    throw new ArgumentNullException("identity");
                Direction = direction;
                EntryMode = entryMode;
                Entry =
                    entry ??
                    throw new ArgumentNullException("entry");
                ExecutionAnchor =
                    executionAnchor ??
                    throw new ArgumentNullException("executionAnchor");
                StructuralStop =
                    structuralStop ??
                    throw new ArgumentNullException("structuralStop");
                TargetLadder =
                    targetLadder ??
                    throw new ArgumentNullException("targetLadder");
                RiskRewardToTp1 = Math.Max(0, riskRewardToTp1);
                RiskRewardToFinalTarget = Math.Max(0, riskRewardToFinalTarget);
                LevelQuality = Math.Max(0, Math.Min(100, levelQuality));
                IsValid = isValid;
    
                if (IsValid)
                {
                    if (!CFIPClean89DirectionRules.IsDirectional(Direction))
                        throw new ArgumentException(
                            "A valid TradePlan must be directional.",
                            "direction");
    
                    if (Entry.Direction != Direction)
                        throw new ArgumentException(
                            "TradePlan direction must match Entry direction.",
                            "entry");
    
                    if (!CFIPClean89DirectionRules.IsProtectivePrice(
                        Direction,
                        ExecutionAnchor.Price,
                        StructuralStop.Price))
                        throw new ArgumentException(
                            "TradePlan structural stop must protect the selected direction.",
                            "structuralStop");
    
                    if (!TargetLadder.ValidateForDirection(Direction, ExecutionAnchor.Price) ||
                        TargetLadder.Find(CFIPClean89TargetStage.TP1) == null ||
                        RiskRewardToTp1 <= 0)
                        throw new ArgumentException(
                            "A valid TradePlan requires an ordered TP ladder with TP1 and positive RR.",
                            "targetLadder");
                }
    
                CreatedUtc = createdUtc;
                ReferenceBarIndex = referenceBarIndex;
                Provenance =
                    provenance ??
                    CFIPClean89Provenance.Direct(
                        "UNKNOWN",
                        "UNSPECIFIED");
            }
        }
}
