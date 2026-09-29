using System;

namespace cAlgo
{
    internal readonly struct SubmissionAttemptIdentity : IEquatable<SubmissionAttemptIdentity>
    {
        public SubmissionAttemptIdentity(
            string signalKey,
            string attemptKey,
            ExecutionSubmissionPath path)
        {
            SignalKey = signalKey ?? string.Empty;
            AttemptKey = attemptKey ?? string.Empty;
            Path = path;
        }

        public string SignalKey { get; }
        public string AttemptKey { get; }
        public ExecutionSubmissionPath Path { get; }

        public string CanonicalKey
        {
            get
            {
                return SignalKey + "|" +
                       AttemptKey + "|" +
                       ((int)Path).ToString();
            }
        }

        public bool Equals(SubmissionAttemptIdentity other)
        {
            return string.Equals(SignalKey, other.SignalKey, StringComparison.Ordinal) &&
                   string.Equals(AttemptKey, other.AttemptKey, StringComparison.Ordinal) &&
                   Path == other.Path;
        }

        public override bool Equals(object obj)
        {
            return obj is SubmissionAttemptIdentity &&
                   Equals((SubmissionAttemptIdentity)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + StringComparer.Ordinal.GetHashCode(SignalKey);
                hash = hash * 31 + StringComparer.Ordinal.GetHashCode(AttemptKey);
                hash = hash * 31 + (int)Path;
                return hash;
            }
        }

        public static bool operator ==
            (SubmissionAttemptIdentity left, SubmissionAttemptIdentity right)
        {
            return left.Equals(right);
        }

        public static bool operator !=
            (SubmissionAttemptIdentity left, SubmissionAttemptIdentity right)
        {
            return !left.Equals(right);
        }
    }
}