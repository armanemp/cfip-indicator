namespace cAlgo
{
    internal readonly struct DivergenceCandidate
    {
        public int Quality { get; }
        public string Type { get; }
        public bool Regular { get; }
        public bool BearRegular { get; }
        public bool Hidden { get; }

        public DivergenceCandidate(
            int quality,
            string type,
            bool regular,
            bool bearRegular,
            bool hidden)
        {
            Quality =
                NumericGuards.ClampInt(
                    quality,
                    0,
                    100);
            Type =
                string.IsNullOrWhiteSpace(type)
                    ? "NONE"
                    : type;
            Regular =
                regular;
            BearRegular =
                bearRegular;
            Hidden =
                hidden;
        }
    }
}
