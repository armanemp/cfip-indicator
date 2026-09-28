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
                                                    MtfAlignmentText(),
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
    }
}
