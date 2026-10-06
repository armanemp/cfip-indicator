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
        private string GetMarketTickVolumeBarsText()
        {
            Frame[] frames =
            {
                _m1Frame, _m5Frame, _m15Frame, _h1Frame, _h4Frame
            };

            string[] labels =
            {
                "M1", "M5", "M15", "H1", "H4"
            };

            string text = "TICK VOL  ";

            for (int i = 0; i < frames.Length; i++)
            {
                Frame frame = frames[i];
                text += labels[i] + " ";
                text += VolumeBarGlyph(frame);
                text += i == frames.Length - 1 ? "" : "  ";
            }

            return text;
        }

        private Color GetMarketTickVolumeBarsColor()
        {
            int direction = GetMarketBiasDirection();
            return direction == 0
                ? PanelSecondaryTextColor
                : PanelDirectionColor(direction);
        }

        private string VolumeBarGlyph(Frame frame)
        {
            if (frame == null ||
                frame.Bars == null ||
                frame.Index < 1 ||
                frame.Index >= frame.Bars.Count)
                return "—";

            double current = frame.Bars.TickVolumes[frame.Index];
            if (double.IsNaN(current) ||
                double.IsInfinity(current) ||
                current <= 0)
                return "—";

            int start = Math.Max(0, frame.Index - 20);
            double sum = 0;
            int count = 0;

            for (int i = start; i < frame.Index; i++)
            {
                double volume = frame.Bars.TickVolumes[i];
                if (double.IsNaN(volume) ||
                    double.IsInfinity(volume) ||
                    volume <= 0)
                    continue;

                sum += volume;
                count++;
            }

            if (count == 0 || sum <= 0)
                return "▂";

            double ratio = current / (sum / count);
            int level =
                ratio >= 2.50 ? 8 :
                ratio >= 2.00 ? 7 :
                ratio >= 1.70 ? 6 :
                ratio >= 1.40 ? 5 :
                ratio >= 1.15 ? 4 :
                ratio >= 0.90 ? 3 :
                ratio >= 0.65 ? 2 : 1;

            const string glyphs = "▁▂▃▄▅▆▇█";
            return glyphs[level - 1].ToString();
        }

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
                                    return FrameText(
                                        frame,
                                        ResolvePanelTimeframeState(frame));
                                }

        private string FrameText(
                                    Frame frame,
                                    PanelTimeframePresentationState state)
                                {
                                    if (frame == null)
                                        return "WAIT";

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
