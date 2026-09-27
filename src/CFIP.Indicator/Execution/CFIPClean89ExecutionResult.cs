// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public sealed class CFIPClean89ExecutionResult
        {
            public bool Accepted { get; private set; }
            public string BrokerOrderId { get; private set; }
            public string BrokerPositionId { get; private set; }
            public CFIPClean89PriceLevel ActualFill { get; private set; }
            public double RequestedVsActualDelta { get; private set; }
            public string BrokerError { get; private set; }
            public CFIPClean89ProtectionState ProtectionState { get; private set; }
            public bool ReconciliationRequired { get; private set; }
            public string StatusDetail { get; private set; }
    
            public CFIPClean89ExecutionResult(
                bool accepted,
                string brokerOrderId,
                string brokerPositionId,
                CFIPClean89PriceLevel actualFill,
                double requestedVsActualDelta,
                string brokerError,
                CFIPClean89ProtectionState protectionState,
                bool reconciliationRequired,
                string statusDetail = "")
            {
                Accepted = accepted;
                BrokerOrderId = brokerOrderId ?? string.Empty;
                BrokerPositionId = brokerPositionId ?? string.Empty;
                ActualFill = actualFill;
                RequestedVsActualDelta = requestedVsActualDelta;
                BrokerError = brokerError ?? string.Empty;
                ProtectionState = protectionState;
                ReconciliationRequired = reconciliationRequired;
                StatusDetail = statusDetail ?? string.Empty;
            }
        }
}
