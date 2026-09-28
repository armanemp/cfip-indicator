// ============================================================================
// CFIP Indicator — ReversalProtection.cs
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
        private bool CheckProfitExhaustionExit(
                                            int closedM5,
                                            double market,
                                            double peakRR)
                                        {
                                            if (!EnableProfitExhaustionProtection ||
                                                _plan == null ||
                                                !_plan.IsLivePosition ||
                                                peakRR <
                                                Math.Max(
                                                    0.8,
                                                    ExhaustionMinimumPeakRR) ||
                                                _plan.Risk <= 0)
                                                return false;
                                
                                            double favorable =
                                                Math.Max(
                                                    Symbol.PipSize,
                                                    _plan.Risk) *
                                                peakRR;
                                
                                            if (favorable <= 0)
                                                return false;
                                
                                            double retracement =
                                                _plan.Direction == 1
                                                    ? _peakPrice - market
                                                    : market - _peakPrice;
                                
                                            if (retracement <= 0)
                                                return false;
                                
                                            double retracementPercent =
                                                retracement /
                                                favorable *
                                                100.0;
                                
                                            if (retracementPercent <
                                                Math.Max(
                                                    15,
                                                    ExhaustionRetracementPercent))
                                                return false;
                                
                                            double currentRR =
                                                _plan.Direction == 1
                                                    ? (market - _plan.Entry) /
                                                      Math.Max(
                                                          Symbol.PipSize,
                                                          _plan.Risk)
                                                    : (_plan.Entry - market) /
                                                      Math.Max(
                                                          Symbol.PipSize,
                                                          _plan.Risk);
                                
                                            int pressure =
                                                CalculateSmartExitPressure(
                                                    market,
                                                    currentRR);
                                
                                            if (pressure <
                                                Math.Max(
                                                    55,
                                                    ExhaustionPressureThreshold))
                                                return false;
                                
                                            int opposite =
                                                _plan.Direction * -1;
                                
                                            bool oppositeStructure =
                                                _m5Frame != null &&
                                                (opposite == 1
                                                    ? (_m5Frame.StructureBull ||
                                                       _m5Frame.MssBull ||
                                                       _m5Frame.ChochBull)
                                                    : (_m5Frame.StructureBear ||
                                                       _m5Frame.MssBear ||
                                                       _m5Frame.ChochBear));
                                
                                            bool oppositeReaction =
                                                _reaction != null &&
                                                _reaction.Direction == opposite &&
                                                _reaction.Confidence >=
                                                Math.Max(
                                                    LiveReactionThreshold,
                                                    LiveReversalMinimumConfidence) &&
                                                _reaction.IndependentEvidence >=
                                                Math.Max(
                                                    1,
                                                    ExhaustionMinimumOppositeEvidence);
                                
                                            if (!oppositeStructure &&
                                                !oppositeReaction)
                                                return false;
                                
                                            Position position =
                                                GetManagedLivePositionForPlan();
                                
                                            if (position == null ||
                                                position.NetProfit <= 0)
                                                return false;
                                
                                            double protectedProfit =
                                                position.NetProfit;
                                
                                            if (!RequestLivePlanExit(
                                                    closedM5,
                                                    "PROFIT EXHAUSTION"))
                                                return false;
                                
                                            SendUnifiedAlert(
                                                "EXHAUSTION-CLOSE|" +
                                                position.Id,
                                                "CFIP EXHAUSTION EXIT REQUESTED | #" +
                                                position.Id +
                                                " | +" +
                                                protectedProfit.ToString("F2") +
                                                " | PEAK RR " +
                                                peakRR.ToString("F2") +
                                                " | RETRACE " +
                                                retracementPercent.ToString("F0") +
                                                "% | PRESSURE " +
                                                pressure,
                                                _plan.Direction,
                                                true);
                                
                                            SetAutoTradingState(
                                                "EXECUTED",
                                                "EXHAUSTION EXIT REQUESTED #" +
                                                position.Id);
                                
                                            return true;
                                        }
    }
}
