namespace cAlgo
{
    internal static class EntryTrapRiskPolicy
    {
        public const double AdverseM5BlockAtr = 0.30;
        public const double AdverseM1BlockAtr = 0.45;
        public const double StrongAdverseM5Atr = 0.45;
        public const double StrongAdverseM1Atr = 0.40;
        public const int StrongAdverseRiskFloor = 75;
        public const int M5AdverseRiskCap = 28;
        public const double M5AdverseRiskAtrWeight = 60;
        public const int M1AdverseRiskCap = 18;
        public const double M1AdverseRiskAtrWeight = 45;

        public const string AdverseM5Reason = "TRAP_ADVERSE_M5";
        public const string AdverseM1Reason = "TRAP_ADVERSE_M1";
        public const string ExtremeReason = "TRAP_EXTREME";
        public const string DivergenceReason = "TRAP_DIVERGENCE";

        public static string ResolveBlockReason(
            bool extreme,
            int divergenceQuality,
            int divergenceBlockQuality,
            double adverseM5Atr,
            double adverseM1Atr,
            int risk)
        {
            string reason = string.Empty;

            if (extreme)
                reason = ExtremeReason;

            if (divergenceQuality >= divergenceBlockQuality)
                reason = AppendReason(reason, DivergenceReason);

            if (adverseM5Atr >= AdverseM5BlockAtr)
                reason = AppendReason(reason, AdverseM5Reason);

            if (adverseM1Atr >= StrongAdverseM1Atr)
                reason = AppendReason(reason, AdverseM1Reason);

            if (string.IsNullOrEmpty(reason) &&
                risk >= StrongAdverseRiskFloor)
                return AdverseM1Reason;

            return string.IsNullOrEmpty(reason)
                ? "NONE"
                : reason;
        }

        public static string ResolveRetestContext(
            bool isRetest,
            bool insideZone,
            bool adverseM5PreZone,
            bool adverseM1PreZone,
            bool adverseEvidenceKnown)
        {
            if (!isRetest || !insideZone)
                return "NON_RETEST";

            if (!adverseEvidenceKnown)
                return "ZONE CONTEXT UNKNOWN";

            if (adverseM5PreZone && adverseM1PreZone)
                return "RETEST PRE-ZONE M5/M1";

            if (adverseM5PreZone)
                return "RETEST PRE-ZONE M5";

            if (adverseM1PreZone)
                return "RETEST PRE-ZONE M1";

            return "RETEST POST-ZONE/REACTION";
        }

        public static string FormatPresentationReason(
            string reason,
            string context)
        {
            if (string.IsNullOrWhiteSpace(reason))
                return "NOT EVALUATED";

            if (string.IsNullOrWhiteSpace(context) ||
                string.Equals(
                    context,
                    "NON_RETEST",
                    System.StringComparison.OrdinalIgnoreCase))
                return reason;

            return reason + " • " + context;
        }

        private static string AppendReason(
            string current,
            string next)
        {
            if (string.IsNullOrEmpty(current))
                return next;

            return current + " • " + next;
        }
    }
}
