using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        [Parameter(
            "Enable OSS Extended M5 Confluence",
            Group = "25 · OSS ANALYTICS",
            DefaultValue = false)]
        public bool UseOssExtendedIndicatorConfluence { get; set; }

        [Parameter(
            "Minimum OSS Indicator Agreement",
            Group = "25 · OSS ANALYTICS",
            DefaultValue = 3,
            MinValue = 1,
            MaxValue = 10)]
        public int MinimumOssIndicatorAgreement { get; set; }

        [Parameter(
            "OSS Confluence Weight",
            Group = "25 · OSS ANALYTICS",
            DefaultValue = 4,
            MinValue = 1,
            MaxValue = 15)]
        public int OssConfluenceWeight { get; set; }
    }
}
