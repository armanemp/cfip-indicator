namespace cAlgo
{
    internal sealed class Decision
                    {
                        public int Direction;
                        public int Confidence;
                        public int Edge;
                        public int SmartQuality;
                        public int TimeframeAgreement;
                        public int HtfAnchorDirection;
                        public int HtfAlignment;
                        public int MidframeDirection;
                        public int MidframeAlignment;
                        public int EntryFrameAlignment;
                        public bool TopDownEligible;
                        public string TopDownStage;
                        public int IndependentEvidence;
                        public int StructuralConfirmations;
                        public int RetestQuality;
                        public int BuyShare;
                        public int SellShare;
                        public string Regime;
                        public int RegimeQuality;
                        public bool TriggerReady;
                        public bool EntryAllowed;
                        public string BlockReason;
                        public string Reason;
                    }
}
