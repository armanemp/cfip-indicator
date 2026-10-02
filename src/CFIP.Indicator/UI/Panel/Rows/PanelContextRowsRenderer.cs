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
        private void RenderPanelContextRows(
            ref int slot,
            int contentWidth)
        {
                                                if (_reaction != null &&
                                                    _reaction.Direction != 0)
                                                {
                                                    AddPanelRow(
                                                        ref slot,
                                                        "LIVE REACTION  •  " +
                                                        (_reaction.Direction == 1
                                                            ? "BUY"
                                                            : "SELL") +
                                                        "  •  Q" +
                                                        _reaction.Confidence +
                                                        "  •  EVID " +
                                                        _reaction.IndependentEvidence +
                                                        (_reaction.EntryAllowed
                                                            ? "  •  READY"
                                                            : "  •  WATCH"),
                                                        PanelDirectionColor(
                                                            _reaction.Direction),
                                                        true,
                                                        contentWidth);
                                                }
                                    
                                                AddPanelRow(
                                                    ref slot,
                                                    "MTF ALIGNMENT",
                                                    PanelSectionColor,
                                                    true,
                                                    contentWidth);
                                    
                                                AddPanelRow(
                                                    ref slot,
                                                    "M1   " +
                                                    FrameText(_m1Frame),
                                                    PanelDirectionColor(
                                                        FrameDirection(_m1Frame)),
                                                    false,
                                                    contentWidth);
                                    
                                                AddPanelRow(
                                                    ref slot,
                                                    "M5   " +
                                                    FrameText(_m5Frame),
                                                    PanelDirectionColor(
                                                        FrameDirection(_m5Frame)),
                                                    false,
                                                    contentWidth);
                                    
                                                AddPanelRow(
                                                    ref slot,
                                                    "M15  " +
                                                    FrameText(_m15Frame),
                                                    PanelDirectionColor(
                                                        FrameDirection(_m15Frame)),
                                                    false,
                                                    contentWidth);
                                    
                                                AddPanelRow(
                                                    ref slot,
                                                    "M30  " +
                                                    FrameText(_m30Frame),
                                                    PanelDirectionColor(
                                                        FrameDirection(_m30Frame)),
                                                    false,
                                                    contentWidth);
                                    
                                                AddPanelRow(
                                                    ref slot,
                                                    "H1   " +
                                                    FrameText(_h1Frame),
                                                    PanelDirectionColor(
                                                        FrameDirection(_h1Frame)),
                                                    false,
                                                    contentWidth);
                                    
                                                int primaryM15Direction =
                                                    FrameDirection(_m15Frame);
                                                int primaryH1Direction =
                                                    FrameDirection(_h1Frame);

                                                string primaryState =
                                                    primaryM15Direction != 0 &&
                                                    primaryH1Direction != 0
                                                        ? primaryM15Direction == primaryH1Direction
                                                            ? "ALIGNED"
                                                            : "CONFLICT"
                                                        : "CALIBRATING";

                                                string primaryDirections =
                                                    DirectionText(primaryM15Direction) +
                                                    "/" +
                                                    DirectionText(primaryH1Direction);

                                                AddPanelRow(
                                                    ref slot,
                                                    "PRIMARY M15/H1  •  " +
                                                    primaryState +
                                                    "  •  " +
                                                    primaryDirections +
                                                    "  •  Q" +
                                                    FrameQuality(_m15Frame) +
                                                    "/" +
                                                    FrameQuality(_h1Frame),
                                                    primaryM15Direction != 0 &&
                                                    primaryM15Direction == primaryH1Direction
                                                        ? PanelDirectionColor(primaryM15Direction)
                                                        : PanelWarningColor,
                                                    true,
                                                    contentWidth);

                                                AddPanelRow(
                                                    ref slot,
                                                    "H4   " +
                                                    FrameText(_h4Frame),
                                                    PanelDirectionColor(
                                                        FrameDirection(_h4Frame)),
                                                    false,
                                                    contentWidth);
                                    
                                                if (SmartWeeklyContext)
                                                {
                                                    AddPanelRow(
                                                        ref slot,
                                                        "D1   " +
                                                        FrameText(_d1Frame),
                                                        PanelDirectionColor(
                                                            FrameDirection(_d1Frame)),
                                                        false,
                                                        contentWidth);
                                    
                                                    AddPanelRow(
                                                        ref slot,
                                                        "W1   " +
                                                        FrameText(_w1Frame),
                                                        PanelDirectionColor(
                                                            FrameDirection(_w1Frame)),
                                                        false,
                                                        contentWidth);
                                                }
                                    
                                                if (ShowOutcomeDiagnostics)
                                                {
                                                    AddPanelRow(
                                                        ref slot,
                                                        "OUTCOME  W" +
                                                        _wins +
                                                        "  •  L" +
                                                        _losses +
                                                        "  •  CAL " +
                                                        CalibrationText(),
                                                        PanelAccentColor,
                                                        false,
                                                        contentWidth);
                                                }
                                    
            
        }
        private string DirectionText(int direction)
        {
            if (direction == 1)
                return "BUY";
            if (direction == -1)
                return "SELL";
            return "NEUTRAL";
        }

        private int FrameQuality(Frame frame)
        {
            return frame == null
                ? 0
                : Math.Max(0, Math.Min(100, frame.Quality));
        }

        }
    }
