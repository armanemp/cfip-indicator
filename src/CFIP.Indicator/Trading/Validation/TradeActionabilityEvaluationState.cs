using System;

namespace cAlgo
{
    internal sealed class TradeActionabilityEvaluationState
    {
        public int ClosedM5;
        public int Direction;
        public OpportunityLane Lane;
        public string Regime;

        public ExecutionModel Execution;
        public TradeSetupPreview InputPreview;
        public TradeSetupPreview EffectivePreview;
        public CanonicalTradePathGeometry CanonicalPath;

        public double Atr;
        public double Market;
        public double ActualEntry;
        public double EntryDistanceAtr;
        public double ZoneDistanceAtr;
        public ExecutionMode LiveMode;

        public bool InsideZone;
        public bool QualityReady;
        public bool Late;
        public bool RetestTrapContext;

        public double AdverseM5Atr;
        public bool M5AdverseEvidenceKnown;
        public bool M5AdversePreZone;

        public double AdverseM1Atr;
        public bool M1AdverseEvidenceKnown;
        public bool M1DirectionConflict;
        public bool M1AdversePreZone;

        public double RangePosition;
        public bool OpposingRegularDivergence;
        public bool SupportiveHiddenDivergence;
        public bool MicroConflict;

        public int LocationQuality;
        public int TimingQuality;
        public int PricePositionQuality;

        public double Tp1RR;

        public DivergenceResult Divergence;
        public EntryTrapRiskResult TrapRisk;
        public PlanRewardRiskQualityResult RewardRisk;
        public string IndicatorGateReason;

        public TradeActionabilityResult ToBlockedResult(
            string reason,
            bool preserveMetrics)
        {
            if (!preserveMetrics)
                return TradeActionabilityResult.Blocked(reason);

            return new TradeActionabilityResult(
                false,
                LocationQuality,
                TimingQuality,
                PricePositionQuality,
                EntryDistanceAtr,
                Math.Max(0, Tp1RR),
                Divergence.Quality,
                Divergence.Direction,
                Divergence.Type,
                reason);
        }
    }
}
