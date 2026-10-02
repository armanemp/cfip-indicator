using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace cAlgo
{
    public partial class CFIPIndicator
    {
        private const string PortableMemorySnapshotSchema =
            "CFIP-PORTABLE-MEMORY,2";

        private const string PriorPortableMemorySnapshotSchema =
            "CFIP-PORTABLE-MEMORY,1";

        // The indicator is explicitly registered as CFIPIndicator. With
        // AccessRights.None, cTrader's designated indicator storage root is
        // Documents/cAlgo/Data/Indicators/<Indicator Name>. Keep this exact
        // identity here so the history location is deterministic and easy
        // to verify from the terminal file system.
        private const string IndicatorStorageFolderName =
            "CFIPIndicator";

        private const string HistoryLocationMarkerFileName =
            "CFIP_HISTORY_LOCATION.txt";

        private string HistoryLocationMarkerPath()
        {
            return
                OutcomeArchiveDirectory +
                Path.DirectorySeparatorChar +
                HistoryLocationMarkerFileName;
        }

        private string HistoryLocationMarkerText()
        {
            return
                "CFIP HISTORY STORAGE" + Environment.NewLine +
                "IndicatorName=" + IndicatorStorageFolderName + Environment.NewLine +
                "RelativeHistoryPath=History" + Environment.NewLine +
                "DesignatedStorageRoot=Documents/cAlgo/Data/Indicators/" +
                IndicatorStorageFolderName + Environment.NewLine +
                "DesignatedHistoryPath=Documents/cAlgo/Data/Indicators/" +
                IndicatorStorageFolderName +
                Path.DirectorySeparatorChar +
                "History" + Environment.NewLine +
                "CreatedUtcTicks=" +
                Server.TimeInUtc.Ticks.ToString(CultureInfo.InvariantCulture) +
                Environment.NewLine;
        }

        private void EnsureHistoryLocationMarker()
        {
            try
            {
                if (!_bufferedArchivePersistence.TryEnsureDirectory(
                        OutcomeArchiveDirectory))
                    return;

                string marker =
                    HistoryLocationMarkerPath();

                if (!_bufferedArchivePersistence.FileExists(marker))
                {
                    _bufferedArchivePersistence.TryWriteAllText(
                        marker,
                        HistoryLocationMarkerText(),
                        Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP history location marker failed: {0}",
                    ex.Message);
            }
        }

        private string PortableMemorySnapshotPath()
        {
            string symbol =
                SanitizeArchivePart(
                    string.IsNullOrWhiteSpace(SymbolName)
                        ? "UNKNOWN"
                        : SymbolName);

            string timeframe =
                SanitizeArchivePart(
                    ExecutionTimeframePolicy.PrimaryExecution);

            return
                OutcomeArchiveDirectory +
                Path.DirectorySeparatorChar +
                "CFIP_PortableMemory_" +
                symbol +
                "_" +
                timeframe +
                "_" +
                MemoryAccountScopeToken() +
                "_" +
                MemoryConfigurationFingerprint() +
                ".txt";
        }

        private string PriorPortableMemorySnapshotPath()
        {
            string symbol =
                SanitizeArchivePart(
                    string.IsNullOrWhiteSpace(SymbolName)
                        ? "UNKNOWN"
                        : SymbolName);

            string timeframe =
                SanitizeArchivePart(
                    ExecutionTimeframePolicy.PrimaryExecution);

            return
                OutcomeArchiveDirectory +
                Path.DirectorySeparatorChar +
                "CFIP_PortableMemory_" +
                symbol +
                "_" +
                timeframe +
                "_" +
                PriorMemoryConfigurationFingerprint() +
                ".txt";
        }

        private string PortableMemorySnapshotText()
        {
            string payload =
                SerializeOutcomeHistory();

            byte[] bytes =
                Encoding.UTF8.GetBytes(
                    payload ?? "");

            StringBuilder text =
                new StringBuilder();

            text.Append(
                PortableMemorySnapshotSchema);
            text.Append(Environment.NewLine);

            text.Append("GeneratedUtcTicks=");
            text.Append(
                Server.TimeInUtc.Ticks.ToString(
                    CultureInfo.InvariantCulture));
            text.Append(Environment.NewLine);

            text.Append("Symbol=");
            text.Append(SymbolName ?? "UNKNOWN");
            text.Append(Environment.NewLine);

            text.Append("TimeFrame=");
            text.Append(
                ExecutionTimeframePolicy.PrimaryExecution);
            text.Append(Environment.NewLine);

            text.Append("AccountNumber=");
            text.Append(
                Account.Number.ToString(
                    CultureInfo.InvariantCulture));
            text.Append(Environment.NewLine);

            text.Append("AccountType=");
            text.Append(
                Account.AccountType.ToString());
            text.Append(Environment.NewLine);

            text.Append("AccountIsLive=");
            text.Append(
                Account.IsLive ? "1" : "0");
            text.Append(Environment.NewLine);

            text.Append("AccountBroker=");
            text.Append(
                Account.BrokerName ?? "UNKNOWN");
            text.Append(Environment.NewLine);

            text.Append("Fingerprint=");
            text.Append(
                MemoryConfigurationFingerprint());
            text.Append(Environment.NewLine);

            text.Append("LocalStorageScope=Type");
            text.Append(Environment.NewLine);

            text.Append("OutcomeMemoryKey=");
            text.Append(OutcomeMemoryKey());
            text.Append(Environment.NewLine);

            text.Append("OutcomeHistoryCount=");
            text.Append(
                _outcomeHistory == null
                    ? 0
                    : _outcomeHistory.Count);
            text.Append(Environment.NewLine);

            text.Append("HistoryDirectory=History");
            text.Append(Environment.NewLine);

            text.Append("OutcomeArchivePrefix=");
            text.Append(OutcomeArchivePrefix());
            text.Append(Environment.NewLine);

            text.Append("SignalTraceArchivePrefix=");
            text.Append(SignalTraceArchivePrefix());
            text.Append(Environment.NewLine);

            text.Append("OutcomePayloadBase64=");
            text.Append(
                Convert.ToBase64String(bytes));
            text.Append(Environment.NewLine);

            text.Append(
                "CopyInstruction=Copy the entire History folder to the same CFIP indicator folder on the destination device.");
            text.Append(Environment.NewLine);

            text.Append(
                "LocalStorageInstruction=When the matching Type-scoped LocalStorage key is absent, CFIP restores this bounded outcome cache automatically.");
            text.Append(Environment.NewLine);

            return text.ToString();
        }

        private bool TryRestorePortableMemorySnapshot(
            out string payload)
        {
            payload = "";

            try
            {
                string path =
                    PortableMemorySnapshotPath();

                bool priorPath =
                    false;

                if (!File.Exists(path))
                {
                    path =
                        PriorPortableMemorySnapshotPath();

                    priorPath = true;
                }

                if (!File.Exists(path))
                    return false;

                string[] lines;

                if (!_bufferedArchivePersistence.TryReadAllLines(
                        path,
                        out lines) ||
                    lines == null)
                    return false;

                if (lines.Length == 0)
                    return false;

                string schema =
                    lines[0].Trim();

                bool currentSchema =
                    string.Equals(
                        schema,
                        PortableMemorySnapshotSchema,
                        StringComparison.Ordinal);

                bool priorSchema =
                    string.Equals(
                        schema,
                        PriorPortableMemorySnapshotSchema,
                        StringComparison.Ordinal);

                if (!currentSchema &&
                    !priorSchema)
                    return false;

                string fingerprint = "";
                string encoded = "";
                string accountNumber = "";
                string accountType = "";
                string accountIsLive = "";
                string accountBroker = "";

                for (int i = 1;
                     i < lines.Length;
                     i++)
                {
                    string line =
                        lines[i] ?? "";

                    int separator =
                        line.IndexOf('=');

                    if (separator <= 0)
                        continue;

                    string key =
                        line.Substring(
                            0,
                            separator);

                    string value =
                        line.Substring(
                            separator + 1);

                    if (string.Equals(
                            key,
                            "Fingerprint",
                            StringComparison.Ordinal))
                    {
                        fingerprint = value;
                    }
                    else if (string.Equals(
                                 key,
                                 "OutcomePayloadBase64",
                                 StringComparison.Ordinal))
                    {
                        encoded = value;
                    }
                    else if (string.Equals(
                                 key,
                                 "AccountNumber",
                                 StringComparison.Ordinal))
                    {
                        accountNumber = value;
                    }
                    else if (string.Equals(
                                 key,
                                 "AccountType",
                                 StringComparison.Ordinal))
                    {
                        accountType = value;
                    }
                    else if (string.Equals(
                                 key,
                                 "AccountIsLive",
                                 StringComparison.Ordinal))
                    {
                        accountIsLive = value;
                    }
                    else if (string.Equals(
                                 key,
                                 "AccountBroker",
                                 StringComparison.Ordinal))
                    {
                        accountBroker = value;
                    }
                }

                if (string.IsNullOrWhiteSpace(encoded))
                    return false;

                if (currentSchema)
                {
                    string currentAccountNumber =
                        Account.Number.ToString(
                            CultureInfo.InvariantCulture);

                    if (!string.Equals(
                            fingerprint,
                            MemoryConfigurationFingerprint(),
                            StringComparison.Ordinal) ||
                        !string.Equals(
                            accountNumber,
                            currentAccountNumber,
                            StringComparison.Ordinal) ||
                        !string.Equals(
                            accountType,
                            Account.AccountType.ToString(),
                            StringComparison.Ordinal) ||
                        !string.Equals(
                            accountIsLive,
                            Account.IsLive ? "1" : "0",
                            StringComparison.Ordinal) ||
                        !string.Equals(
                            accountBroker,
                            Account.BrokerName ?? "UNKNOWN",
                            StringComparison.Ordinal))
                        return false;
                }
                else
                {
                    // Legacy snapshots did not carry account identity. Keep
                    // them eligible only when their legacy configuration
                    // fingerprint matches; RestoreOutcomeHistory then applies
                    // broker-history PositionId ownership filtering.
                    if (!priorPath ||
                        !string.Equals(
                            fingerprint,
                            PriorMemoryConfigurationFingerprint(),
                            StringComparison.Ordinal))
                        return false;
                }

                byte[] bytes =
                    Convert.FromBase64String(encoded);

                payload =
                    Encoding.UTF8.GetString(bytes);

                bool validPayload =
                    string.Equals(
                        payload.StartsWith(
                            OutcomeMemorySchema,
                            StringComparison.Ordinal)
                            ? OutcomeMemorySchema
                            : payload.StartsWith(
                                PriorOutcomeMemorySchema,
                                StringComparison.Ordinal)
                                ? PriorOutcomeMemorySchema
                                : "",
                        priorSchema
                            ? PriorOutcomeMemorySchema
                            : OutcomeMemorySchema,
                        StringComparison.Ordinal);

                if (!validPayload)
                {
                    payload = "";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP portable memory snapshot restore failed: {0}",
                    ex.Message);

                payload = "";
                return false;
            }
        }

        private void EnsurePortableMemoryArtifacts()
        {
            try
            {
                if (!_bufferedArchivePersistence.TryEnsureDirectory(
                        OutcomeArchiveDirectory))
                {
                    Print(
                        "CFIP history storage unavailable: cannot create History directory.");
                    return;
                }

                EnsureHistoryLocationMarker();

                string marker =
                    HistoryLocationMarkerPath();

                string markerReadback;

                bool markerRoundTrip =
                    _bufferedArchivePersistence.TryReadAllText(
                        marker,
                        out markerReadback) &&
                    !string.IsNullOrWhiteSpace(markerReadback) &&
                    markerReadback.StartsWith(
                        "CFIP HISTORY STORAGE",
                        StringComparison.Ordinal);

                _bufferedArchivePersistence.MarkProbeResult(
                    markerRoundTrip,
                    markerRoundTrip
                        ? ""
                        : "History read/write round-trip not verified");

                if (!markerRoundTrip)
                {
                    Print(
                        "CFIP history persistence probe failed: History read/write round-trip not verified.");
                }
                else
                {
                    Print(
                        "CFIP history persistence probe PASS: relative History path is readable and writable.");
                }

                string path =
                    PortableMemorySnapshotPath();

                if (!_bufferedArchivePersistence.FileExists(path))
                {
                    _bufferedArchivePersistence.TryWriteAllText(
                        path,
                        PortableMemorySnapshotText(),
                        Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP portable memory artifact initialization failed: {0}",
                    ex.Message);
            }
        }

        private void PersistPortableMemorySnapshot()
        {
            try
            {
                _bufferedArchivePersistence.TryWriteAllText(
                    PortableMemorySnapshotPath(),
                    PortableMemorySnapshotText(),
                    Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP portable memory snapshot persist failed: {0}",
                    ex.Message);
            }
        }
    }
}
