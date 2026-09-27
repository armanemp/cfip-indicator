// ============================================================================
// CFIP Indicator — HistoricalRenderer.cs
// One responsibility per module. Behavioral parity with v73 is preserved.
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
        // ============================================================
                
                        private void RenderHistoricalSignals()
                        {
                            RemoveHistoricalObjects();
                
                            if (Bars == null ||
                                Bars.Count < 60)
                                return;
                
                            int drawn = 0;
                            int last =
                                Bars.Count - 2;
                
                            for (int i = last;
                                 i >= 40 &&
                                 drawn < HistoricalSignalLimit;
                                 i--)
                            {
                                Frame frame =
                                    AnalyzeFrame(
                                        Bars,
                                        i);
                
                                if (frame.Direction == 0 ||
                                    frame.Quality <
                                    MinimumSmartQuality)
                                    continue;
                
                                int trigger =
                                    frame.Direction == 1
                                        ? BullTriggerScore(
                                            Bars,
                                            i)
                                        : BearTriggerScore(
                                            Bars,
                                            i);
                
                                if (trigger < 4)
                                    continue;
                
                                string name =
                                    H +
                                    i;
                
                                double atr =
                                    Atr(
                                        Bars,
                                        i);
                
                                double offset =
                                    Math.Max(
                                        Symbol.PipSize * 2,
                                        atr * 0.18);
                
                                if (ShowHistoricalArrows)
                                {
                                Chart.DrawIcon(
                                    name,
                                    frame.Direction == 1
                                        ? ChartIconType.UpArrow
                                        : ChartIconType.DownArrow,
                                    i,
                                    frame.Direction == 1
                                        ? Bars.LowPrices[i] -
                                          offset
                                        : Bars.HighPrices[i] +
                                          offset,
                                    frame.Direction == 1
                                        ? BuyArrowColor
                                        : SellArrowColor);
                
                                }
                
                                _historicalDrawn.Add(name);
                                drawn++;
                            }
                        }
        
        private void RemoveHistoricalObjects()
                        {
                            foreach (string name in _historicalDrawn)
                                Chart.RemoveObject(name);
                
                            _historicalDrawn.Clear();
                        }
    }
}
