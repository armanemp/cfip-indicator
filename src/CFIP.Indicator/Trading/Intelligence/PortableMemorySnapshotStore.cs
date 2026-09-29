using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace cAlgo
{
    public partial class CFIPIndicator
    {
        private const string PortableMemorySnapshotSchema =
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
                Directory.CreateDirectory(
                    OutcomeArchiveDirectory);

                string marker =
                    HistoryLocationMarkerPath();

                if (!File.Exists(marker))
                {
                    File.WriteAllText(
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
                    Bars == null
                        ? "UNKNOWN"
                        : Bars.TimeFrame.ToString());

            return
                OutcomeArchiveDirectory +
                Path.DirectorySeparatorChar +
                "CFIP_PortableMemory_" +
                symbol +
                "_" +
                timeframe +
                "_" +
                MemoryConfigurationFingerprint() +
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
                Bars == null
                    ? "UNKNOWN"
                    : Bars.TimeFrame.ToString());
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

                if (!File.Exists(path))
                    return false;

                string[] lines =
                    File.ReadAllLines(path);

                if (lines.Length == 0 ||
                    !string.Equals(
                        lines[0].Trim(),
                        PortableMemorySnapshotSchema,
                        StringComparison.Ordinal))
                    return false;

                string fingerprint = "";
                string encoded = "";

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
                }

                if (!string.Equals(
                        fingerprint,
                        MemoryConfigurationFingerprint(),
                        StringComparison.Ordinal) ||
                    string.IsNullOrWhiteSpace(encoded))
                    return false;

                byte[] bytes =
                    Convert.FromBase64String(encoded);

                payload =
                    Encoding.UTF8.GetString(bytes);

                return
                    !string.IsNullOrWhiteSpace(payload) &&
                    payload.StartsWith(
                        OutcomeMemorySchema,
                        StringComparison.Ordinal);
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
                Directory.CreateDirectory(
                    OutcomeArchiveDirectory);

                EnsureHistoryLocationMarker();

                Print(
                    "CFIP history storage ready: Documents/cAlgo/Data/Indicators/{0}/History",
                    IndicatorStorageFolderName);

                string path =
                    PortableMemorySnapshotPath();

                if (!File.Exists(path))
                {
                    File.WriteAllText(
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
                Directory.CreateDirectory(
                    OutcomeArchiveDirectory);

                File.WriteAllText(
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
