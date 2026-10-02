using System;
using System.Globalization;

namespace CFIP.Contracts
{
    public static class SignalBusKey
    {
        private const ulong OffsetBasis = 14695981039346656037UL;
        private const ulong Prime = 1099511628211UL;

        public static string ForInstance(string instanceId)
        {
            ulong hash = OffsetBasis;
            string value = instanceId ?? "";

            unchecked
            {
                for (int i = 0; i < value.Length; i++)
                {
                    hash ^= value[i];
                    hash *= Prime;
                }
            }

            return "CFIPSignalBus" +
                   hash.ToString("X16", CultureInfo.InvariantCulture);
        }
    }
}