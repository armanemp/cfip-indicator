using System;
using System.Globalization;

namespace CFIP.Contracts
{
    internal static class ContractBusKeyHash
    {
        private const ulong OffsetBasis = 14695981039346656037UL;
        private const ulong Prime = 1099511628211UL;

        public static string Hash(
            string value)
        {
            ulong hash = OffsetBasis;
            string input = value ?? string.Empty;

            unchecked
            {
                for (int i = 0; i < input.Length; i++)
                {
                    hash ^= input[i];
                    hash *= Prime;
                }
            }

            return hash.ToString(
                "X16",
                CultureInfo.InvariantCulture);
        }
    }
}
