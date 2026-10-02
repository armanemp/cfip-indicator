using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace cAlgo
{
    public partial class CFIPIndicator
    {
        private const string OutcomeArchiveDirectory = "History";
        private const string OutcomeArchiveSchema = "CFIP-OUTCOME-ARCHIVE,2";

        private string _outcomeArchivePrefixCache;
        private string _outcomeArchivePrefixIdentityCache;

        private string OutcomeArchivePrefix()
        {
            string symbol =
                SanitizeArchivePart(
                    string.IsNullOrWhiteSpace(SymbolName)
                        ? "UNKNOWN"
                        : SymbolName);

            string timeframe =
                SanitizeArchivePart(
                    ExecutionTimeframePolicy.PrimaryExecution);

            string accountScope =
                MemoryAccountScopeToken();

            string identity =
                symbol +
                "|" +
                timeframe +
                "|" +
                accountScope +
                "|" +
                MemoryConfigurationFingerprint();

            if (!string.IsNullOrWhiteSpace(
                    _outcomeArchivePrefixCache) &&
                string.Equals(
                    _outcomeArchivePrefixIdentityCache,
                    identity,
                    StringComparison.Ordinal))
                return _outcomeArchivePrefixCache;

            _outcomeArchivePrefixIdentityCache =
                identity;

            _outcomeArchivePrefixCache =
                "CFIP_History_" +
                symbol +
                "_" +
                timeframe +
                "_" +
                accountScope +
                "_" +
                MemoryConfigurationFingerprint();

            return _outcomeArchivePrefixCache;
        }

        private static string SanitizeArchivePart(
            string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "UNKNOWN";

            StringBuilder result =
                new StringBuilder();

            foreach (char ch in value)
            {
                result.Append(
                    char.IsLetterOrDigit(ch)
                        ? ch
                        : '_');
            }

            return result.ToString();
        }

        private static DateTime OutcomeArchivePeriodStart(
            DateTime observedUtc)
        {
            return CanonicalTimeRule.RollingPeriodStart(
                observedUtc,
                CanonicalTimeRule.OutcomeArchivePeriodDays);
        }

        private string OutcomeArchiveFilePath(
            DateTime observedUtc)
        {
            DateTime start =
                OutcomeArchivePeriodStart(
                    observedUtc);

            DateTime endExclusive =
                start.AddDays(
                    CanonicalTimeRule.OutcomeArchivePeriodDays);

            return
                OutcomeArchiveDirectory +
                Path.DirectorySeparatorChar +
                OutcomeArchivePrefix() +
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

        private string SerializeArchiveObservation(
            OutcomeObservation o)
        {
            StringBuilder row =
                new StringBuilder();

            row.Append(o.PositionId.ToString(CultureInfo.InvariantCulture));
            row.Append(',');
            row.Append(o.Direction.ToString(CultureInfo.InvariantCulture));
            row.Append(',');
            row.Append(((int)o.Lane).ToString(CultureInfo.InvariantCulture));
            row.Append(',');
            row.Append(((int)o.EntryMode).ToString(CultureInfo.InvariantCulture));
            row.Append(',');
            row.Append(Encode(o.Regime));
            row.Append(',');
            row.Append(o.Confidence.ToString(CultureInfo.InvariantCulture));
            row.Append(',');
            row.Append(o.ConfidenceBucket.ToString(CultureInfo.InvariantCulture));
            row.Append(',');
            row.Append(o.CreatedM5.ToString(CultureInfo.InvariantCulture));
            row.Append(',');
            row.Append(o.ClosedM5.ToString(CultureInfo.InvariantCulture));
            row.Append(',');
            row.Append(o.LifecycleBars.ToString(CultureInfo.InvariantCulture));
            row.Append(',');
            row.Append(o.Pips.ToString("R", CultureInfo.InvariantCulture));
            row.Append(',');
            row.Append(o.NetProfit.ToString("R", CultureInfo.InvariantCulture));
            row.Append(',');
            row.Append(o.RealizedR.ToString("R", CultureInfo.InvariantCulture));
            row.Append(',');
            row.Append(o.Profitable ? '1' : '0');
            row.Append(',');
            row.Append(o.CalibrationEligible ? '1' : '0');
            row.Append(',');
            row.Append(o.ProtectionRecoveryAtClose ? '1' : '0');
            row.Append(',');
            row.Append(o.ServerSideTakeProfitLadderActive ? '1' : '0');
            row.Append(',');
            row.Append(o.ObservedUtcTicks.ToString(CultureInfo.InvariantCulture));
            row.Append(',');
            row.Append(Encode(o.SignalTraceId));

            return row.ToString();
        }

        private void ArchiveOutcomeObservation(
            OutcomeObservation observation)
        {
            if (observation == null ||
                observation.PositionId <= 0)
                return;

            try
            {
                string path =
                    OutcomeArchiveFilePath(
                        observation.ObservedUtcTicks > 0
                            ? new DateTime(
                                observation.ObservedUtcTicks,
                                DateTimeKind.Utc)
                            : Server.TimeInUtc);

                bool queued =
                    _bufferedArchivePersistence.Enqueue(
                        path,
                        OutcomeArchiveSchema,
                        SerializeArchiveObservation(
                            observation),
                        observation.PositionId.ToString(
                            CultureInfo.InvariantCulture));

                if (!queued)
                    return;
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP outcome archive queue failed: {0}",
                    ex.Message);
            }
        }

        private bool TryParseArchiveObservation(
            string line,
            out OutcomeObservation observation)
        {
            observation = null;

            if (string.IsNullOrWhiteSpace(line) ||
                line.StartsWith(
                    "CFIP-OUTCOME-ARCHIVE",
                    StringComparison.Ordinal))
                return false;

            string[] parts =
                line.Split(',');

            if (parts.Length < 18)
                return false;

            return TryParseArchiveObservationParts(
                parts,
                out observation);
        }

        private bool TryParseArchiveObservationParts(
            string[] parts,
            out OutcomeObservation observation)
        {
            observation = null;

            long positionId;
            int direction;
            int lane;
            int entryMode;
            int confidence;
            int confidenceBucket;
            int createdM5;
            int closedM5;
            int lifecycleBars;
            double pips;
            double netProfit;
            double realizedR;
            long ticks;

            if (!long.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out positionId) ||
                !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out direction) ||
                !int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out lane) ||
                !int.TryParse(parts[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out entryMode) ||
                !int.TryParse(parts[5], NumberStyles.Integer, CultureInfo.InvariantCulture, out confidence) ||
                !int.TryParse(parts[6], NumberStyles.Integer, CultureInfo.InvariantCulture, out confidenceBucket) ||
                !int.TryParse(parts[7], NumberStyles.Integer, CultureInfo.InvariantCulture, out createdM5) ||
                !int.TryParse(parts[8], NumberStyles.Integer, CultureInfo.InvariantCulture, out closedM5) ||
                !int.TryParse(parts[9], NumberStyles.Integer, CultureInfo.InvariantCulture, out lifecycleBars) ||
                !double.TryParse(parts[10], NumberStyles.Float, CultureInfo.InvariantCulture, out pips) ||
                !double.TryParse(parts[11], NumberStyles.Float, CultureInfo.InvariantCulture, out netProfit) ||
                !double.TryParse(parts[12], NumberStyles.Float, CultureInfo.InvariantCulture, out realizedR) ||
                !long.TryParse(parts[17], NumberStyles.Integer, CultureInfo.InvariantCulture, out ticks))
                return false;

            if (positionId <= 0 ||
                (direction != 1 && direction != -1) ||
                !Enum.IsDefined(
                    typeof(OpportunityLane),
                    lane) ||
                !Enum.IsDefined(
                    typeof(ExecutionMode),
                    entryMode))
                return false;

            observation =
                new OutcomeObservation
                {
                    PositionId = positionId,
                    Direction = direction,
                    Lane = (OpportunityLane)lane,
                    EntryMode = (ExecutionMode)entryMode,
                    Regime = Decode(parts[4]),
                    Confidence = NumericGuards.ClampInt(
                        confidence,
                        0,
                        100),
                    ConfidenceBucket = confidenceBucket,
                    CreatedM5 = createdM5,
                    ClosedM5 = closedM5,
                    LifecycleBars = Math.Max(
                        0,
                        lifecycleBars),
                    Pips = pips,
                    NetProfit = netProfit,
                    RealizedR = realizedR,
                    Profitable = parts[13] == "1",
                    CalibrationEligible = parts[14] == "1",
                    ProtectionRecoveryAtClose = parts[15] == "1",
                    ServerSideTakeProfitLadderActive = parts[16] == "1",
                    ObservedUtcTicks = ticks,
                    SignalTraceId =
                        parts.Length >= 19
                            ? Decode(parts[18])
                            : ""
                };

            return true;
        }

        private void RegisterArchiveLearningObservation(
            OutcomeObservation observation)
        {
            if (observation == null ||
                !observation.CalibrationEligible ||
                (observation.Direction != 1 &&
                 observation.Direction != -1))
                return;

            ConfidenceCalibrationKey key =
                new ConfidenceCalibrationKey(
                    observation.Direction,
                    observation.Lane,
                    observation.Regime,
                    EmpiricalConfidenceCalibrator.ConfidenceBucket(
                        observation.Confidence));

            if (!_archiveCalibrationSamples.ContainsKey(key))
                _archiveCalibrationSamples[key] = 0;

            if (!_archiveCalibrationWins.ContainsKey(key))
                _archiveCalibrationWins[key] = 0;

            _archiveCalibrationSamples[key]++;

            if (observation.Profitable)
                _archiveCalibrationWins[key]++;

            _archiveLearningOutcomeCount++;
        }

        private void ImportOutcomeArchive()
        {
            if (_outcomeArchiveImported)
                return;

            _outcomeArchiveImported = true;

            try
            {
                _archiveCalibrationSamples.Clear();
                _archiveCalibrationWins.Clear();
                _archiveLearningOutcomeCount = 0;

                if (!Directory.Exists(
                        OutcomeArchiveDirectory))
                    return;

                string search =
                    OutcomeArchivePrefix() +
                    "_*.csv";

                string[] files =
                    Directory.GetFiles(
                        OutcomeArchiveDirectory,
                        search);

                Array.Sort(
                    files,
                    StringComparer.Ordinal);

                HashSet<long> archiveIds =
                    new HashSet<long>();

                for (int i = 0; i < files.Length; i++)
                {
                    try
                    {
                        foreach (string line in File.ReadLines(files[i]))
                        {
                            if (!TryParseArchiveObservation(
                                    line,
                                    out OutcomeObservation observation))
                                continue;

                            if (!archiveIds.Add(
                                    observation.PositionId))
                                continue;

                            RegisterArchiveLearningObservation(
                                observation);
                        }
                    }
                    catch (Exception fileException)
                    {
                        Print(
                            "CFIP outcome archive file read failed [{0}]: {1}",
                            files[i],
                            fileException.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP outcome archive import failed: {0}",
                    ex.Message);
            }
        }

        private void QueueOutcomeArchiveImport()
        {
            if (_outcomeArchiveImportQueued ||
                _outcomeArchiveImported)
                return;

            _outcomeArchiveImportQueued = true;

            BeginInvokeOnMainThread(
                () =>
                {
                    _outcomeArchiveImportQueued = false;
                    ImportOutcomeArchive();
                });
        }
    }
}
