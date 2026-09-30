using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace cAlgo
{
    internal sealed class BufferedArchivePersistence
    {
        private sealed class PendingLine
        {
            public string Line;
            public string Key;
        }

        private sealed class PendingFile
        {
            public string Path;
            public string Header;
            public readonly List<PendingLine> Lines =
                new List<PendingLine>();
            public readonly HashSet<string> PendingKeys =
                new HashSet<string>(StringComparer.Ordinal);
            public HashSet<string> ExistingKeys;
        }

        private readonly Dictionary<string, PendingFile> _files =
            new Dictionary<string, PendingFile>(
                StringComparer.Ordinal);

        public int PendingLineCount
        {
            get
            {
                int count = 0;

                foreach (PendingFile file in _files.Values)
                    count += file.Lines.Count;

                return count;
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

            PendingFile file;

            if (!_files.TryGetValue(
                    path,
                    out file))
            {
                file =
                    new PendingFile
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
                new PendingLine
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
                PendingFile file =
                    _files[paths[i]];

                if (file.Lines.Count == 0)
                {
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

                List<PendingLine> write =
                    new List<PendingLine>();

                int consumed = 0;

                for (int lineIndex = 0;
                     lineIndex < file.Lines.Count &&
                     remainingBudget > 0;
                     lineIndex++)
                {
                    PendingLine pending =
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
                    EnsureDirectory(
                        file.Path);

                    if (!File.Exists(
                            file.Path))
                    {
                        if (!string.IsNullOrEmpty(
                                file.Header))
                        {
                            File.WriteAllText(
                                file.Path,
                                file.Header.EndsWith(
                                    Environment.NewLine,
                                    StringComparison.Ordinal)
                                    ? file.Header
                                    : file.Header +
                                      Environment.NewLine,
                                Encoding.UTF8);
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

                        File.AppendAllText(
                            file.Path,
                            payload.ToString(),
                            Encoding.UTF8);

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
                catch
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

        public void Clear()
        {
            _files.Clear();
        }

        private HashSet<string> EnsureExistingKeys(
            PendingFile file)
        {
            if (file.ExistingKeys != null)
                return file.ExistingKeys;

            file.ExistingKeys =
                new HashSet<string>(
                    StringComparer.Ordinal);

            if (!File.Exists(file.Path))
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
            catch
            {
                file.ExistingKeys =
                    new HashSet<string>(
                        StringComparer.Ordinal);
            }

            return file.ExistingKeys;
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
