using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    internal sealed class Frame
                    {
                        public Bars Bars;
                        public int Index;
                        public double Atr;
                        public double Rsi;
                        public double Adx;
                        public double EmaFast;
                        public double EmaSlow;
                        public int Direction;
                        public int BullScore;
                        public int BearScore;
                        public int Evidence;
                        public int Quality;
                        public bool StructureBull;
                        public bool StructureBear;
                        public bool MssBull;
                        public bool MssBear;
                        public bool ChochBull;
                        public bool ChochBear;
                        public bool DisplacementBull;
                        public bool DisplacementBear;
                        public bool LiquidityBull;
                        public bool LiquidityBear;
                        public bool FvgBull;
                        public bool FvgBear;
                        public bool ObBull;
                        public bool ObBear;
                        public bool TrendBull;
                        public bool TrendBear;
                        public bool MomentumBull;
                        public bool MomentumBear;
                        public bool RejectionBull;
                        public bool RejectionBear;
                        public bool EqualHigh;
                        public bool EqualLow;
                        public bool VolumeBull;
                        public bool VolumeBear;
                        public bool MacdBull;
                        public bool MacdBear;
                        public bool VwapBull;
                        public bool VwapBear;
                        public bool VolatilityBull;
                        public bool VolatilityBear;
                        public bool Choppy;
                        public double Choppiness;
                        public double AtrRatio;
                        public double EmaSpreadAtr;
                        public double EmaSlopeAtr;
                        public double RangeEfficiency;
                        public int OssBullVotes;
                        public int OssBearVotes;
                        public int OssIndicatorCount;
                        public bool OssBull;
                        public bool OssBear;
                    }
}
