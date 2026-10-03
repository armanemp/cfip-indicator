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
                                    return ResolvePanelTimeframeState(frame)
                                        .Direction;
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
                        
                                    PanelTimeframePresentationState state =
                                        ResolvePanelTimeframeState(frame);

                                    string zones =
                                        "FVG " +
                                        (frame.FvgBull
                                            ? "B" + frame.FvgBullQuality
                                            : frame.FvgBear
                                                ? "S" + frame.FvgBearQuality
                                                : "0") +
                                        " | OB " +
                                        (frame.ObBull
                                            ? "B" + frame.ObBullQuality
                                            : frame.ObBear
                                                ? "S" + frame.ObBearQuality
                                                : "0") +
                                        (frame.FvgObBullConfluence
                                            ? " | FVG+OB B"
                                            : frame.FvgObBearConfluence
                                                ? " | FVG+OB S"
                                                : "");

                                    return
                                        state.DirectionLabel +
                                        " | Q" +
                                        frame.Quality +
                                        " | E" +
                                        frame.Evidence +
                                        " | SCORE " +
                                        frame.BullScore +
                                        "/" +
                                        frame.BearScore +
                                        " | " +
                                        zones;
                                }
    }
}
