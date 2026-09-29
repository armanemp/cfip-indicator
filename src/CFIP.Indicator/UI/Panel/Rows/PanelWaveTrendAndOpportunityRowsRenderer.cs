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
            if (UseWaveTrendEvidence)
            {
                AddPanelRow(
                    ref slot,
                    "WAVETREND  " +
                    _m5Frame.WaveTrend.ToString("F1") +
                    " / " +
                    _m5Frame.WaveTrendSignal.ToString("F1") +
                    "  •  Q " +
                    _m5Frame.WaveTrendQuality +
                    "  •  " +
                    (_m5Frame.WaveTrendBullCross
                        ? "BULL CROSS"
                        : _m5Frame.WaveTrendBearCross
                            ? "BEAR CROSS"
                            : _m5Frame.WaveTrendBull
                                ? "BULL"
                                : _m5Frame.WaveTrendBear
                                    ? "BEAR"
                                    : "NEUTRAL"),
                    _m5Frame.WaveTrendDirection == direction
                        ? PanelDirectionColor(direction)
                        : PanelSecondaryTextColor,
                    false,
                    contentWidth);
            }

            if (EnableParallelOpportunities &&
                _opportunityRegistry.Count > 0)
            {
                string laneSummary = "";

                for (int oi = 0;
                     oi < _opportunityRegistry.Count;
                     oi++)
                {
                    TradeOpportunityCandidate opportunity =
                        _opportunityRegistry[oi];

                    if (opportunity == null)
                        continue;

                    if (laneSummary.Length > 0)
                        laneSummary += "  •  ";

                    laneSummary +=
                        opportunity.LabelPrefix +
                        " " +
                        (opportunity.Direction == 1
                            ? "BUY"
                            : "SELL") +
                        " " +
                        opportunity.Tp1RR.ToString("F2") +
                        "R/" +
                        opportunity.Quality;
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