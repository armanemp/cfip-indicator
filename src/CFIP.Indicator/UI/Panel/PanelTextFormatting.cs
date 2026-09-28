// ============================================================================
// CFIP Indicator — PanelTextFormatting.cs
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
                                        Server.TimeInUtc;
                        
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
        

        private string MtfAlignmentText()
                                {
                                    DateTime now =
                                        Server.TimeInUtc;

                                    if (_lastMtfClosedContext == null)
                                        return
                                            "MTF ALIGNMENT  •  " +
                                            now.ToString(
                                                "HH:mm:ss") +
                                            " UTC  •  WAITING";

                                    int bullish = 0;
                                    int bearish = 0;
                                    int available = 0;

                                    Frame[] frames =
                                    {
                                        _m5Frame,
                                        _m15Frame,
                                        _m30Frame,
                                        _h1Frame,
                                        _h4Frame
                                    };

                                    for (int i = 0;
                                         i < frames.Length;
                                         i++)
                                    {
                                        Frame frame =
                                            frames[i];

                                        if (frame == null)
                                            continue;

                                        if (frame.Direction == 1)
                                            bullish++;
                                        else if (frame.Direction == -1)
                                            bearish++;

                                        if (frame.Direction != 0)
                                            available++;
                                    }

                                    string alignment =
                                        available == 0
                                            ? "WAIT"
                                            : bullish == available
                                                ? "FULL BUY"
                                                : bearish == available
                                                    ? "FULL SELL"
                                                    : bullish >= 3
                                                        ? "BUY " +
                                                          bullish +
                                                          "/" +
                                                          available
                                                        : bearish >= 3
                                                            ? "SELL " +
                                                              bearish +
                                                              "/" +
                                                              available
                                                            : "MIXED";

                                    return
                                        "MTF ALIGNMENT  •  " +
                                        alignment +
                                        "  •  REF " +
                                        _lastMtfClosedContext.Reference.ToString(
                                            "HH:mm:ss") +
                                        "  •  CLOSED " +
                                        _lastMtfClosedContext.M5 +
                                        "/" +
                                        _lastMtfClosedContext.M15 +
                                        "/" +
                                        _lastMtfClosedContext.M30 +
                                        "/" +
                                        _lastMtfClosedContext.H1 +
                                        "/" +
                                        _lastMtfClosedContext.H4 +
                                        "  •  NOW " +
                                        now.ToString(
                                            "HH:mm:ss") +
                                        " UTC";
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
