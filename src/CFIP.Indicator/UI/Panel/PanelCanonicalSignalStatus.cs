using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private string GetCanonicalSignalPanelStatus()
        {
            if (_decision == null ||
                _decision.Direction == 0)
            {
                string primary =
                    GetPrimaryTimeframeSignalPanelStatus();

                return string.IsNullOrWhiteSpace(primary)
                    ? "SIGNAL  •  WAIT / NO DIRECTION"
                    : primary;
            }

            string direction =
                DirectionText(_decision.Direction);

            SignalVisualSnapshot snapshot =
                _renderSignalVisualSnapshot != null
                    ? _renderSignalVisualSnapshot
                    : BuildSignalVisualSnapshot(
                        Math.Max(
                            1,
                            _lastEvaluatedM5));

            string strength =
                snapshot != null &&
                snapshot.MtfTrendDirection == _decision.Direction &&
                snapshot.MtfTrendStrengthLevel > 0
                    ? PanelNineLevelPresentationRule.LevelLabel(
                        snapshot.MtfTrendStrengthLevel)
                    : "WAIT";

            if (!_decision.EntryAllowed)
                return
                    "SIGNAL  •  " +
                    direction +
                    "  •  BLOCKED  •  " +
                    strength;

            if (_decision.ActionableNow)
                return
                    "SIGNAL  •  " +
                    direction +
                    "  •  ACTIONABLE  •  " +
                    strength;

            if (!_decision.TriggerReady)
                return
                    "SIGNAL  •  " +
                    direction +
                    "  •  WATCH  •  " +
                    strength;

            return
                "SIGNAL  •  " +
                direction +
                "  •  CONFIRMED  •  " +
                strength;
        }

        private string GetPrimaryTimeframeSignalPanelStatus()
        {
            PanelTimeframePresentationState m15State =
                ResolvePanelTimeframeState(_m15Frame);

            PanelTimeframePresentationState h1State =
                ResolvePanelTimeframeState(_h1Frame);

            int m15 =
                m15State.Direction;

            int h1 =
                h1State.Direction;

            if (m15 == 0 && h1 == 0)
                return string.Empty;

            string m15Text =
                m15State.DirectionLabel;

            string h1Text =
                h1State.DirectionLabel;

            if (m15 != 0 &&
                h1 != 0 &&
                m15 == h1)
            {
                return
                    "PRIMARY M15/H1  •  " +
                    m15Text +
                    "  •  M5/M1 TUNING";
            }

            if (m15 != 0 &&
                h1 != 0 &&
                m15 != h1)
            {
                return
                    "PRIMARY M15/H1  •  CONFLICT  •  " +
                    m15Text +
                    "/" +
                    h1Text;
            }

            return
                "PRIMARY M15/H1  •  " +
                (m15 != 0 ? "M15 " + m15Text : "M15 WAIT") +
                "  •  " +
                (h1 != 0 ? "H1 " + h1Text : "H1 WAIT") +
                "  •  M5/M1 TUNING";
        }

        private Color GetCanonicalSignalPanelStatusColor()
        {
            if (_decision == null ||
                _decision.Direction == 0)
                return PanelDirectionColor(
                    GetMarketBiasDirection());

            if (!_decision.EntryAllowed)
                return PanelWarningColor;

            SignalVisualSnapshot snapshot =
                _renderSignalVisualSnapshot != null
                    ? _renderSignalVisualSnapshot
                    : BuildSignalVisualSnapshot(
                        Math.Max(
                            1,
                            _lastEvaluatedM5));

            if (snapshot != null &&
                snapshot.MtfTrendDirection == _decision.Direction &&
                snapshot.MtfTrendStrengthLevel > 0)
            {
                return PanelNineLevelPresentationRule.ResolveColor(
                    _decision.Direction,
                    snapshot.MtfTrendStrengthLevel,
                    StrongBuyArrowColor,
                    StrongSellArrowColor,
                    ConfirmedBuyArrowColor,
                    ConfirmedSellArrowColor,
                    CautionBuyArrowColor,
                    CautionSellArrowColor,
                    BlockedReactionArrowColor);
            }

            return PanelDirectionColor(
                _decision.Direction);
        }
    }
}