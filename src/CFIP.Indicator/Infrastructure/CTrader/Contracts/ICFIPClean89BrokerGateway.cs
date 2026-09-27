// Service contract migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public interface ICFIPClean89BrokerGateway
        {
            CFIPClean89ExecutionResult Execute(
                CFIPClean89ExecutionIntent intent);
    
            CFIPClean89ExecutionResult ModifyProtection(
                string brokerPositionId,
                double? stopLoss,
                double? takeProfit);
    
            CFIPClean89ExecutionResult ClosePosition(
                string brokerPositionId);
    
            CFIPClean89ExecutionResult PartialClosePosition(
                string brokerPositionId,
                double volumeInUnits);
    
            CFIPClean89ExecutionResult CancelPendingOrder(
                string brokerOrderId);
    
            CFIPClean89ExecutionResult ModifyPendingProtection(
                string brokerOrderId,
                double? stopLoss,
                double? takeProfit);
        }
}
