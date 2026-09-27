// ============================================================================
// CFIP Indicator — PanelExecutionState.cs
// One responsibility per module. Behavioral parity with v73 is preserved.
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
        private string GetAutoTradingPanelState()
                                {
                                    if (!AutoTradingEnabled)
                                        return "OFF";
                        
                                    string state =
                                        string.IsNullOrWhiteSpace(
                                            _autoTradingState)
                                            ? "ARMED"
                                            : _autoTradingState;
                        
                                    if (string.IsNullOrWhiteSpace(
                                            _autoTradingReason))
                                        return "ON • " + state;
                        
                                    return
                                        "ON • " +
                                        state +
                                        " • " +
                                        CompactText(
                                            _autoTradingReason,
                                            72);
                                }
        
        private Color GetAutoTradingPanelColor()
                                {
                                    if (!AutoTradingEnabled)
                                        return PanelMutedTextColor;
                        
                                    if ((EnableAggressiveAutoEntry &&
                                         _reaction != null &&
                                         _reaction.EntryAllowed) ||
                                        (_plan != null &&
                                         _decision != null &&
                                         _decision.EntryAllowed))
                                        return TpLineColor;
                        
                                    return PanelAccentColor;
                                }
        
        private string GetAutoProtectionPanelState()
                                {
                                    if (!AutoTradingEnabled)
                                        return "DISABLED WITH AUTO ENGINE";
                        
                                    if (!AutoBrokerProtection &&
                                        !AutoProtectBrokerPositions)
                                        return "OFF";
                        
                                    string state = "";
                        
                                    if (AutoBrokerProtection)
                                        state = "NEW TRADES";
                        
                                    if (AutoProtectBrokerPositions)
                                        state +=
                                            string.IsNullOrEmpty(state)
                                                ? "MANAGED POSITIONS"
                                                : " + MANAGED POSITIONS";
                        
                                    return state;
                                }
        
        private string GetExecutionRelationText(
                                    ExecutionModel model,
                                    double entry)
                                {
                                    if (model == null)
                                        return "NONE";
                        
                                    if (model.Mode ==
                                        ExecutionMode.WaitingForTrigger)
                                        return
                                            model.Direction == 1
                                                ? "BUY WAIT • ENTRY MUST REACH TRIGGER ABOVE"
                                                : "SELL WAIT • ENTRY MUST REACH TRIGGER BELOW";
                        
                                    if (!IsFinitePositive(entry))
                                        return "NOT EXECUTABLE";
                        
                                    if (model.Mode ==
                                        ExecutionMode.BreakoutMarket)
                                    {
                                        double tolerance =
                                            Math.Max(
                                                Symbol.TickSize * 2,
                                                Math.Max(
                                                    Symbol.PipSize * 0.5,
                                                    (Symbol.Ask - Symbol.Bid) * 2));
                        
                                        bool acceptable =
                                            model.Direction == 1
                                                ? entry >= model.Trigger - tolerance
                                                : entry <= model.Trigger + tolerance;
                        
                                        if (!acceptable)
                                            return "BREAKOUT NOT CONFIRMED";
                        
                                        return IsTriggerReached(
                                            model.Direction,
                                            entry,
                                            model.Trigger)
                                            ? "BREAKOUT CONFIRMED"
                                            : "BREAKOUT • SLIPPAGE ACCEPTED";
                                    }
                        
                                    if (model.Mode ==
                                        ExecutionMode.RetestMarket)
                                        return "RETEST INSIDE ZONE";
                        
                                    return ExecutionModeText(model.Mode);
                                }
    }
}
