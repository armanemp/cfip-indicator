using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private DecisionFilterResult EvaluateDecisionConfirmationGates(
            int closedM5,
            Decision decision)
        {
            if (RequireCoreAgreement &&
                _m5Frame != null &&
                _m15Frame != null)
            {
                bool aligned =
                    decision.Direction == 1
                        ? _m5Frame.Direction == 1 &&
                          (_m15Frame.Direction == 1 ||
                           (_m15Frame.Direction == 0 &&
                            AllowM15NeutralPullback))
                        : _m5Frame.Direction == -1 &&
                          (_m15Frame.Direction == -1 ||
                           (_m15Frame.Direction == 0 &&
                            AllowM15NeutralPullback));

                if (!aligned)
                    return new DecisionFilterResult(false, "CORE ALIGNMENT");
            }

            if (UseM5Confirmation &&
                (_m5Frame == null ||
                 _m5Frame.Direction != decision.Direction))
            {
                bool allowNeutralPrimaryPullback =
                    _m5Frame != null &&
                    _m15Frame != null &&
                    _h1Frame != null &&
                    PrimaryPullbackTuningRule.AllowsNeutralM5(
                        decision.Direction,
                        _m5Frame.Direction,
                        _m15Frame.Direction,
                        _h1Frame.Direction,
                        _m15Frame.Quality,
                        _h1Frame.Quality,
                        Math.Max(
                            MinimumTimeframeAgreement,
                            SmartMinimumTimeframeAgreement),
                        decision.TopDownEligible);

                if (!allowNeutralPrimaryPullback)
                    return new DecisionFilterResult(false, "M5 CONFIRMATION");
            }

            // EntryAllowed is a decision-quality permission, not the live
            // execution trigger. Retest/zone entries must be allowed to proceed
            // to the canonical actionability evaluator even before a breakout
            // trigger is confirmed. M5OnlyConfirmedTrigger is enforced there
            // only for trigger-dependent modes, while RetestMarket remains
            // zone-driven.
            // M1 direction is a live closed-bar trigger input, not a frozen
            // decision-direction veto. TriggerRuntime evaluates each newly closed
            // M1 inside the active M5 window and latches only a causal confirmation.
            return new DecisionFilterResult(true, string.Empty);
        }
    }
}