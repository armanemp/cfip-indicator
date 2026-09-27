// ============================================================================
// CFIP Indicator — LiveReversalAnalyzer.cs
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
        private bool CheckLiveReversalAgainstPlan(
                                    int closedM5)
                                {
                                    if (_plan == null ||
                                        !_plan.IsLivePosition ||
                                        _m5Frame == null ||
                                        !EnableLiveStructuralReversal ||
                                        !EnableFastReversalIntelligence)
                                        return false;
                        
                                    int opposite =
                                        _plan.Direction * -1;
                        
                                    bool structural =
                                        opposite == 1
                                            ? (_m5Frame.MssBull ||
                                               _m5Frame.ChochBull)
                                            : (_m5Frame.MssBear ||
                                               _m5Frame.ChochBear);
                        
                                    bool force =
                                        opposite == 1
                                            ? (_m5Frame.DisplacementBull &&
                                               _m5Frame.LiquidityBull)
                                            : (_m5Frame.DisplacementBear &&
                                               _m5Frame.LiquidityBear);
                        
                                    int reversalConfidence =
                                        _reaction != null
                                            ? Math.Max(
                                                _reaction.Confidence,
                                                _m5Frame.Quality)
                                            : _m5Frame.Quality;
                        
                                    if (reversalConfidence <
                                        Math.Max(
                                            50,
                                            LiveReversalMinimumConfidence))
                                        return false;
                        
                                    if (!structural ||
                                        _m5Frame.Quality <
                                        LiveReversalStructuralScore ||
                                        _m5Frame.Evidence <
                                        LiveReversalMinimumEvidence)
                                        return false;
                        
                                    if (RequireReversalForce &&
                                        !force)
                                        return false;
                        
                                    if (!AllowReversalAgainstStaleHtf &&
                                        _m15Frame != null &&
                                        _m15Frame.Direction ==
                                        _plan.Direction)
                                        return false;
                        
                                    if (RequireM15ReversalForOpposite &&
                                        _m15Frame != null &&
                                        _m15Frame.Direction !=
                                        opposite)
                                        return false;
                        
                                    if (StructuralSequence(
                                            _m5Bars,
                                            closedM5,
                                            opposite) <
                                        MinimumOppositeM5Structure)
                                        return false;
                        
                                    if (PreventRapidDirectionFlip &&
                                        _lastSignalM5 >= 0 &&
                                        closedM5 -
                                        _lastSignalM5 <
                                        OppositeSignalCooldownM5)
                                        return false;
                        
                                    int flipBars =
                                        Math.Max(
                                            1,
                                            SmartFlipConfirmationBars);
                        
                                    if (flipBars > 1 &&
                                        !StableDirection(
                                            _m5Bars,
                                            closedM5,
                                            opposite,
                                            flipBars))
                                        return false;
                        
                                    SendUnifiedAlert(
                                        "REVERSAL|" +
                                        closedM5 +
                                        "|" +
                                        opposite,
                                        "CFIP ACTIVE PLAN REVERSAL | " +
                                        (opposite == 1
                                            ? "BUY"
                                            : "SELL") +
                                        " REVERSAL | Q " +
                                        _m5Frame.Quality,
                                        opposite,
                                        true);
                        
                                    Position livePosition =
                                        GetManagedLivePositionForPlan();
                        
                                    if (livePosition != null)
                                    {
                                        if (!EnableReversalProtectionClose ||
                                            livePosition.NetProfit <= 0)
                                        {
                                            SetAutoTradingState(
                                                "BLOCKED",
                                                "REVERSAL DETECTED • POSITION RETAINED");
                                            return false;
                                        }
                        
                                        double protectedProfit =
                                            livePosition.NetProfit;
                        
                                        SetLifecycleState(
                                            LifecycleState.ExitRequested,
                                            "ACTIVE PLAN REVERSAL");
                        
                                        if (!TryClosePosition(
                                                livePosition,
                                                "ACTIVE PLAN REVERSAL"))
                                        {
                                            SetLifecycleState(
                                                LifecycleState.RecoveryRequired,
                                                "ACTIVE PLAN REVERSAL • EXIT REJECTED");
                        
                                            SetAutoTradingState(
                                                "ERROR",
                                                "REVERSAL CLOSE REJECTED");
                        
                                            return false;
                                        }
                        
                                        _lastExitM5 =
                                            Math.Max(
                                                _lastExitM5,
                                                closedM5);
                        
                                        SetAutoTradingState(
                                            "EXECUTED",
                                            "REVERSAL EXIT REQUESTED #" +
                                            livePosition.Id);
                        
                                        SendUnifiedAlert(
                                            "REVERSAL-CLOSE|" +
                                            livePosition.Id,
                                            "CFIP ACTIVE PLAN REVERSAL • EXIT REQUESTED | #" +
                                            livePosition.Id +
                                            " | protected +" +
                                            protectedProfit.ToString("F2"),
                                            _plan.Direction,
                                            true);
                        
                                        return true;
                                    }
                        
                                    SetLifecycleState(
                                        LifecycleState.Closed,
                                        "REVERSAL • POSITION ABSENT");
                        
                                    _plan = null;
                                    _executionModel = null;
                                    RemovePlanObjects();
                        
                                    return true;
                                }
        
        private Position GetManagedLivePositionForPlan()
                                {
                                    if (_plan == null ||
                                        !_plan.IsLivePosition ||
                                        _plan.PositionId <= 0)
                                        return null;
                        
                                    foreach (Position position in Positions)
                                    {
                                        if (position != null &&
                                            position.SymbolName == SymbolName &&
                                            position.Id == _plan.PositionId)
                                            return position;
                                    }
                        
                                    return null;
                                }
    }
}
