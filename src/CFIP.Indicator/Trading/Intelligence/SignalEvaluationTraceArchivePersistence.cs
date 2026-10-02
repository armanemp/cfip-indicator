using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace cAlgo
{
    public partial class CFIPIndicator
    {
        private const string SignalTraceSchema = "CFIP-SIGNAL-TRACE,3";

        private string _signalTraceArchivePrefixCache;
        private string _signalTraceArchivePrefixIdentityCache;
        private const string SignalTraceHeader =
            "BarOpenTimeUtcTicks,ObservedUtcTicks,ClosedM5,Open,High,Low,Close,Direction," +
            "BuyShare,SellShare,Edge,BaseConfidence,Confidence,SmartQuality," +
            "HtfAnchorDirection,HtfAlignment,MidframeDirection,MidframeAlignment,EntryFrameAlignment," +
            "TopDownEligible,TopDownStage,Lane,M5BullScore,M5BearScore,M5Evidence,M5Quality," +
            "FvgBullQuality,FvgBearQuality,ObBullQuality,ObBearQuality,FvgObBullConfluence,FvgObBearConfluence," +
            "LocationEvidenceBull,LocationEvidenceBear,IndicatorConfluenceQuality,IndicatorConflict," +
            "WaveTrendDirection,WaveTrendQuality,DivergenceDirection,DivergenceQuality," +
            "EntryAllowed,TriggerReady,ActionableNow,EntryLocationQuality,EntryTimingQuality,EntryPositionQuality," +
            "EntryDistanceAtr,ActionableTp1RR,PlanRiskAtr,EffectiveTp1RR,RequiredTp1RR,EntryMode,Entry,IdealEntry,Stop,Tp1,Tp2,Tp3,Tp4," +
            "TraceGate,BlockReason,ActionabilityReason,DecisionReason," +
            "SignalTraceId,GeometryBarOpenTimeUtcTicks,GeometrySource";

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
                    ExecutionTimeframePolicy.PrimaryExecution);

            string identity =
                symbol +
                "|" +
                timeframe +
                "|" +
                MemoryAccountScopeToken() +
                "|" +
                MemoryConfigurationFingerprint();

            if (!string.IsNullOrWhiteSpace(
                    _signalTraceArchivePrefixCache) &&
                string.Equals(
                    _signalTraceArchivePrefixIdentityCache,
                    identity,
                    StringComparison.Ordinal))
                return _signalTraceArchivePrefixCache;

            _signalTraceArchivePrefixIdentityCache =
                identity;

            _signalTraceArchivePrefixCache =
                "CFIP_SignalTrace_v2_" +
                symbol +
                "_" +
                timeframe +
                "_" +
                MemoryAccountScopeToken() +
                "_" +
                MemoryConfigurationFingerprint();

            return
                _signalTraceArchivePrefixCache;
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
            row.Append(trace.PlanRiskAtr.ToString("R", CultureInfo.InvariantCulture));
            row.Append(',');
            row.Append(trace.EffectiveTp1RR.ToString("R", CultureInfo.InvariantCulture));
            row.Append(',');
            row.Append(trace.RequiredTp1RR.ToString("R", CultureInfo.InvariantCulture));
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
            row.Append(',');
            row.Append(Encode(trace.SignalTraceId));
            row.Append(',');
            row.Append(trace.GeometryBarOpenTimeUtcTicks.ToString(CultureInfo.InvariantCulture));
            row.Append(',');
            row.Append(Encode(trace.GeometrySource));

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
                DateTime observed =
                    trace.ObservedUtcTicks > 0
                        ? new DateTime(
                            trace.ObservedUtcTicks,
                            DateTimeKind.Utc)
                        : Server.TimeInUtc;

                return _bufferedArchivePersistence.Enqueue(
                    SignalTraceArchiveFilePath(
                        observed),
                    SignalTraceSchema +
                    Environment.NewLine +
                    SignalTraceHeader,
                    SerializeSignalTrace(
                        trace),
                    trace.BarOpenTimeUtcTicks.ToString(
                        CultureInfo.InvariantCulture));
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP signal trace queue failed: {0}",
                    ex.Message);
                return false;
            }
        }
    }
}