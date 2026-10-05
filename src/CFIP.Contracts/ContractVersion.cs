namespace CFIP.Contracts
{
    public static class ContractVersion
    {
        public const int Current = 2;

        public static bool IsSupported(int version) =>
            version == Current;
    }
}
