using System;

namespace cAlgo
{
    public partial class CFIPIndicator
    {
        private bool TryValidateTargetStageFeasibility(
            double requiredRR,
            double maximumRR,
            double risk,
            double atr,
            bool allowHtfExtension,
            out string rejectionReason)
        {
            rejectionReason = "";

            if (requiredRR > maximumRR)
            {
                rejectionReason =
                    TargetCandidateRejectionReasons.StageRequiredAboveMaximum;
                return false;
            }

            if (!TargetRewardEnvelopeRule.CanReachStage(
                    requiredRR,
                    risk,
                    atr,
                    MaximumTargetExtensionAtr,
                    maximumRR,
                    allowHtfExtension))
            {
                rejectionReason =
                    TargetCandidateRejectionReasons.StageUnreachableByExtension;
                return false;
            }

            return true;
        }
    }
}
