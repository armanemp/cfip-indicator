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
                                    SignalVisualSnapshot snapshot =
                                        _renderSignalVisualSnapshot != null
                                            ? _renderSignalVisualSnapshot
                                            : BuildSignalVisualSnapshot(
                                                Math.Max(
                                                    1,
                                                    _lastEvaluatedM5));

                                    if (direction == 0)
                                        return "WAITING";

                                    string prefix =
                                        direction == 1
                                            ? "BUY "
                                            : "SELL ";

                                    switch (snapshot.Stage)
                                    {
                                        case "ACTIVE":
                                            return prefix + "ACTIVE";
                                        case "PENDING":
                                            return prefix + "PENDING";
                                        case "PLAN":
                                            return prefix + "PLAN";
                                        case "CONFIRMED":
                                            return prefix + "READY";
                                        case "REACTION":
                                            return prefix + "REACTION";
                                        case "PREDICTION":
                                            return prefix + "PREDICTION";
                                        default:
                                            return prefix + "WATCH";
                                    }
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
