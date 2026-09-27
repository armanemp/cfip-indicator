// ============================================================================
// CFIP Indicator — PanelState.cs
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
        
        private string PredictionReadinessText()
                        {
                            if (_decision == null)
                                return "NO DECISION";
                
                            if (_decision.EntryAllowed)
                                return "ENTRY CONFIRMED";
                
                            if (_reaction != null &&
                                _reaction.EntryAllowed)
                                return "LIVE REACTION READY";
                
                            if (_prediction != null &&
                                _prediction.Direction != 0)
                                return
                                    (_prediction.Direction == 1 ? "BUY" : "SELL") +
                                    " PREDICTED • CONF " +
                                    _prediction.Confidence;
                
                            return
                                "WAIT • " +
                                (string.IsNullOrWhiteSpace(_decision.BlockReason)
                                    ? "STRUCTURAL GATE"
                                    : _decision.BlockReason);
                        }
        
        private Color PredictionReadinessColor()
                        {
                            if (_decision != null &&
                                _decision.EntryAllowed)
                                return TpLineColor;
                
                            if (_reaction != null &&
                                _reaction.EntryAllowed)
                                return PanelAccentColor;
                
                            return PanelWarningColor;
                        }
        
        private string DailyPivotPanelText()
                        {
                            if (_d1Bars == null ||
                                _d1Bars.Count < 3)
                                return "UNAVAILABLE";
                
                            int idx =
                                ClosedIndex(
                                    _d1Bars,
                                    TimeInUtc);
                
                            if (idx <= 0)
                                return "UNAVAILABLE";
                
                            int prev = idx - 1;
                
                            double h = _d1Bars.HighPrices[prev];
                            double l = _d1Bars.LowPrices[prev];
                            double c = _d1Bars.ClosePrices[prev];
                
                            if (h <= l)
                                return "INVALID";
                
                            double p = (h + l + c) / 3.0;
                
                            double r1 = 2.0 * p - l;
                            double s1 = 2.0 * p - h;
                            double r2 = p + (h - l);
                            double s2 = p - (h - l);
                
                            return
                                "P " + Price(p) +
                                "  R1 " + Price(r1) +
                                "  S1 " + Price(s1) +
                                "  R2 " + Price(r2) +
                                "  S2 " + Price(s2);
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
        
        private Color PanelDirectionColor(
                            int direction)
                        {
                            if (direction == 1)
                                return BuyArrowColor;
                
                            if (direction == -1)
                                return SellArrowColor;
                
                            return PanelTextColor;
                        }
        
        private int GetAuthoritativeDirection()
                        {
                            if (!UseAuthoritativeSignalState)
                                return _decision != null
                                    ? _decision.Direction
                                    : 0;
                
                            if (_plan != null &&
                                _plan.IsLivePosition &&
                                (_plan.Direction == 1 ||
                                 _plan.Direction == -1))
                                return _plan.Direction;
                
                            if (_reaction != null &&
                                _reaction.EntryAllowed &&
                                _reaction.Confidence >=
                                Math.Max(
                                    50,
                                    LiveReactionThreshold) &&
                                (_reaction.Direction == 1 ||
                                 _reaction.Direction == -1))
                                return _reaction.Direction;
                
                            if (_m5Frame != null &&
                                (_m5Frame.Direction == 1 ||
                                 _m5Frame.Direction == -1) &&
                                _m5Frame.Quality >=
                                Math.Max(
                                    50,
                                    LiveReactionThreshold) &&
                                ((_m5Frame.Direction == 1 &&
                                  (_m5Frame.MssBull ||
                                   _m5Frame.ChochBull)) ||
                                 (_m5Frame.Direction == -1 &&
                                  (_m5Frame.MssBear ||
                                   _m5Frame.ChochBear))))
                                return _m5Frame.Direction;
                
                            if (_decision != null &&
                                (_decision.Direction == 1 ||
                                 _decision.Direction == -1))
                                return _decision.Direction;
                
                            if (_prediction != null &&
                                _prediction.Direction != 0 &&
                                _prediction.Confidence >=
                                Math.Max(
                                    MinimumEarlyConfidence,
                                    EarlySetupConfidence))
                                return _prediction.Direction;
                
                            return 0;
                        }
        
        private string GetAuthoritativeState(
                            int direction)
                        {
                            if (_plan != null &&
                                _plan.IsLivePosition)
                                return direction == 1
                                    ? "BUY ACTIVE"
                                    : direction == -1
                                        ? "SELL ACTIVE"
                                        : "ACTIVE";
                
                            if (_reaction != null &&
                                _reaction.EntryAllowed &&
                                _reaction.Confidence >=
                                Math.Max(
                                    LiveReactionThreshold,
                                    LiveReactionStrongThreshold))
                                return direction == 1
                                    ? "BUY REACTION"
                                    : direction == -1
                                        ? "SELL REACTION"
                                        : "REACTION";
                
                            if (_decision != null &&
                                _decision.EntryAllowed)
                                return direction == 1
                                    ? "BUY READY"
                                    : direction == -1
                                        ? "SELL READY"
                                        : "READY";
                
                            if (_prediction != null &&
                                _prediction.Direction != 0)
                                return direction == 1
                                    ? "BUY PREDICTION"
                                    : direction == -1
                                        ? "SELL PREDICTION"
                                        : "PREDICTION";
                
                            return direction == 1
                                ? "BUY WATCH"
                                : direction == -1
                                    ? "SELL WATCH"
                                    : "WAITING";
                        }
        
        private string GetSignalSynchronizationText()
                        {
                            int direction =
                                GetAuthoritativeDirection();
                
                            bool planAligned =
                                _plan == null ||
                                direction == 0 ||
                                _plan.Direction == direction;
                
                            bool decisionAligned =
                                _decision == null ||
                                _decision.Direction == 0 ||
                                direction == 0 ||
                                _decision.Direction == direction;
                
                            bool reactionAligned =
                                _reaction == null ||
                                _reaction.Direction == 0 ||
                                direction == 0 ||
                                !_reaction.EntryAllowed ||
                                _reaction.Direction == direction;
                
                            bool aligned =
                                planAligned &&
                                decisionAligned &&
                                reactionAligned;
                
                            string visualState =
                                _plan != null
                                    ? "PLAN+ARROW+LEVELS"
                                    : _decision != null &&
                                      _decision.EntryAllowed
                                        ? "DECISION+ARROW"
                                        : _prediction != null &&
                                          _prediction.Direction != 0
                                            ? "PREDICTION"
                                            : "WAIT";
                
                            return
                                "STATE " +
                                (direction == 1
                                    ? "BUY"
                                    : direction == -1
                                        ? "SELL"
                                        : "WAIT") +
                                " | " +
                                (aligned
                                    ? "ALIGNED"
                                    : "CHECK") +
                                " | " +
                                visualState;
                        }
        
        private Color GetSignalSynchronizationColor()
                        {
                            string text =
                                GetSignalSynchronizationText();
                
                            return text.IndexOf(
                                       "ALIGNED",
                                       StringComparison.OrdinalIgnoreCase) >= 0
                                ? TpLineColor
                                : PanelWarningColor;
                        }
        
        private int FrameDirection(
                            Frame frame)
                        {
                            return frame == null
                                ? 0
                                : frame.Direction;
                        }
        
        private string GetStablePanelState(
                            string candidate)
                        {
                            if (string.IsNullOrWhiteSpace(
                                    candidate))
                                candidate = "WAITING";
                
                            DateTime now =
                                TimeInUtc;
                
                            bool authoritative =
                                _plan != null ||
                                candidate.IndexOf(
                                    "ACTIVE",
                                    StringComparison.OrdinalIgnoreCase) >= 0;
                
                            if (authoritative ||
                                PanelStateHoldSeconds <= 0 ||
                                string.IsNullOrWhiteSpace(
                                    _panelStableHeader))
                            {
                                _panelStableHeader =
                                    candidate;
                
                                _panelStableHeaderSinceUtc =
                                    now;
                
                                return _panelStableHeader;
                            }
                
                            double heldSeconds =
                                (now -
                                 _panelStableHeaderSinceUtc)
                                .TotalSeconds;
                
                            if (!string.Equals(
                                    _panelStableHeader,
                                    candidate,
                                    StringComparison.OrdinalIgnoreCase) &&
                                heldSeconds >=
                                Math.Max(
                                    0,
                                    PanelStateHoldSeconds))
                            {
                                _panelStableHeader =
                                    candidate;
                
                                _panelStableHeaderSinceUtc =
                                    now;
                            }
                
                            return _panelStableHeader;
                        }
        
        private string ConfluenceText(
                            Frame frame)
                        {
                            if (frame == null)
                                return "WAIT";
                
                            List<string> parts =
                                new List<string>();
                
                            if (UseVolumeExpansion)
                                parts.Add(
                                    frame.VolumeBull
                                        ? "VOL+"
                                        : frame.VolumeBear
                                            ? "VOL-"
                                            : "VOL0");
                
                            if (UseMacdBias)
                                parts.Add(
                                    frame.MacdBull
                                        ? "MACD+"
                                        : frame.MacdBear
                                            ? "MACD-"
                                            : "MACD0");
                
                            if (UseVwapBias)
                                parts.Add(
                                    frame.VwapBull
                                        ? "VWAP+"
                                        : frame.VwapBear
                                            ? "VWAP-"
                                            : "VWAP0");
                
                            if (UseHealthyVolatility)
                                parts.Add(
                                    frame.VolatilityBull
                                        ? "ATR+"
                                        : frame.VolatilityBear
                                            ? "ATR-"
                                            : "ATR0");
                
                            return parts.Count == 0
                                ? "OFF"
                                : string.Join(
                                    " | ",
                                    parts.ToArray());
                        }
        
        private string FrameText(
                            Frame frame)
                        {
                            if (frame == null)
                                return "WAIT";
                
                            return
                                (frame.Direction == 1
                                    ? "BUY"
                                    : frame.Direction == -1
                                        ? "SELL"
                                        : "NEUTRAL") +
                                " | Q" +
                                frame.Quality +
                                " | E" +
                                frame.Evidence;
                        }
    }
}
