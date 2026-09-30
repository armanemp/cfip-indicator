// ============================================================================
// CFIP Indicator — PendingOrderPolicy.cs
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
        private bool TrendContinuationStrong()
                                {
                                    if (_decision == null ||
                                        _m5Frame == null ||
                                        _m15Frame == null ||
                                        _decision.Direction == 0)
                                        return false;
                        
                                    return
                                        _m5Frame.Direction == _decision.Direction &&
                                        _m15Frame.Direction == _decision.Direction &&
                                        _decision.StructuralConfirmations >=
                                        Math.Max(
                                            3,
                                            MinimumStructuralConfirmations) &&
                                        _decision.Confidence >=
                                        PendingMinimumConfidence &&
                                        _decision.SmartQuality >=
                                        PendingMinimumSmartQuality &&
                                        _decision.TimeframeAgreement >=
                                        PendingMinimumTrendQuality &&
                                        _m5Frame.IndicatorConfluenceQuality >=
                                        ExecutionThresholdPolicy.PendingContinuationIndicatorConfluenceMinimum &&
                                        _m5Frame.IndicatorConflict <=
                                        ExecutionThresholdPolicy.PendingContinuationIndicatorConflictMaximum;
                                }
        
        private bool ReversalSetupStrong()
        {
            if (_reaction == null ||
                _decision == null ||
                _reaction.Direction == 0 ||
                _decision.Direction == 0)
                return false;

            bool strongBase =
                _reaction.Direction != _decision.Direction &&
                ReactionQualificationRule.IsQualified(
                    _reaction.Direction,
                    _reaction.ReactionConfirmedQuality,
                    _reaction.ReactionConfirmedEvidence,
                    _reaction.ReactionConfirmedHasContext,
                    Math.Max(
                        PendingMinimumConfidence,
                        ReversalProtectionMinimumQuality),
                    Math.Max(
                        2,
                        ReversalCloseMinimumEvidence),
                    true,
                    _reaction.ReactionClosedBarConfirmed);

            if (!strongBase)
                return false;

            RangeSignalQualityResult rangeQuality =
                EvaluateRangeSignalQuality(
                    Math.Max(
                        0,
                        _m5Bars == null
                            ? 0
                            : _m5Bars.Count - 2),
                    _reaction.Direction,
                    _reaction.ReactionConfirmedQuality,
                    _reaction.ReactionConfirmedQuality,
                    Math.Max(
                        0,
                        _decision.Edge),
                    _reaction.ReactionConfirmedEvidence,
                    StructuralConfirmations(
                        _reaction.Direction));

            return rangeQuality.Allowed &&
                _m5Frame.IndicatorConfluenceQuality >=
                ExecutionThresholdPolicy.PendingReversalIndicatorConfluenceMinimum &&
                _m5Frame.IndicatorConflict <=
                ExecutionThresholdPolicy.PendingReversalIndicatorConflictMaximum;
        }

        private bool PendingModeAllowsStop()
                                {
                                    return
                                        PendingOrderMode == PendingOrderMode.Adaptive ||
                                        PendingOrderMode == PendingOrderMode.ContinuationStop ||
                                        PendingOrderMode == PendingOrderMode.Both;
                                }
        
        private bool PendingModeAllowsLimit()
                                {
                                    return
                                        PendingOrderMode == PendingOrderMode.Adaptive ||
                                        PendingOrderMode == PendingOrderMode.ReversalLimit ||
                                        PendingOrderMode == PendingOrderMode.Both;
                                }
        
        private double EffectiveRiskStopPips(double stopPips)
                                {
                                    double value =
                                        Math.Max(
                                            0,
                                            stopPips);
                        
                                    if (IncludeSpreadInRiskSizing)
                                        value +=
                                            Math.Max(
                                                0,
                                                (Symbol.Ask - Symbol.Bid) /
                                                Math.Max(
                                                    Symbol.PipSize,
                                                    1e-9));
                        
                                    return value;
                                }
        
        private bool IsValidPendingEntry(
                                    int direction,
                                    double price,
                                    bool stopOrder)
                                {
                                    if (!IsFinitePositive(price))
                                        return false;
                        
                                    double tolerance =
                                        Math.Max(
                                            Symbol.TickSize,
                                            Symbol.PipSize * 0.10);
                        
                                    if (direction == 1)
                                        return stopOrder
                                            ? price > Symbol.Ask + tolerance
                                            : price < Symbol.Ask - tolerance;
                        
                                    if (direction == -1)
                                        return stopOrder
                                            ? price < Symbol.Bid - tolerance
                                            : price > Symbol.Bid + tolerance;
                        
                                    return false;
                                }
    }
}
