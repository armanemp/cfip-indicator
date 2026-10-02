using System;
using System.Globalization;

namespace CFIP.Contracts
{
    public static class SignalBusKey
    {
        public static string ForInstance(string instanceId)
        {
            return "CFIPSignalBus" +
                   ContractBusKeyHash.Hash(instanceId);
        }

        public static string ForScenarioBatch(string instanceId)
        {
            return "CFIPSignalScenarioBatch" +
                   ContractBusKeyHash.Hash(instanceId);
        }
    }
}