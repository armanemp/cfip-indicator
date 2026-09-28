// ============================================================================
// CFIP Indicator — ReversalProtection.cs
// ============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private int CalculateSmartExitPressure(
                                            double market,
                                            double currentRR)
                                        {
                                            if (_plan == null)
                                                return 0;
                                
                                            int pressure = 0;
                                            int opposite = _plan.Direction * -1;
                                
                                            if (_reaction != null &&
                                                _reaction.Direction == opposite)
                                            {
                                                pressure +=
                                                    Math.Min(
                                                        30,
                                                        Math.Max(
                                                            0,
                                                            _reaction.Confidence / 3));
                                
                                                if (_reaction.IndependentEvidence >=
                                                    MinimumLiveReactionEvidence)
                                                    pressure += 10;
                                            }
                                
                                            if (_m5Frame != null)
                                            {
                                                bool structure =
                                                    opposite == 1
                                                        ? _m5Frame.StructureBull
                                                        : _m5Frame.StructureBear;
                                
                                                bool reversal =
                                                    opposite == 1
                                                        ? (_m5Frame.MssBull ||
                                                           _m5Frame.ChochBull)
                                                        : (_m5Frame.MssBear ||
                                                           _m5Frame.ChochBear);
                                
                                                bool force =
                                                    opposite == 1
                                                        ? (_m5Frame.DisplacementBull &&
                                                           _m5Frame.LiquidityBull)
                                                        : (_m5Frame.DisplacementBear &&
                                                           _m5Frame.LiquidityBear);
                                
                                                if (structure)
                                                    pressure += 15;
                                
                                                if (reversal)
                                                    pressure += 15;
                                
                                                if (force)
                                                    pressure += 20;
                                            }
                                
                                            double atr =
                                                Atr(
                                                    _m5Bars,
                                                    Math.Max(
                                                        1,
                                                        _m5Bars.Count - 2));
                                
                                            if (atr > 0)
                                            {
                                                Zone zone =
                                                    FindNearestOpposingZone(
                                                        _m5Bars,
                                                        Math.Max(
                                                            1,
                                                            _m5Bars.Count - 2),
                                                        _plan.Direction,
                                                        atr);
                                
                                                if (zone != null &&
                                                    DistanceToZone(
                                                        market,
                                                        zone) <=
                                                    atr *
                                                    Math.Max(
                                                        0.05,
                                                        ZoneProximityAtr))
                                                    pressure += 10;
                                            }
                                
                                            if (currentRR < 0)
                                                pressure += 10;
                                
                                            return ClampInt(
                                                pressure,
                                                0,
                                                100);
                                        }
    }
}
