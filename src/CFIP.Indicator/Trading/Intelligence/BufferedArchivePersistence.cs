using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace cAlgo
{
    internal sealed class BufferedArchivePersistence
    {
        private readonly Dictionary<string, BufferedArchivePendingFile> _files =
            new Dictionary<string, BufferedArchivePendingFile>(
                StringComparer.Ordinal);

        private int _writeFailureCount;
        private int _readFailureCount;
        private string _lastError = "";
        private DateTime _lastSuccessUtc = DateTime.MinValue;
        private DateTime _lastProbeUtc = DateTime.MinValue;
        private bool _probeSucceeded;

        public int WriteFailureCount
        {
            get { return _writeFailureCount; }
        }

        public int ReadFailureCount
        {
            get { return _readFailureCount; }
        }

        public string LastError
        {
            get { return _lastError; }
        }

        public DateTime LastSuccessUtc
        {
            get { return _lastSuccessUtc; }
        }

        public DateTime LastProbeUtc
        {
            get { return _lastProbeUtc; }
        }

        public bool ProbeSucceeded
        {
            get { return _probeSucceeded; }
        }

        public void MarkProbeResult(
            bool succeeded,
            string error = "",
            DateTime? observedUtc = null)
        {
            _probeSucceeded = succeeded;
            _lastProbeUtc =
                observedUtc.HasValue
                    ? CanonicalTimeRule.EnsureUtc(
                        observedUtc.Value)
                    : DateTime.UtcNow;

            if (!succeeded &&
                !string.IsNullOrWhiteSpace(error))
                _lastError = error;
        }

        public int PendingLineCount
        {
            get
            {
                int count = 0;

                foreach (BufferedArchivePendingFile file in _files.Values)
                    count += file.Lines.Count;

                return count;
            }
        }

        public bool FileExists(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            try
            {
                return File.Exists(path);
            }
            catch (Exception ex)
            {
                RegisterReadFailure(ex);
                return false;
            }
        }

        public bool TryEnsureDirectory(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            try
            {
                Directory.CreateDirectory(path);
                RegisterSuccess();
                return true;
            }
            catch (Exception ex)
            {
                RegisterWriteFailure(ex);
                return false;
            }
        }

        public bool TryWriteAllText(
            string path,
            string content,
            Encoding encoding = null)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            try
            {
                EnsureDirectory(path);

                File.WriteAllText(
                    path,
                    content ?? "",
                    encoding ?? Encoding.UTF8);

                RegisterSuccess();
                return true;
            }
            catch (Exception ex)
            {
                RegisterWriteFailure(ex);
                return false;
            }
        }

        public bool TryAppendAllText(
            string path,
            string content,
            Encoding encoding = null)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            try
            {
                EnsureDirectory(path);

                File.AppendAllText(
                    path,
                    content ?? "",
                    encoding ?? Encoding.UTF8);

                RegisterSuccess();
                return true;
            }
            catch (Exception ex)
            {
                RegisterWriteFailure(ex);
                return false;
            }
        }

        public bool TryReadAllText(
            string path,
            out string content)
        {
            content = "";

            if (string.IsNullOrWhiteSpace(path))
                return false;

            try
            {
                if (!File.Exists(path))
                    return false;

                content = File.ReadAllText(path);
                RegisterSuccess();
                return true;
            }
            catch (Exception ex)
            {
                RegisterReadFailure(ex);
                return false;
            }
        }

        public bool TryReadAllLines(
            string path,
            out string[] lines)
        {
            lines = null;

            if (string.IsNullOrWhiteSpace(path))
                return false;

            try
            {
                if (!File.Exists(path))
                    return false;

                lines = File.ReadAllLines(path);
                RegisterSuccess();
                return true;
            }
            catch (Exception ex)
            {
                RegisterReadFailure(ex);
                return false;
            }
        }

        public bool Enqueue(
            string path,
            string header,
            string line,
            string key = null)
        {
            if (string.IsNullOrWhiteSpace(path) ||
                line == null)
                return false;

            BufferedArchivePendingFile file;

            if (!_files.TryGetValue(
                    path,
                    out file))
            {
                file =
                    new BufferedArchivePendingFile
                    {
                        Path = path,
                        Header = header ?? ""
                    };

                _files[path] = file;
            }
            else if (string.IsNullOrWhiteSpace(file.Header) &&
                     !string.IsNullOrWhiteSpace(header))
            {
                file.Header = header;
            }

            if (!string.IsNullOrWhiteSpace(key))
            {
                if (file.ExistingKeys != null &&
                    file.ExistingKeys.Contains(key))
                    return false;

                if (!file.PendingKeys.Add(key))
                    return false;
            }

            file.Lines.Add(
                new BufferedArchivePendingLine
                {
                    Line = line,
                    Key = string.IsNullOrWhiteSpace(key)
                        ? null
                        : key
                });

            return true;
        }

        public int Flush(
            int maximumLines = 512)
        {
            int remainingBudget =
                Math.Max(
                    1,
                    maximumLines);

            int flushed = 0;

            string[] paths =
                _files.Keys
                    .OrderBy(
                        x => x,
                        StringComparer.Ordinal)
                    .ToArray();

            for (int i = 0;
                 i < paths.Length &&
                 remainingBudget > 0;
                 i++)
            {
                BufferedArchivePendingFile file =
                    _files[paths[i]];

                if (file.Lines.Count == 0)
                {
                    if (file.ExistingKeys == null)
                        _files.Remove(file.Path);

                    continue;
                }

                bool hasKeyedLines =
                    false;

                for (int lineIndex = 0;
                     lineIndex < file.Lines.Count;
                     lineIndex++)
                {
                    if (!string.IsNullOrWhiteSpace(
                            file.Lines[lineIndex].Key))
                    {
                        hasKeyedLines = true;
                        break;
                    }
                }

                HashSet<string> existing =
                    hasKeyedLines
                        ? EnsureExistingKeys(file)
                        : null;

                List<BufferedArchivePendingLine> write =
                    new List<BufferedArchivePendingLine>();

                int consumed = 0;

                for (int lineIndex = 0;
                     lineIndex < file.Lines.Count &&
                     remainingBudget > 0;
                     lineIndex++)
                {
                    BufferedArchivePendingLine pending =
                        file.Lines[lineIndex];

                    if (!string.IsNullOrWhiteSpace(
                            pending.Key) &&
                        existing.Contains(
                            pending.Key))
                    {
                        consumed++;
                        file.PendingKeys.Remove(
                            pending.Key);
                        remainingBudget--;
                        continue;
                    }

                    write.Add(pending);
                    consumed++;
                    remainingBudget--;

                    if (!string.IsNullOrWhiteSpace(
                            pending.Key))
                    {
                        existing.Add(
                            pending.Key);
                    }
                }

                try
                {
                    if (!FileExists(
                            file.Path))
                    {
                        if (!string.IsNullOrEmpty(
                                file.Header))
                        {
                            if (!TryWriteAllText(
                                    file.Path,
                                    file.Header.EndsWith(
                                        Environment.NewLine,
                                        StringComparison.Ordinal)
                                        ? file.Header
                                        : file.Header +
                                          Environment.NewLine,
                                    Encoding.UTF8))
                            {
                                file.ExistingKeys = null;
                                continue;
                            }
                        }
                    }

                    if (write.Count > 0)
                    {
                        StringBuilder payload =
                            new StringBuilder();

                        for (int writeIndex = 0;
                             writeIndex < write.Count;
                             writeIndex++)
                        {
                            payload.Append(
                                write[writeIndex].Line);

                            if (!write[writeIndex].Line.EndsWith(
                                    Environment.NewLine,
                                    StringComparison.Ordinal))
                            {
                                payload.Append(
                                    Environment.NewLine);
                            }
                        }

                        if (!TryAppendAllText(
                                file.Path,
                                payload.ToString(),
                                Encoding.UTF8))
                        {
                            file.ExistingKeys = null;
                            continue;
                        }

                        flushed += write.Count;
                    }

                    if (consumed > 0)
                    {
                        file.Lines.RemoveRange(
                            0,
                            consumed);
                    }

                    if (file.Lines.Count == 0 &&
                        file.ExistingKeys == null)
                    {
                        // Keep keyed archive state resident so subsequent
                        // enqueues in this runtime remain idempotent without
                        // another disk read.
                        _files.Remove(
                            file.Path);
                    }
                }
                catch (Exception)
                {
                    // Preserve all lines when disk I/O fails. Requeue by not
                    // removing anything; the next Timer heartbeat can retry.
                    // ExistingKeys is also discarded so a recovered file is
                    // reread from disk before the next append.
                    file.ExistingKeys = null;
                }
            }

            return flushed;
        }

        private HashSet<string> EnsureExistingKeys(
            BufferedArchivePendingFile file)
        {
            if (file.ExistingKeys != null)
                return file.ExistingKeys;

            file.ExistingKeys =
                new HashSet<string>(
                    StringComparer.Ordinal);

            if (!FileExists(file.Path))
                return file.ExistingKeys;

            try
            {
                foreach (string line
                         in File.ReadLines(file.Path))
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    string key =
                        ExtractFirstField(
                            line);

                    if (!string.IsNullOrWhiteSpace(key) &&
                        !key.Equals(
                            "CFIP-OUTCOME-ARCHIVE",
                            StringComparison.Ordinal) &&
                        !key.Equals(
                            "CFIP-SIGNAL-TRACE",
                            StringComparison.Ordinal) &&
                        !key.Equals(
                            "CFIP-RUNTIME-LOG",
                            StringComparison.Ordinal))
                    {
                        file.ExistingKeys.Add(
                            key);
                    }
                }
            }
            catch (Exception ex)
            {
                RegisterReadFailure(ex);
                file.ExistingKeys =
                    new HashSet<string>(
                        StringComparer.Ordinal);
            }

            return file.ExistingKeys;
        }

        private void RegisterWriteFailure(
            Exception exception)
        {
            _writeFailureCount++;
            _lastError =
                exception == null
                    ? "WRITE FAILURE"
                    : exception.Message ?? "WRITE FAILURE";
        }

        private void RegisterReadFailure(
            Exception exception)
        {
            _readFailureCount++;
            _lastError =
                exception == null
                    ? "READ FAILURE"
                    : exception.Message ?? "READ FAILURE";
        }

        private void RegisterSuccess()
        {
            _lastSuccessUtc = DateTime.UtcNow;
            _lastError = "";
        }

        private static string ExtractFirstField(
            string line)
        {
            int comma =
                line.IndexOf(',');

            return
                (comma >= 0
                    ? line.Substring(
                        0,
                        comma)
                    : line).Trim();
        }

        private static void EnsureDirectory(
            string path)
        {
            string directory =
                Path.GetDirectoryName(
                    path);

            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(
                    directory);
        }
    }
}
