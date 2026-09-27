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
                return new DecisionFilterResult(false, "M5 CONFIRMATION");

            if (M5OnlyConfirmedTrigger &&
                !ClosedBarTriggerReady(
                    _m5Bars,
                    closedM5,
                    decision.Direction))
            {
                bool directOverride =
                    AllowDirectDisplacementOverride &&
                    decision.Confidence >= SmartStrongSetupQuality &&
                    decision.Edge >= DirectDisplacementOverrideScore &&
                    _m5Frame != null &&
                    (decision.Direction == 1
                        ? _m5Frame.DisplacementBull
                        : _m5Frame.DisplacementBear);

                bool strongOverride =
                    AllowStrongTriggerOverride &&
                    AllowStrongM5TriggerOverride &&
                    decision.Confidence >= 85 &&
                    decision.Edge >= 20;

                if (!(directOverride || strongOverride))
                    return new DecisionFilterResult(false, "M5 TRIGGER");
            }

            if (UseM1Trigger &&
                _m1Frame != null &&
                _m1Frame.Direction != 0 &&
                _m1Frame.Direction != decision.Direction)
                return new DecisionFilterResult(false, "M1 MISALIGNMENT");

            return new DecisionFilterResult(true, string.Empty);
        }
    }
}