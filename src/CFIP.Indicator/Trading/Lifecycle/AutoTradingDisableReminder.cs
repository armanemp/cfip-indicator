// CFIP Indicator — AutoTradingDisableReminder.cs
// Single-responsibility lifecycle module.

using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void CheckAutoTradingDisabledReminder(int closedM5)
                        {
                            if (_decision == null ||
                                _decision.Direction == 0 ||
                                !_decision.EntryAllowed ||
                                _lastAutoTradingReminderM5 == closedM5)
                                return;
                
                            int confidenceFloor =
                                Math.Max(
                                    MinimumAutoConfidence,
                                    HighConfidenceThreshold);
                
                            if (_decision.Confidence < confidenceFloor ||
                                _decision.SmartQuality < MinimumAutoSmartQuality ||
                                _decision.IndependentEvidence < MinimumIndependentEvidence)
                                return;
                
                            _lastAutoTradingReminderM5 =
                                closedM5;
                
                            SetAutoTradingState(
                                "OFF",
                                "HIGH-CONFIDENCE SIGNAL READY");
                
                            SendUnifiedAlert(
                                "AUTOOFF|" + closedM5,
                                "CFIP AUTO TRADING OFF | " +
                                (_decision.Direction == 1 ? "BUY" : "SELL") +
                                " SIGNAL READY | CONF " +
                                _decision.Confidence +
                                " | SMART " +
                                _decision.SmartQuality,
                                _decision.Direction,
                                true);
                        }
    }
}
