namespace CFIP.Contracts
{
    // Single accepted-version policy for every incoming contract boundary.
    internal static class ContractVersionPolicy
    {
        public static bool IsSupported(int version) =>
            version == ContractVersion.Current;
    }
}
