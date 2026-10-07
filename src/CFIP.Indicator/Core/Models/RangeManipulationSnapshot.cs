namespace cAlgo
{
    internal sealed class RangeManipulationSnapshot
    {
        public static RangeManipulationSnapshot Empty =>
            new RangeManipulationSnapshot
            {
                State = "NONE",
                Direction = 0,
                Score = 0,
                High = 0,
                Low = 0,
                WidthAtr = 0,
                StartIndex = -1,
                EndIndex = -1,
                SweepIndex = -1,
                BreakoutIndex = -1,
                RetestIndex = -1,
                Reason = "NO RANGE"
            };

        public string State { get; set; }
        public int Direction { get; set; }
        public int Score { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double WidthAtr { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public int SweepIndex { get; set; }
        public int BreakoutIndex { get; set; }
        public int RetestIndex { get; set; }
        public bool IsRange { get; set; }
        public bool IsManipulation { get; set; }
        public bool IsConfirmedBreakout { get; set; }
        public bool IsRetest { get; set; }
        public bool IsFailedBreakout { get; set; }
        public string Reason { get; set; }
    }
}