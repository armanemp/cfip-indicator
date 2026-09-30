using System.Collections.Generic;

namespace cAlgo
{
    internal sealed class BufferedArchivePendingLine
    {
        public string Line;
        public string Key;
    }

    internal sealed class BufferedArchivePendingFile
    {
        public string Path;
        public string Header;
        public readonly List<BufferedArchivePendingLine> Lines =
            new List<BufferedArchivePendingLine>();
        public readonly HashSet<string> PendingKeys =
            new HashSet<string>(System.StringComparer.Ordinal);
        public HashSet<string> ExistingKeys;
    }
}
