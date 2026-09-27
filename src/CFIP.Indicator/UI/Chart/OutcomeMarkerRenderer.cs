// ============================================================================
// CFIP Indicator — OutcomeMarkerRenderer.cs
// Presentation-only rendering for outcome telemetry markers.
// ============================================================================

using System;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
private void DrawOutcomeMarker(
                            string label,
                            double price,
                            bool success)
                        {
                            if (!ShowContextEventMarker ||
                                Bars == null ||
                                Bars.Count < 2 ||
                                !IsFinitePositive(price))
                                return;
                
                            try
                            {
                                string name =
                                    P +
                                    "OUTCOME_" +
                                    label.Replace(
                                        " ",
                                        "_") +
                                    "_" +
                                    Bars.Count +
                                    "_" +
                                    _outcomeSequence++;
                
                                ChartText marker =
                                    Chart.DrawText(
                                        name,
                                        label,
                                        Bars.OpenTimes[
                                            Bars.Count - 1],
                                        NormalizePrice(price),
                                        success
                                            ? TpLineColor
                                            : SlLineColor);
                
                                marker.FontSize =
                                    Math.Max(
                                        8,
                                        PanelFontSize);
                
                                marker.IsBold = true;
                                marker.IsInteractive = false;
                
                                _outcomeSequence =
                                    Math.Max(
                                        0,
                                        _outcomeSequence);
                            }
                            catch
                            {
                            }
                        }
    }
}
