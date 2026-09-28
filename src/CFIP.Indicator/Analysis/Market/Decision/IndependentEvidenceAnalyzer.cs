using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private readonly IndependentEvidenceFusionCalculator
            _independentEvidenceFusionCalculator =
                new IndependentEvidenceFusionCalculator();

        private int IndependentEvidence(
            int direction)
        {
            if (_m5Frame == null || direction == 0)
                return 0;

            bool bull = direction == 1;

            return _independentEvidenceFusionCalculator.Calculate(
                new IndependentEvidenceFusionInput(
                    bull
                        ? _m5Frame.StructureBull
                        : _m5Frame.StructureBear,
                    bull
                        ? _m5Frame.MssBull || _m5Frame.ChochBull
                        : _m5Frame.MssBear || _m5Frame.ChochBear,
                    bull
                        ? _m5Frame.DisplacementBull
                        : _m5Frame.DisplacementBear,
                    bull
                        ? _m5Frame.LiquidityBull
                        : _m5Frame.LiquidityBear,
                    bull
                        ? _m5Frame.FvgBull
                        : _m5Frame.FvgBear,
                    bull
                        ? _m5Frame.ObBull
                        : _m5Frame.ObBear,
                    bull
                        ? _m5Frame.TrendBull
                        : _m5Frame.TrendBear,
                    bull
                        ? _m5Frame.MomentumBull
                        : _m5Frame.MomentumBear,
                    bull
                        ? _m5Frame.MacdBull
                        : _m5Frame.MacdBear,
                    bull
                        ? _m5Frame.VwapBull
                        : _m5Frame.VwapBear,
                    bull
                        ? _m5Frame.VolumeBull
                        : _m5Frame.VolumeBear,
                    bull
                        ? _m5Frame.VolatilityBull
                        : _m5Frame.VolatilityBear,
                    bull
                        ? _m5Frame.RejectionBull
                        : _m5Frame.RejectionBear,
                    bull
                        ? _m5Frame.EqualLow
                        : _m5Frame.EqualHigh));
        }
    }
}
