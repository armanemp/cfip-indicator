// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public sealed class CFIPClean89BrokerStateSnapshot
        {
            private readonly ReadOnlyCollection<CFIPClean89BrokerPositionSnapshot> _positions;
            private readonly ReadOnlyCollection<CFIPClean89BrokerPendingOrderSnapshot> _pendingOrders;
    
            public IReadOnlyList<CFIPClean89BrokerPositionSnapshot> Positions
            {
                get { return _positions; }
            }
    
            public IReadOnlyList<CFIPClean89BrokerPendingOrderSnapshot> PendingOrders
            {
                get { return _pendingOrders; }
            }
    
            public CFIPClean89BrokerStateSnapshot(
                IList<CFIPClean89BrokerPositionSnapshot> positions,
                IList<CFIPClean89BrokerPendingOrderSnapshot> pendingOrders)
            {
                _positions =
                    new ReadOnlyCollection<CFIPClean89BrokerPositionSnapshot>(
                        new List<CFIPClean89BrokerPositionSnapshot>(
                            positions ??
                            new List<CFIPClean89BrokerPositionSnapshot>()));
    
                _pendingOrders =
                    new ReadOnlyCollection<CFIPClean89BrokerPendingOrderSnapshot>(
                        new List<CFIPClean89BrokerPendingOrderSnapshot>(
                            pendingOrders ??
                            new List<CFIPClean89BrokerPendingOrderSnapshot>()));
            }
        }
}
