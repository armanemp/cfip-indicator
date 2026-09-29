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
                                    if (_renderSignalVisualSnapshot != null)
                                        return _renderSignalVisualSnapshot.Direction;

                                    return BuildSignalVisualSnapshot(
                                        Math.Max(
                                            1,
                                            _lastEvaluatedM5)).Direction;
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
                                    SignalVisualSnapshot snapshot =
                                        _renderSignalVisualSnapshot != null
                                            ? _renderSignalVisualSnapshot
                                            : BuildSignalVisualSnapshot(
                                                Math.Max(
                                                    1,
                                                    _lastEvaluatedM5));

                                    int direction =
                                        snapshot.Direction;

                                    bool aligned =
                                        direction != 0 &&
                                        (snapshot.PlanDirection == 0 ||
                                         snapshot.PlanDirection == direction) &&
                                        (snapshot.PendingDirection == 0 ||
                                         snapshot.PendingDirection == direction) &&
                                        (snapshot.DecisionDirection == 0 ||
                                         snapshot.DecisionDirection == direction) &&
                                        (snapshot.ReactionDirection == 0 ||
                                         snapshot.ReactionDirection == direction);

                                    string visualState =
                                        snapshot.Stage;

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
