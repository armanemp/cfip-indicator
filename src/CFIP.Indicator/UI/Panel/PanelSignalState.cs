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
        
        private int GetMarketBiasDirection()
                                {
                                    int m15 =
                                        FrameDirection(_m15Frame);

                                    int h1 =
                                        FrameDirection(_h1Frame);

                                    int m5 =
                                        FrameDirection(_m5Frame);

                                    if (m15 != 0 &&
                                        h1 != 0 &&
                                        m15 == h1)
                                        return m15;

                                    if (m15 != 0 &&
                                        h1 == 0)
                                        return m15;

                                    if (h1 != 0 &&
                                        m15 == 0)
                                        return h1;

                                    if (m5 != 0)
                                        return m5;

                                    if (_decision != null &&
                                        (_decision.Direction == 1 ||
                                         _decision.Direction == -1))
                                        return _decision.Direction;

                                    return 0;
                                }

        private string GetMarketBiasText()
                                {
                                    PanelTimeframePresentationState m15State =
                                        ResolvePanelTimeframeState(_m15Frame);

                                    PanelTimeframePresentationState h1State =
                                        ResolvePanelTimeframeState(_h1Frame);

                                    PanelTimeframePresentationState m5State =
                                        ResolvePanelTimeframeState(_m5Frame);

                                    int marketBiasDirection =
                                        GetMarketBiasDirection();

                                    string marketBiasLabel =
                                        PanelFrameDirectionRule.ResolveLabel(
                                            0,
                                            marketBiasDirection);

                                    return
                                        "MARKET BIAS  •  M15 " +
                                        m15State.DirectionLabel +
                                        "  •  H1 " +
                                        h1State.DirectionLabel +
                                        "  •  M5 " +
                                        m5State.DirectionLabel +
                                        "  •  " +
                                        marketBiasLabel;
                                }

        private int GetAuthoritativeDirection()
                                {
                                    if (_renderSignalVisualSnapshot != null &&
                                        _renderSignalVisualSnapshot.AuthoritativeDirection != 0)
                                        return _renderSignalVisualSnapshot.AuthoritativeDirection;

                                    if (_decision != null &&
                                        (_decision.Direction == 1 ||
                                         _decision.Direction == -1))
                                        return _decision.Direction;

                                    return BuildSignalVisualSnapshot(
                                        Math.Max(
                                            1,
                                            _lastEvaluatedM5)).AuthoritativeDirection;
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
                                        return
                                            GetMarketBiasDirection() != 0
                                                ? "WAITING FOR TRADE"
                                                : "WAITING";

                                    string prefix =
                                        DirectionText(direction) +
                                        " ";

                                    switch (snapshot.Stage)
                                    {
                                        case "ACTIVE":
                                            return prefix + "ACTIVE";
                                        case "PENDING":
                                            return prefix + "PENDING";
                                        case "PLAN":
                                            return prefix + "PLAN";
                                        case "CONFIRMED SETUP":
                                            return prefix + "SETUP";
                                        case "SETUP WATCH":
                                            return prefix + "WATCH";
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
                                        snapshot.AuthoritativeDirection;

                                    bool aligned =
                                        direction == 0 ||
                                        ((snapshot.PlanDirection == 0 ||
                                          snapshot.PlanDirection == direction) &&
                                         (snapshot.PendingDirection == 0 ||
                                          snapshot.PendingDirection == direction) &&
                                         (snapshot.DecisionDirection == 0 ||
                                          snapshot.DecisionDirection == direction) &&
                                         (snapshot.ReactionDirection == 0 ||
                                          snapshot.ReactionDirection == direction));

                                    string visualState =
                                        snapshot.Stage;

                                    return
                                        "STATE " +
                                        (direction == 0
                                            ? "WAIT"
                                            : DirectionText(direction)) +
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