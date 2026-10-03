// ============================================================================
// CFIP Indicator — PanelPredictionState.cs
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
        private string PredictionReadinessText()
                                {
                                    if (_decision == null)
                                        return "NO DECISION";
                        
                                    if (_decision.ActionableNow)
                                        return "ENTRY ACTIONABLE";

                                    if (_decision.EntryAllowed)
                                        return _decision.TriggerReady
                                            ? "SETUP CONFIRMED • WAITING ENTRY"
                                            : "SETUP QUALIFIED • WAITING TRIGGER";
                        
                                    if (_reaction != null &&
                                        _reaction.EntryAllowed)
                                        return "LIVE REACTION READY";
                        
                                    if (_prediction != null &&
                                        _prediction.Direction != 0)
                                        return
                                            DirectionText(_prediction.Direction) +
                                            " PREDICTED • SHARE " +
                                            _prediction.DirectionalShare +
                                            " • STR " +
                                            _prediction.AbsoluteStrength.ToString("F1", CultureInfo.InvariantCulture);
                        
                                    return
                                        "WAIT • " +
                                        (string.IsNullOrWhiteSpace(_decision.BlockReason)
                                            ? "STRUCTURAL GATE"
                                            : _decision.BlockReason);
                                }
        
        private Color PredictionReadinessColor()
                                {
                                    if (_decision != null &&
                                        _decision.ActionableNow)
                                        return TpLineColor;

                                    if (_decision != null &&
                                        _decision.EntryAllowed)
                                        return PanelAccentColor;
                        
                                    if (_reaction != null &&
                                        _reaction.EntryAllowed)
                                        return PanelAccentColor;
                        
                                    return PanelWarningColor;
                                }
        
        private string DailyPivotPanelText()
                                {
                                    if (_d1Bars == null ||
                                        _d1Bars.Count < 3)
                                        return "UNAVAILABLE";
                        
                                    int idx =
                                        ClosedIndex(
                                            _d1Bars,
                                            TimeInUtc);
                        
                                    if (idx <= 0)
                                        return "UNAVAILABLE";
                        
                                    int prev = idx - 1;
                        
                                    double h = _d1Bars.HighPrices[prev];
                                    double l = _d1Bars.LowPrices[prev];
                                    double c = _d1Bars.ClosePrices[prev];
                        
                                    if (h <= l)
                                        return "INVALID";
                        
                                    double p = (h + l + c) / 3.0;
                        
                                    double r1 = 2.0 * p - l;
                                    double s1 = 2.0 * p - h;
                                    double r2 = p + (h - l);
                                    double s2 = p - (h - l);
                        
                                    return
                                        "P " + Price(p) +
                                        "  R1 " + Price(r1) +
                                        "  S1 " + Price(s1) +
                                        "  R2 " + Price(r2) +
                                        "  S2 " + Price(s2);
                                }
    }
}
