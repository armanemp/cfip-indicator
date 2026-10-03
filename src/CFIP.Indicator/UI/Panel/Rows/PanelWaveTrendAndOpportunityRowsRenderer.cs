using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator
    {
        private void RenderPanelWaveTrendAndOpportunityRows(
            ref int slot,
            int contentWidth,
            int direction)
        {
            if (UseWaveTrendEvidence &&
                _m5Frame != null)
            {
                int waveTrendDirection =
                    _m5Frame.WaveTrendDirection == 1
                        ? 1
                        : _m5Frame.WaveTrendDirection == -1
                            ? -1
                            : 0;

                bool waveTrendConflict =
                    waveTrendDirection != 0 &&
                    direction != 0 &&
                    waveTrendDirection != direction;

                string waveTrendState =
                    _m5Frame.WaveTrendBullCross
                        ? "BULL CROSS"
                        : _m5Frame.WaveTrendBearCross
                            ? "BEAR CROSS"
                            : waveTrendDirection == 1
                                ? "BULL"
                                : waveTrendDirection == -1
                                    ? "BEAR"
                                    : "NEUTRAL";

                if (waveTrendConflict)
                    waveTrendState += " • CONFLICT";

                Color waveTrendColor =
                    waveTrendConflict
                        ? PanelWarningColor
                        : PanelDirectionColor(waveTrendDirection);

                AddPanelRow(
                    ref slot,
                    "WAVETREND  " +
                    _m5Frame.WaveTrend.ToString("F1") +
                    " / " +
                    _m5Frame.WaveTrendSignal.ToString("F1") +
                    "  •  Q " +
                    _m5Frame.WaveTrendQuality +
                    "  •  " +
                    waveTrendState,
                    waveTrendColor,
                    false,
                    contentWidth);
            }

            if (_m5Frame != null)
            {
                AddPanelRow(
                    ref slot,
                    "INDICATOR FUSION  Q " +
                    _m5Frame.IndicatorConfluenceQuality +
                    "  •  CONFLICT " +
                    _m5Frame.IndicatorConflict,
                    _m5Frame.IndicatorConfluenceQuality >= 60 &&
                    _m5Frame.IndicatorConflict <= 45
                        ? PanelAccentColor
                        : PanelSecondaryTextColor,
                    false,
                    contentWidth);
            }

            if (EnableParallelOpportunities &&
                _opportunityCandidates.Count > 0)
            {
                string laneSummary = "";

                for (int oi = 0;
                     oi < _opportunityCandidates.Count;
                     oi++)
                {
                    TradeOpportunityCandidate opportunity =
                        _opportunityCandidates[oi];

                    if (opportunity == null)
                        continue;

                    if (laneSummary.Length > 0)
                        laneSummary += "  •  ";

                    laneSummary +=
                        opportunity.LabelPrefix +
                        " " +
                        DirectionText(opportunity.Direction) +
                        " " +
                        opportunity.Tp1RR.ToString("F2") +
                        "R/" +
                        opportunity.Quality;

                    if (opportunity.IsPrimaryTimeframeSignal)
                    {
                        laneSummary +=
                            opportunity.M1TuningConfirmed
                                ? " • M1✓"
                                : opportunity.M5TuningAligned
                                    ? " • M5✓"
                                    : " • TUNE";

                        if (opportunity.SourceFvgObConfluence)
                        {
                            laneSummary +=
                                " • OB+FVG " +
                                Math.Min(
                                    opportunity.SourceFvgQuality,
                                    opportunity.SourceOrderBlockQuality);
                        }
                        else if (opportunity.SourceOrderBlockQuality > 0)
                        {
                            laneSummary +=
                                " • OB " +
                                opportunity.SourceOrderBlockQuality;
                        }
                        else if (opportunity.SourceFvgQuality > 0)
                        {
                            laneSummary +=
                                " • FVG " +
                                opportunity.SourceFvgQuality;
                        }
                    }
                }

                AddPanelRow(
                    ref slot,
                    "LANES  " +
                    laneSummary,
                    PanelAccentColor,
                    false,
                    contentWidth);
            }
        }
    }
}