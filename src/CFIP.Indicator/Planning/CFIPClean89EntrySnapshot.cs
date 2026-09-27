// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public sealed class CFIPClean89EntrySnapshot
        {
            private readonly ReadOnlyCollection<CFIPClean89BlockReason> _blockReasons;
    
            // This is the exact Phase-6 snapshot received by Entry.
            public CFIPClean89DecisionSnapshot Decision { get; private set; }
    
            public CFIPClean89Direction Direction { get; private set; }
            public CFIPClean89EntryMode Mode { get; private set; }
            public CFIPClean89EntryTriggerState State { get; private set; }
            public CFIPClean89EntryModel Model { get; private set; }
            public CFIPClean89PriceLevel RequestedEntry { get; private set; }
    
            public bool TriggerReached { get; private set; }
            public bool Eligible { get; private set; }
    
            public int EntryQuality { get; private set; }
            public DateTime ReferenceUtc { get; private set; }
            public DateTime CreatedUtc { get; private set; }
            public DateTime? ExpiresUtc { get; private set; }
    
            public IReadOnlyList<CFIPClean89BlockReason> BlockReasons
            {
                get { return _blockReasons; }
            }
    
            public CFIPClean89Provenance Provenance { get; private set; }
    
            public CFIPClean89EntrySnapshot(
                CFIPClean89DecisionSnapshot decision,
                CFIPClean89Direction direction,
                CFIPClean89EntryMode mode,
                CFIPClean89EntryTriggerState state,
                CFIPClean89EntryModel model,
                CFIPClean89PriceLevel requestedEntry,
                bool triggerReached,
                bool eligible,
                int entryQuality,
                DateTime referenceUtc,
                DateTime createdUtc,
                DateTime? expiresUtc,
                IList<CFIPClean89BlockReason> blockReasons,
                CFIPClean89Provenance provenance)
            {
                Decision =
                    decision ??
                    throw new ArgumentNullException("decision");
    
                if (direction != decision.Direction)
                    throw new ArgumentException(
                        "Entry direction must equal the authoritative Decision direction.",
                        "direction");
    
                if (eligible &&
                    (!decision.DecisionEligible ||
                     state != CFIPClean89EntryTriggerState.Ready ||
                     model == null ||
                     requestedEntry == null ||
                     direction == CFIPClean89Direction.Wait))
                    throw new ArgumentException(
                        "Eligible Entry requires a directional ready Decision-backed model.",
                        "eligible");
    
                if (eligible &&
                    blockReasons != null &&
                    blockReasons.Count > 0)
                    throw new ArgumentException(
                        "Eligible Entry cannot contain block reasons.",
                        "blockReasons");
    
                if (!eligible &&
                    (blockReasons == null || blockReasons.Count == 0))
                    throw new ArgumentException(
                        "Blocked Entry requires at least one block reason.",
                        "blockReasons");
    
                if (triggerReached &&
                    model == null)
                    throw new ArgumentException(
                        "A reached Trigger requires an EntryModel.",
                        "triggerReached");
    
                Direction = direction;
                Mode = mode;
                State = state;
                Model = model;
                RequestedEntry = requestedEntry;
                TriggerReached = triggerReached;
                Eligible = eligible;
                EntryQuality = Math.Max(0, Math.Min(100, entryQuality));
                ReferenceUtc = referenceUtc;
                CreatedUtc = createdUtc;
                ExpiresUtc = expiresUtc;
    
                _blockReasons =
                    new ReadOnlyCollection<CFIPClean89BlockReason>(
                        new List<CFIPClean89BlockReason>(
                            blockReasons ??
                            new List<CFIPClean89BlockReason>()));
    
                Provenance =
                    provenance ??
                    CFIPClean89Provenance.Direct(
                        "UNKNOWN",
                        "UNSPECIFIED");
            }
        }
}
