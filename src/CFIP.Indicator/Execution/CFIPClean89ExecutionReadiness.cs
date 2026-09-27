// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public sealed class CFIPClean89ExecutionReadiness
        {
            private readonly ReadOnlyCollection<CFIPClean89BlockReason> _blockReasons;
    
            public bool Eligible { get; private set; }
            public CFIPClean89ExecutionKind Kind { get; private set; }
            public CFIPClean89EntryMode EntryMode { get; private set; }
            public CFIPClean89Direction Direction { get; private set; }
            public double RequestedPrice { get; private set; }
            public double VolumeInUnits { get; private set; }
            public double RiskAmount { get; private set; }
            public double EstimatedMargin { get; private set; }
            public IReadOnlyList<CFIPClean89BlockReason> BlockReasons { get { return _blockReasons; } }
    
            public CFIPClean89ExecutionReadiness(
                bool eligible,
                CFIPClean89ExecutionKind kind,
                CFIPClean89EntryMode entryMode,
                CFIPClean89Direction direction,
                double requestedPrice,
                double volumeInUnits,
                double riskAmount,
                double estimatedMargin,
                IList<CFIPClean89BlockReason> blockReasons)
            {
                Eligible = eligible;
                Kind = kind;
                EntryMode = entryMode;
                Direction = direction;
                RequestedPrice = Math.Max(0, requestedPrice);
                VolumeInUnits = Math.Max(0, volumeInUnits);
                RiskAmount = Math.Max(0, riskAmount);
                EstimatedMargin = Math.Max(0, estimatedMargin);
                _blockReasons =
                    new ReadOnlyCollection<CFIPClean89BlockReason>(
                        new List<CFIPClean89BlockReason>(
                            blockReasons ??
                            new List<CFIPClean89BlockReason>()));
            }
        }
}
