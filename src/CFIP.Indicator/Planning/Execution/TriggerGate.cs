// ============================================================================
// CFIP Indicator — TriggerGate.cs
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
        // ============================================================
                        
                                private bool IsTriggerReached(
                                    int direction,
                                    double market,
                                    double trigger)
                                {
                                    if (!IsFinitePositive(market) ||
                                        !IsFinitePositive(trigger))
                                        return false;
                        
                                    double tolerance =
                                        EntryActionabilityPolicy.ResolveTriggerTolerance(
                                            Symbol.TickSize,
                                            Symbol.PipSize);
                        
                                    return direction == 1
                                        ? market >= trigger - tolerance
                                        : direction == -1 &&
                                          market <= trigger + tolerance;
                                }
        
        private bool IsContinuationExecutionContext(
                                    int direction)
                                {
                                    if (_decision == null ||
                                        _decision.Direction != direction ||
                                        _m5Frame == null ||
                                        _m15Frame == null)
                                        return false;
                        
                                    bool m5Aligned =
                                        _m5Frame.Direction == direction;
                        
                                    bool m15Compatible =
                                        _m15Frame.Direction == direction ||
                                        (_m15Frame.Direction == 0 &&
                                         AllowM15NeutralPullback);
                        
                                    bool trendRegime =
                                        string.Equals(
                                            _decision.Regime,
                                            "TREND",
                                            StringComparison.OrdinalIgnoreCase) ||
                                        string.Equals(
                                            _decision.Regime,
                                            "EXPANSION",
                                            StringComparison.OrdinalIgnoreCase);
                        
                                    return
                                        m5Aligned &&
                                        m15Compatible &&
                                        (trendRegime ||
                                         _decision.StructuralConfirmations >=
                                         EntryActionabilityPolicy.ResolveContinuationStructuralMinimum(
                                             MinimumStructuralConfirmations));
                                }
        
        private bool IsActionabilityTriggerReady(
            int closedM5,
            int direction,
            ExecutionMode mode)
        {
            if (!EntryActionabilityPolicy.RequiresConfirmedTrigger(
                    mode,
                    M5OnlyConfirmedTrigger))
                return true;

            if (ClosedBarTriggerReady(
                    _m5Bars,
                    closedM5,
                    direction))
                return true;

            if (AllowStrongTriggerOverride &&
                AllowStrongM5TriggerOverride &&
                _decision != null &&
                _decision.Direction == direction &&
                _decision.Confidence >= 85 &&
                _decision.Edge >= 20)
                return true;

            return false;
        }

        private string ExecutionModeText(
                                    ExecutionMode mode)
                                {
                                    switch (mode)
                                    {
                                        case ExecutionMode.WaitingForTrigger:
                                            return "WAIT TRIGGER";
                                        case ExecutionMode.RetestMarket:
                                            return "RETEST MARKET";
                                        case ExecutionMode.BreakoutMarket:
                                            return "BREAKOUT MARKET";
                                        case ExecutionMode.ContinuationStop:
                                            return "CONTINUATION STOP";
                                        case ExecutionMode.ReversalLimit:
                                            return "REVERSAL LIMIT";
                                        default:
                                            return "NONE";
                                    }
                                }
    }
}