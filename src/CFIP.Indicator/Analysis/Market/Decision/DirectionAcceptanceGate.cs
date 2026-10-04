// CFIP Indicator — DirectionAcceptanceGate.cs
// Single-responsibility decision evidence module.

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
        private bool CanAcceptConfirmedDirection(
                                    int direction,
                                    int closedM5)
                                {
                                    if (direction == 0)
                                        return false;
                        
                                    if (!PreventRapidDirectionFlip)
                                        return true;
                        
                                    if (_lastConfirmedDirection == 0 ||
                                        _lastConfirmedM5 < 0)
                                        return true;
                        
                                    int elapsed =
                                        closedM5 -
                                        _lastConfirmedM5;
                        
                                    if (elapsed < 0)
                                        return false;
                        
                                    if (direction ==
                                        _lastConfirmedDirection)
                                    {
                                        if (_plan != null)
                                            return false;
                        
                                        if (_lastExitM5 >= 0 &&
                                            closedM5 -
                                            _lastExitM5 <
                                            Math.Max(
                                                0,
                                                ExitReentryCooldownM5))
                                            return false;
                        
                                        return true;
                                    }
                        
                                    if (elapsed <
                                        Math.Max(
                                            1,
                                            OppositeSignalCooldownM5))
                                        return false;
                        
                                    if (_plan != null &&
                                        !AllowOppositeWhileActive)
                                        return false;
                        
                                    if (RequireM15ReversalForOpposite)
                                    {
                                        if (_m15Frame == null ||
                                            _m15Frame.Direction !=
                                            direction)
                                            return false;
                        
                                        bool structural =
                                            direction == 1
                                                ? (_m15Frame.MssBull ||
                                                   _m15Frame.ChochBull)
                                                : (_m15Frame.MssBear ||
                                                   _m15Frame.ChochBear);
                        
                                        bool force =
                                            direction == 1
                                                ? (_m15Frame.DisplacementBull &&
                                                   _m15Frame.LiquidityBull)
                                                : (_m15Frame.DisplacementBear &&
                                                   _m15Frame.LiquidityBear);
                        
                                        if (!(RequireReversalForce
                                                ? structural && force
                                                : structural || force))
                                            return false;
                        
                                        int m15Index =
                                            ClosedIndex(
                                                _m15Bars,
                                                ClosedBarBoundaryReference(
                            _m5Bars,
                            closedM5,
                            Server.TimeInUtc));
                        
                                        if (m15Index >= 0 &&
                                            !StableDirection(
                                                _m15Bars,
                                                m15Index,
                                                direction,
                                                Math.Max(
                                                    1,
                                                    SmartFlipConfirmationBars)))
                                            return false;
                                    }
                        
                                    int m5Evidence = 0;
                        
                                    if (_m5Frame != null)
                                    {
                                        if (direction == 1)
                                        {
                                            if (_m5Frame.MssBull) m5Evidence++;
                                            if (_m5Frame.ChochBull) m5Evidence++;
                                            if (_m5Frame.DisplacementBull) m5Evidence++;
                                            if (_m5Frame.LiquidityBull) m5Evidence++;
                                        }
                                        else
                                        {
                                            if (_m5Frame.MssBear) m5Evidence++;
                                            if (_m5Frame.ChochBear) m5Evidence++;
                                            if (_m5Frame.DisplacementBear) m5Evidence++;
                                            if (_m5Frame.LiquidityBear) m5Evidence++;
                                        }
                                    }
                        
                                    return
                                        m5Evidence >=
                                        Math.Max(
                                            1,
                                            MinimumOppositeM5Structure);
                                }
    }
}
