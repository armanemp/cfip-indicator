namespace cAlgo
{
    internal readonly struct PanelTimeframePresentationState
    {
        public PanelTimeframePresentationState(
            int direction,
            int strength,
            string label)
        {
            Direction = direction;
            Strength = strength;
            Label = label ?? "WAIT";
        }

        public int Direction { get; }

        public int Strength { get; }

        public string Label { get; }
    }
}
