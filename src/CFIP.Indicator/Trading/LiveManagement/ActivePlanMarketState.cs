// ============================================================================
// CFIP Indicator — ActivePlanEvaluation.cs
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
        private bool TryPrepareActivePlanEvaluation(
                                    int closedM5,
                                    out double market,
                                    out double currentMove,
                                    out double peakRR)
                                {
                                    market = 0;
                                    currentMove = 0;
                                    peakRR = 0;

                                    if (_plan == null ||
                                        !_plan.IsLivePosition)
                                        return false;

                                    market =
                                        _plan.Direction == 1
                                            ? Symbol.Bid
                                            : Symbol.Ask;

                                    _lastMarket = market;

                                    if (!IsFinitePositive(market) ||
                                        _plan.Risk <= 0)
                                        return false;

                                    if (_plan.Direction == 1)
                                        _peakPrice =
                                            Math.Max(
                                                _peakPrice,
                                                market);
                                    else
                                        _peakPrice =
                                            Math.Min(
                                                _peakPrice,
                                                market);

                                    double favorable =
                                        _plan.Direction == 1
                                            ? _peakPrice - _plan.Entry
                                            : _plan.Entry - _peakPrice;

                                    currentMove =
                                        _plan.Direction == 1
                                            ? market - _plan.Entry
                                            : _plan.Entry - market;

                                    peakRR =
                                        favorable /
                                        Math.Max(
                                            Symbol.PipSize,
                                            _plan.Risk);

                                    return true;
                                }
    }
}
