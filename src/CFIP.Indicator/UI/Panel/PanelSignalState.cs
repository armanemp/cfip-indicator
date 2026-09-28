// ============================================================================
// CFIP Indicator — PanelSignalState.cs
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
                        
                                    PendingOrder pending =
                                        GetManagedPendingOrder();

                                    if (pending != null &&
                                        (pending.TradeType == TradeType.Buy ||
                                         pending.TradeType == TradeType.Sell))
                                        return pending.TradeType == TradeType.Buy
                                            ? 1
                                            : -1;

                                    // An active Plan owns the visual/trading thesis until it
                                    // is invalidated or replaced. Lower-level live reaction/frame
                                    // evidence must never flip the arrow while stale plan levels remain.
                                    if (_plan != null &&
                                        (_plan.Direction == 1 ||
                                         _plan.Direction == -1))
                                        return _plan.Direction;

                                    if (_decision != null &&
                                        (_decision.Direction == 1 ||
                                         _decision.Direction == -1))
                                        return _decision.Direction;

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
    }
}
