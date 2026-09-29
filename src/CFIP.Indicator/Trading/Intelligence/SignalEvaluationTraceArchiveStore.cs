using System;
using System.Collections.Generic;

namespace cAlgo
{
    public partial class CFIPIndicator
    {
        private const int MaxSignalEvaluationTraceHistory = 256;

        private readonly List<SignalEvaluationTrace>
            _signalEvaluationTraces =
                new List<SignalEvaluationTrace>();

        private readonly HashSet<long>
            _signalTraceMemoryKeys =
                new HashSet<long>();

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

            if (!EnableOutcomeTelemetry)
                return;

            DateTime barOpen =
                _m5Bars.OpenTimes[closedM5];

            long key =
                barOpen.Ticks;

            if (key <= 0 ||
                _signalTraceMemoryKeys.Contains(key))
                return;

            SignalEvaluationTrace trace =
                new SignalEvaluationTrace
                {
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
                    EntryMode =
                        _executionModel == null
                            ? ExecutionMode.None
                            : _executionModel.Mode,
                    Entry =
                        _setupPreview == null
                            ? 0
                            : _setupPreview.Entry,
                    IdealEntry =
                        _setupPreview == null
                            ? 0
                            : _setupPreview.IdealEntry,
                    Stop =
                        _setupPreview == null
                            ? 0
                            : _setupPreview.Stop,
                    Tp1 =
                        _setupPreview == null
                            ? 0
                            : _setupPreview.Tp1,
                    Tp2 =
                        _setupPreview == null
                            ? 0
                            : _setupPreview.Tp2,
                    Tp3 =
                        _setupPreview == null
                            ? 0
                            : _setupPreview.Tp3,
                    Tp4 =
                        _setupPreview == null
                            ? 0
                            : _setupPreview.Tp4,
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
            if (!EnableOutcomeTelemetry ||
                _signalEvaluationTraces == null ||
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