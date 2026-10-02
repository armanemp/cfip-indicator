using System;

namespace cAlgo
{
    public partial class CFIPIndicator
    {
        private void RecordSignalEvaluationTrace(
            Decision decision,
            OpportunityLane lane,
            int closedM5)
        {
            if (decision == null ||
                _m5Bars == null ||
                closedM5 < 0 ||
                closedM5 >= _m5Bars.Count)
                return;

            DateTime barOpen =
                _m5Bars.OpenTimes[closedM5];

            long key =
                barOpen.Ticks;

            if (key <= 0 ||
                _signalTraceMemoryKeys.Contains(key))
                return;

            string traceId =
                SignalTraceIdentityRule.CreateTraceId(
                    SymbolName,
                    ExecutionTimeframePolicy.PrimaryExecution,
                    MemoryAccountScopeToken(),
                    MemoryConfigurationFingerprint(),
                    key);

            if (string.IsNullOrWhiteSpace(traceId))
                return;

            ExecutionMode entryMode;
            double entry;
            double idealEntry;
            double stop;
            double tp1;
            double tp2;
            double tp3;
            double tp4;
            double planRiskAtr;
            double effectiveTp1RR;
            long geometryBarTicks;
            string geometrySource;

            ResolveTraceGeometry(
                decision,
                closedM5,
                key,
                traceId,
                out entryMode,
                out entry,
                out idealEntry,
                out stop,
                out tp1,
                out tp2,
                out tp3,
                out tp4,
                out planRiskAtr,
                out effectiveTp1RR,
                out geometryBarTicks,
                out geometrySource);

            SignalEvaluationTrace trace =
                new SignalEvaluationTrace
                {
                    SignalTraceId = traceId,
                    BarOpenTimeUtcTicks = key,
                    ObservedUtcTicks = Server.TimeInUtc.Ticks,
                    ClosedM5 = closedM5,
                    Open = _m5Bars.OpenPrices[closedM5],
                    High = _m5Bars.HighPrices[closedM5],
                    Low = _m5Bars.LowPrices[closedM5],
                    Close = _m5Bars.ClosePrices[closedM5],
                    Direction = decision.Direction,
                    BuyShare = decision.BuyShare,
                    SellShare = decision.SellShare,
                    Edge = decision.Edge,
                    BaseConfidence = decision.BaseConfidence,
                    Confidence = decision.Confidence,
                    SmartQuality = decision.SmartQuality,
                    HtfAnchorDirection = decision.HtfAnchorDirection,
                    HtfAlignment = decision.HtfAlignment,
                    MidframeDirection = decision.MidframeDirection,
                    MidframeAlignment = decision.MidframeAlignment,
                    EntryFrameAlignment = decision.EntryFrameAlignment,
                    TopDownEligible =
                        decision.TopDownEligible ? 1 : 0,
                    TopDownStage =
                        decision.TopDownStage ?? "UNKNOWN",
                    Lane = lane,
                    M5BullScore =
                        _m5Frame == null ? 0 : _m5Frame.BullScore,
                    M5BearScore =
                        _m5Frame == null ? 0 : _m5Frame.BearScore,
                    M5Evidence =
                        _m5Frame == null ? 0 : _m5Frame.Evidence,
                    M5Quality =
                        _m5Frame == null ? 0 : _m5Frame.Quality,
                    FvgBullQuality =
                        _m5Frame == null ? 0 : _m5Frame.FvgBullQuality,
                    FvgBearQuality =
                        _m5Frame == null ? 0 : _m5Frame.FvgBearQuality,
                    ObBullQuality =
                        _m5Frame == null ? 0 : _m5Frame.ObBullQuality,
                    ObBearQuality =
                        _m5Frame == null ? 0 : _m5Frame.ObBearQuality,
                    FvgObBullConfluence =
                        _m5Frame != null &&
                        _m5Frame.FvgObBullConfluence ? 1 : 0,
                    FvgObBearConfluence =
                        _m5Frame != null &&
                        _m5Frame.FvgObBearConfluence ? 1 : 0,
                    LocationEvidenceBull =
                        _m5Frame == null
                            ? 0
                            : _m5Frame.LocationEvidenceBull,
                    LocationEvidenceBear =
                        _m5Frame == null
                            ? 0
                            : _m5Frame.LocationEvidenceBear,
                    IndicatorConfluenceQuality =
                        decision.IndicatorConfluenceQuality,
                    IndicatorConflict =
                        decision.IndicatorConflict,
                    WaveTrendDirection =
                        _m5Frame == null
                            ? 0
                            : _m5Frame.WaveTrendDirection,
                    WaveTrendQuality =
                        _m5Frame == null
                            ? 0
                            : _m5Frame.WaveTrendQuality,
                    DivergenceDirection =
                        decision.DivergenceDirection,
                    DivergenceQuality =
                        decision.DivergenceQuality,
                    EntryAllowed =
                        decision.EntryAllowed ? 1 : 0,
                    TriggerReady =
                        decision.TriggerReady ? 1 : 0,
                    ActionableNow =
                        decision.ActionableNow ? 1 : 0,
                    EntryLocationQuality =
                        decision.EntryLocationQuality,
                    EntryTimingQuality =
                        decision.EntryTimingQuality,
                    EntryPositionQuality =
                        decision.EntryPositionQuality,
                    EntryDistanceAtr =
                        decision.EntryDistanceAtr,
                    ActionableTp1RR =
                        decision.ActionableTp1RR,
                    PlanRiskAtr =
                        planRiskAtr,
                    EffectiveTp1RR =
                        effectiveTp1RR,
                    RequiredTp1RR =
                        TraceRequiredTp1RR(
                            decision.Regime),
                    GeometryBarOpenTimeUtcTicks =
                        geometryBarTicks,
                    GeometrySource =
                        geometrySource,
                    EntryMode = entryMode,
                    Entry = entry,
                    IdealEntry = idealEntry,
                    Stop = stop,
                    Tp1 = tp1,
                    Tp2 = tp2,
                    Tp3 = tp3,
                    Tp4 = tp4,
                    TraceGate =
                        ResolveSignalTraceGate(
                            decision),
                    BlockReason =
                        decision.BlockReason ?? "",
                    ActionabilityReason =
                        decision.ActionabilityReason ?? "",
                    DecisionReason =
                        decision.Reason ?? ""
                };

            while (_signalEvaluationTraces.Count >=
                   MaxSignalEvaluationTraceHistory)
            {
                SignalEvaluationTrace oldest =
                    _signalEvaluationTraces[0];

                _signalEvaluationTraces.RemoveAt(0);

                if (oldest != null)
                    _signalTraceMemoryKeys.Remove(
                        oldest.BarOpenTimeUtcTicks);
            }

            _signalEvaluationTraces.Add(trace);
            _signalTraceMemoryKeys.Add(key);

            ArchiveSignalTrace(trace);
            ArchiveRuntimeDecisionTrace(trace);
        }

        private string BuildSignalTraceId(
            int closedM5)
        {
            return SignalTraceIdentityRule.CreateTraceId(
                SymbolName,
                ExecutionTimeframePolicy.PrimaryExecution,
                MemoryAccountScopeToken(),
                MemoryConfigurationFingerprint(),
                GetSignalBarOpenTimeUtcTicks(closedM5));
        }

        private long GetSignalBarOpenTimeUtcTicks(
            int closedM5)
        {
            if (_m5Bars == null ||
                closedM5 < 0 ||
                closedM5 >= _m5Bars.Count)
                return 0;

            return _m5Bars.OpenTimes[closedM5].Ticks;
        }

        private void ResolveTraceGeometry(
            Decision decision,
            int closedM5,
            long barOpenTimeUtcTicks,
            string traceId,
            out ExecutionMode entryMode,
            out double entry,
            out double idealEntry,
            out double stop,
            out double tp1,
            out double tp2,
            out double tp3,
            out double tp4,
            out double planRiskAtr,
            out double effectiveTp1RR,
            out long geometryBarTicks,
            out string geometrySource)
        {
            entryMode = ExecutionMode.None;
            entry = 0;
            idealEntry = 0;
            stop = 0;
            tp1 = 0;
            tp2 = 0;
            tp3 = 0;
            tp4 = 0;
            planRiskAtr = 0;
            effectiveTp1RR = 0;
            geometryBarTicks = 0;
            geometrySource = "NONE";

            if (decision == null ||
                closedM5 < 0 ||
                barOpenTimeUtcTicks <= 0)
                return;

            if (_plan != null &&
                _plan.Direction == decision.Direction &&
                SignalTraceLineageRule.MatchesClosedBar(
                    closedM5,
                    barOpenTimeUtcTicks,
                    _plan.CreatedM5,
                    _plan.SignalBarOpenTimeUtcTicks) &&
                SignalTraceLineageRule.CanJoinOutcome(
                    traceId,
                    _plan.SignalTraceId))
            {
                entryMode = _plan.EntryMode;
                entry = _plan.Entry;
                idealEntry = _plan.IdealEntry;
                stop = _plan.Stop;
                tp1 = _plan.Tp1;
                tp2 = _plan.Tp2;
                tp3 = _plan.Tp3;
                tp4 = _plan.Tp4;
                geometryBarTicks =
                    _plan.SignalBarOpenTimeUtcTicks;
                geometrySource = "PLAN";

                ApplyTraceRewardMetrics(
                    decision,
                    closedM5,
                    entry,
                    stop,
                    tp1,
                    out planRiskAtr,
                    out effectiveTp1RR);

                return;
            }

            if (_setupPreview != null &&
                _setupPreview.Direction == decision.Direction &&
                SignalTraceLineageRule.MatchesClosedBar(
                    closedM5,
                    barOpenTimeUtcTicks,
                    _setupPreview.CreatedM5,
                    barOpenTimeUtcTicks))
            {
                entryMode = _setupPreview.EntryMode;
                entry = _setupPreview.Entry;
                idealEntry = _setupPreview.IdealEntry;
                stop = _setupPreview.Stop;
                tp1 = _setupPreview.Tp1;
                tp2 = _setupPreview.Tp2;
                tp3 = _setupPreview.Tp3;
                tp4 = _setupPreview.Tp4;
                geometryBarTicks = barOpenTimeUtcTicks;
                geometrySource = "PREVIEW";

                ApplyTraceRewardMetrics(
                    decision,
                    closedM5,
                    entry,
                    stop,
                    tp1,
                    out planRiskAtr,
                    out effectiveTp1RR);
            }
        }

        private void ApplyTraceRewardMetrics(
            Decision decision,
            int closedM5,
            double entry,
            double stop,
            double tp1,
            out double planRiskAtr,
            out double effectiveTp1RR)
        {
            planRiskAtr = 0;
            effectiveTp1RR = 0;

            if (_m5Bars == null ||
                closedM5 < 0 ||
                closedM5 >= _m5Bars.Count ||
                !IsFinitePositive(entry) ||
                !IsFinitePositive(stop) ||
                !IsFinitePositive(tp1))
                return;

            double atr =
                Atr(
                    _m5Bars,
                    closedM5);

            planRiskAtr =
                atr > 0
                    ? Math.Abs(
                        entry - stop) / atr
                    : 0;

            PlanRewardRiskQualityResult rewardRisk =
                PlanRewardRiskQualityRule.Evaluate(
                    decision == null
                        ? 0
                        : decision.Direction,
                    entry,
                    stop,
                    tp1,
                    atr,
                    Math.Max(
                        0,
                        Symbol.Ask - Symbol.Bid),
                    Math.Max(
                        Tp1MinimumRR,
                        MinimumRequiredRRForRegime(
                            decision == null
                                ? "UNKNOWN"
                                : decision.Regime)),
                    PreferredStopRiskAtr,
                    StructuralStopRiskRule.EffectiveMaximumStopRiskAtr(
                        MinimumSlAtr,
                        MaximumSlAtr,
                        MaximumStructuralStopAtr),
                        Math.Max(0, MaximumRewardRR),
                        Symbol.PipSize);

            effectiveTp1RR = rewardRisk.EffectiveRR;
        }

        private double TraceRequiredTp1RR(
            string regime)
        {
            return Math.Max(
                Tp1MinimumRR,
                MinimumRequiredRRForRegime(
                    regime));
        }

        private static string ResolveSignalTraceGate(
            Decision decision)
        {
            if (decision == null)
                return "NO-DECISION";

            if (decision.Direction == 0)
                return "CONSENSUS";

            if (!decision.EntryAllowed)
                return "DECISION-FILTER";

            if (!decision.TriggerReady)
                return "TRIGGER";

            if (!decision.ActionableNow)
                return "ACTIONABILITY";

            return "ACTIONABLE";
        }

        private string SignalTracePanelText()
        {
            if (_signalEvaluationTraces == null ||
                _signalEvaluationTraces.Count == 0)
                return "TRACE 0";

            SignalEvaluationTrace latest =
                _signalEvaluationTraces[
                    _signalEvaluationTraces.Count - 1];

            if (latest == null)
                return "TRACE 0";

            return
                "TRACE " +
                _signalEvaluationTraces.Count +
                " • LAST " +
                latest.TraceGate +
                (string.IsNullOrWhiteSpace(
                    latest.ActionabilityReason)
                    ? ""
                    : " • " +
                      latest.ActionabilityReason);
        }
    }
}
