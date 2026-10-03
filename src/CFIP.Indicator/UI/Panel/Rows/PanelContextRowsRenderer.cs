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
                                                        DirectionText(_reaction.Direction) +
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
                                    
                                                PanelTimeframePresentationState m1FrameState =
                                                    ResolvePanelTimeframeState(_m1Frame);

                                                AddPanelRow(
                                                    ref slot,
                                                    "M1   " +
                                                    FrameText(
                                                        _m1Frame,
                                                        m1FrameState),
                                                    m1FrameState.Color,
                                                    false,
                                                    contentWidth);
                                    
                                                PanelTimeframePresentationState m5FrameState =
                                                    ResolvePanelTimeframeState(_m5Frame);

                                                AddPanelRow(
                                                    ref slot,
                                                    "M5   " +
                                                    FrameText(
                                                        _m5Frame,
                                                        m5FrameState),
                                                    m5FrameState.Color,
                                                    false,
                                                    contentWidth);
                                    
                                                PanelTimeframePresentationState m15FrameState =
                                                    ResolvePanelTimeframeState(_m15Frame);

                                                AddPanelRow(
                                                    ref slot,
                                                    "M15  " +
                                                    FrameText(
                                                        _m15Frame,
                                                        m15FrameState),
                                                    m15FrameState.Color,
                                                    false,
                                                    contentWidth);
                                    
                                                PanelTimeframePresentationState m30FrameState =
                                                    ResolvePanelTimeframeState(_m30Frame);

                                                AddPanelRow(
                                                    ref slot,
                                                    "M30  " +
                                                    FrameText(
                                                        _m30Frame,
                                                        m30FrameState),
                                                    m30FrameState.Color,
                                                    false,
                                                    contentWidth);
                                    
                                                PanelTimeframePresentationState h1FrameState =
                                                    ResolvePanelTimeframeState(_h1Frame);

                                                AddPanelRow(
                                                    ref slot,
                                                    "H1   " +
                                                    FrameText(
                                                        _h1Frame,
                                                        h1FrameState),
                                                    h1FrameState.Color,
                                                    false,
                                                    contentWidth);
                                    
                                                // Primary-signal alignment must use the frame's
                                                // resolved direction. The display-only BULL/BEAR
                                                // bias fallback is intentionally excluded here.
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
                                                    m15FrameState.DirectionLabel +
                                                    "/" +
                                                    h1FrameState.DirectionLabel;

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

                                                PanelTimeframePresentationState h4FrameState =
                                                    ResolvePanelTimeframeState(_h4Frame);

                                                AddPanelRow(
                                                    ref slot,
                                                    "H4   " +
                                                    FrameText(
                                                        _h4Frame,
                                                        h4FrameState),
                                                    h4FrameState.Color,
                                                    false,
                                                    contentWidth);
                                    
                                                if (SmartWeeklyContext)
                                                {
                                                    PanelTimeframePresentationState d1FrameState =
                                                        ResolvePanelTimeframeState(_d1Frame);

                                                    AddPanelRow(
                                                        ref slot,
                                                        "D1   " +
                                                        FrameText(
                                                            _d1Frame,
                                                            d1FrameState),
                                                        d1FrameState.Color,
                                                        false,
                                                        contentWidth);
                                    
                                                    PanelTimeframePresentationState w1FrameState =
                                                        ResolvePanelTimeframeState(_w1Frame);

                                                    AddPanelRow(
                                                        ref slot,
                                                        "W1   " +
                                                        FrameText(
                                                            _w1Frame,
                                                            w1FrameState),
                                                        w1FrameState.Color,
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
        private int FrameQuality(Frame frame)
        {
            return frame == null
                ? 0
                : Math.Max(0, Math.Min(100, frame.Quality));
        }

        }
    }