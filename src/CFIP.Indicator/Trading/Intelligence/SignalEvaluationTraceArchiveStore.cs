using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace cAlgo
{
    public partial class CFIPIndicator
    {
        private const int MaxSignalEvaluationTraceHistory = 256;
        private const string SignalTraceSchema = "CFIP-SIGNAL-TRACE,1";
        private const string SignalTraceHeader =
            "BarOpenTimeUtcTicks,ObservedUtcTicks,ClosedM5,Open,High,Low,Close,Direction," +
            "BuyShare,SellShare,Edge,BaseConfidence,Confidence,SmartQuality," +
            "HtfAnchorDirection,HtfAlignment,MidframeDirection,MidframeAlignment,EntryFrameAlignment," +
            "TopDownEligible,TopDownStage,Lane,M5BullScore,M5BearScore,M5Evidence,M5Quality," +
            "FvgBullQuality,FvgBearQuality,ObBullQuality,ObBearQuality,FvgObBullConfluence,FvgObBearConfluence," +
            "LocationEvidenceBull,LocationEvidenceBear,IndicatorConfluenceQuality,IndicatorConflict," +
            "WaveTrendDirection,WaveTrendQuality,DivergenceDirection,DivergenceQuality," +
            "EntryAllowed,TriggerReady,ActionableNow,EntryLocationQuality,EntryTimingQuality,EntryPositionQuality," +
            "EntryDistanceAtr,ActionableTp1RR,EntryMode,Entry,IdealEntry,Stop,Tp1,Tp2,Tp3,Tp4," +
            "TraceGate,BlockReason,ActionabilityReason,DecisionReason";

        private readonly List<SignalEvaluationTrace>
            _signalEvaluationTraces =
                new List<SignalEvaluationTrace>();

        private readonly HashSet<long>
            _signalTraceMemoryKeys =
                new HashSet<long>();

        private HashSet<long> _signalTraceArchiveKeys;
        private string _signalTraceArchiveKeyPath;

        private OpportunityLane ResolveSignalTraceLane(
            Decision decision,
            OpportunityLane tacticalLane)
        {
            if (decision != null &&
                decision.TopDownEligible &&
                string.Equals(
                    decision.TopDownStage,
                    "ENTRY CALIBRATED",
                    StringComparison.OrdinalIgnoreCase))
                return OpportunityLane.Strategic;

            if (decision != null &&
                decision.TacticalOpportunityAllowed)
                return decision.TacticalOpportunityLane;

            return tacticalLane;
        }

        private string SignalTraceArchivePrefix()
        {
            string symbol =
                SanitizeArchivePart(
                    string.IsNullOrWhiteSpace(SymbolName)
                        ? "UNKNOWN"
                        : SymbolName);

            string timeframe =
                SanitizeArchivePart(
                    Bars == null
                        ? "UNKNOWN"
                        : Bars.TimeFrame.ToString());

            return
                "CFIP_SignalTrace_" +
                symbol +
                "_" +
                timeframe +
                "_" +
                MemoryConfigurationFingerprint();
        }

        private string SignalTraceArchiveFilePath(
            DateTime observedUtc)
        {
            DateTime start =
                OutcomeArchivePeriodStart(
                    observedUtc);

            DateTime endExclusive =
                start.AddDays(90);

            return
                OutcomeArchiveDirectory +
                Path.DirectorySeparatorChar +
                SignalTraceArchivePrefix() +
                "_" +
                start.ToString(
                    "yyyyMMdd",
                    CultureInfo.InvariantCulture) +
                "_" +
                endExclusive.AddDays(-1).ToString(
                    "yyyyMMdd",
                    CultureInfo.InvariantCulture) +
                ".csv";
        }

        private void PrepareSignalTraceArchiveIndex(
            string path)
        {
            if (string.Equals(
                    _signalTraceArchiveKeyPath,
                    path,
                    StringComparison.Ordinal) &&
                _signalTraceArchiveKeys != null)
                return;

            _signalTraceArchiveKeyPath = path;
            _signalTraceArchiveKeys =
                new HashSet<long>();

            if (!File.Exists(path))
                return;

            try
            {
                foreach (string line in File.ReadLines(path))
                {
                    if (string.IsNullOrWhiteSpace(line) ||
                        line.StartsWith(
                            "CFIP-SIGNAL-TRACE",
                            StringComparison.Ordinal))
                        continue;

                    int comma =
                        line.IndexOf(',');

                    if (comma <= 0)
                        continue;

                    if (long.TryParse(
                            line.Substring(0, comma),
                            NumberStyles.Integer,
                            CultureInfo.InvariantCulture,
                            out long key))
                    {
                        _signalTraceArchiveKeys.Add(key);
                    }
                }
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP signal trace archive index load failed: {0}",
                    ex.Message);
            }
        }

        private string SerializeSignalTrace(
            SignalEvaluationTrace trace)
        {
            StringBuilder row =
                new StringBuilder();

            row.Append(trace.BarOpenTimeUtcTicks.ToString(CultureInfo.InvariantCulture));
            row.Append(',');
            row.Append(trace.ObservedUtcTicks.ToString(CultureInfo.InvariantCulture));
            row.Append(',');
            row.Append(trace.ClosedM5.ToString(CultureInfo.InvariantCulture));
            row.Append(',');
            row.Append(trace.Open.ToString("R", CultureInfo.InvariantCulture));
            row.Append(',');
            row.Append(trace.High.ToString("R", CultureInfo.InvariantCulture));
            row.Append(',');
            row.Append(trace.Low.ToString("R", CultureInfo.InvariantCulture));
            row.Append(',');
            row.Append(trace.Close.ToString("R", CultureInfo.InvariantCulture));
            row.Append(',');
            row.Append(trace.Direction);
            row.Append(',');
            row.Append(trace.BuyShare);
            row.Append(',');
            row.Append(trace.SellShare);
            row.Append(',');
            row.Append(trace.Edge);
            row.Append(',');
            row.Append(trace.BaseConfidence);
            row.Append(',');
            row.Append(trace.Confidence);
            row.Append(',');
            row.Append(trace.SmartQuality);
            row.Append(',');
            row.Append(trace.HtfAnchorDirection);
            row.Append(',');
            row.Append(trace.HtfAlignment);
            row.Append(',');
            row.Append(trace.MidframeDirection);
            row.Append(',');
            row.Append(trace.MidframeAlignment);
            row.Append(',');
            row.Append(trace.EntryFrameAlignment);
            row.Append(',');
            row.Append(trace.TopDownEligible);
            row.Append(',');
            row.Append(Encode(trace.TopDownStage));
            row.Append(',');
            row.Append((int)trace.Lane);
            row.Append(',');
            row.Append(trace.M5BullScore);
            row.Append(',');
            row.Append(trace.M5BearScore);
            row.Append(',');
            row.Append(trace.M5Evidence);
            row.Append(',');
            row.Append(trace.M5Quality);
            row.Append(',');
            row.Append(trace.FvgBullQuality);
            row.Append(',');
            row.Append(trace.FvgBearQuality);
            row.Append(',');
            row.Append(trace.ObBullQuality);
            row.Append(',');
            row.Append(trace.ObBearQuality);
            row.Append(',');
            row.Append(trace.FvgObBullConfluence);
            row.Append(',');
            row.Append(trace.FvgObBearConfluence);
            row.Append(',');
            row.Append(trace.LocationEvidenceBull);
            row.Append(',');
            row.Append(trace.LocationEvidenceBear);
            row.Append(',');
            row.Append(trace.IndicatorConfluenceQuality);
            row.Append(',');
            row.Append(trace.IndicatorConflict);
            row.Append(',');
            row.Append(trace.WaveTrendDirection);
            row.Append(',');
            row.Append(trace.WaveTrendQuality);
            row.Append(',');
            row.Append(trace.DivergenceDirection);
            row.Append(',');
            row.Append(trace.DivergenceQuality);
            row.Append(',');
            row.Append(trace.EntryAllowed);
            row.Append(',');
            row.Append(trace.TriggerReady);
            row.Append(',');
            row.Append(trace.ActionableNow);
            row.Append(',');
            row.Append(trace.EntryLocationQuality);
            row.Append(',');
            row.Append(trace.EntryTimingQuality);
            row.Append(',');
            row.Append(trace.EntryPositionQuality);
            row.Append(',');
            row.Append(trace.EntryDistanceAtr.ToString("R", CultureInfo.InvariantCulture));
            row.Append(',');
            row.Append(trace.ActionableTp1RR.ToString("R", CultureInfo.InvariantCulture));
            row.Append(',');
            row.Append((int)trace.EntryMode);
            row.Append(',');
            row.Append(trace.Entry.ToString("R", CultureInfo.InvariantCulture));
            row.Append(',');
            row.Append(trace.IdealEntry.ToString("R", CultureInfo.InvariantCulture));
            row.Append(',');
            row.Append(trace.Stop.ToString("R", CultureInfo.InvariantCulture));
            row.Append(',');
            row.Append(trace.Tp1.ToString("R", CultureInfo.InvariantCulture));
            row.Append(',');
            row.Append(trace.Tp2.ToString("R", CultureInfo.InvariantCulture));
            row.Append(',');
            row.Append(trace.Tp3.ToString("R", CultureInfo.InvariantCulture));
            row.Append(',');
            row.Append(trace.Tp4.ToString("R", CultureInfo.InvariantCulture));
            row.Append(',');
            row.Append(Encode(trace.TraceGate));
            row.Append(',');
            row.Append(Encode(trace.BlockReason));
            row.Append(',');
            row.Append(Encode(trace.ActionabilityReason));
            row.Append(',');
            row.Append(Encode(trace.DecisionReason));

            return row.ToString();
        }

        private bool ArchiveSignalTrace(
            SignalEvaluationTrace trace)
        {
            if (trace == null ||
                trace.BarOpenTimeUtcTicks <= 0)
                return false;

            try
            {
                Directory.CreateDirectory(
                    OutcomeArchiveDirectory);

                DateTime observed =
                    trace.ObservedUtcTicks > 0
                        ? new DateTime(
                            trace.ObservedUtcTicks,
                            DateTimeKind.Utc)
                        : Server.TimeInUtc;

                string path =
                    SignalTraceArchiveFilePath(
                        observed);

                PrepareSignalTraceArchiveIndex(path);

                if (_signalTraceArchiveKeys.Contains(
                        trace.BarOpenTimeUtcTicks))
                    return false;

                if (!File.Exists(path))
                {
                    File.WriteAllText(
                        path,
                        SignalTraceSchema +
                        Environment.NewLine +
                        SignalTraceHeader +
                        Environment.NewLine,
                        Encoding.UTF8);
                }

                File.AppendAllText(
                    path,
                    SerializeSignalTrace(trace) +
                    Environment.NewLine,
                    Encoding.UTF8);

                _signalTraceArchiveKeys.Add(
                    trace.BarOpenTimeUtcTicks);

                return true;
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP signal trace archive persist failed: {0}",
                    ex.Message);
                return false;
            }
        }

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
