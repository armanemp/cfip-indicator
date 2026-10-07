using System;
using System.Collections.Generic;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    /// <summary>
    /// Specialist opening-range engine for CFIP.
    ///
    /// Responsibilities:
    /// - Build a configurable UTC opening range.
    /// - Detect liquidity sweeps/manipulation outside the range and reclaim.
    /// - Estimate the likely breakout side before the actual breakout.
    /// - Confirm breakout only from a closed candle with displacement/close-location/volume evidence.
    /// - Detect first retest after a confirmed breakout.
    ///
    /// This indicator never places, modifies or closes orders.
    /// All decisions use closed bars by default, so historical signals are causal
    /// and do not depend on future candles.
    /// </summary>
#pragma warning disable CS0612
    [Indicator(
        "CFIP Opening Range Manipulation",
        IsOverlay = true,
        AutoRescale = false,
        TimeZone = TimeZones.UTC,
        AccessRights = AccessRights.None)]
    public class CFIPOpeningRangeManipulationIndicator : Indicator
#pragma warning restore CS0612
    {
        public enum RangeState
        {
            WaitingForSession,
            BuildingRange,
            RangeReady,
            BullishSetup,
            BearishSetup,
            BullishManipulation,
            BearishManipulation,
            BullishBreakout,
            BearishBreakout,
            BullishRetest,
            BearishRetest,
            FailedBullishBreakout,
            FailedBearishBreakout,
            Neutral
        }

        private sealed class SessionInfo
        {
            public DateTime Start;
            public DateTime End;
            public int StartIndex = -1;
            public int EndIndex = -1;
            public int RangeBars;
            public double High = double.MinValue;
            public double Low = double.MaxValue;
            public double AtrAtRangeEnd;
            public bool RangeReady;
            public int LastProcessedSignalBar = -1;
            public int LastBullManipulationBar = -1;
            public int LastBearManipulationBar = -1;
            public int BullBreakoutBar = -1;
            public int BearBreakoutBar = -1;
            public RangeState State = RangeState.BuildingRange;
            public string Reason = string.Empty;
        }

        [Parameter("Session Start UTC Hour", Group = "01 · Session", DefaultValue = 7, MinValue = 0, MaxValue = 23)]
        public int SessionStartUtcHour { get; set; }

        [Parameter("Session Start UTC Minute", Group = "01 · Session", DefaultValue = 0, MinValue = 0, MaxValue = 59)]
        public int SessionStartUtcMinute { get; set; }

        [Parameter("Opening Range Minutes", Group = "01 · Session", DefaultValue = 90, MinValue = 15, MaxValue = 240)]
        public int OpeningRangeMinutes { get; set; }

        [Parameter("Max Historical Sessions", Group = "01 · Session", DefaultValue = 5, MinValue = 1, MaxValue = 20)]
        public int MaxHistoricalSessions { get; set; }

        [Parameter("ATR Period", Group = "02 · Volatility", DefaultValue = 14, MinValue = 5, MaxValue = 50)]
        public int AtrPeriod { get; set; }

        [Parameter("Min Range Width ATR", Group = "02 · Volatility", DefaultValue = 0.60, MinValue = 0.10, MaxValue = 5.0, Step = 0.05)]
        public double MinRangeWidthAtr { get; set; }

        [Parameter("Max Range Width ATR", Group = "02 · Volatility", DefaultValue = 4.00, MinValue = 0.50, MaxValue = 10.0, Step = 0.10)]
        public double MaxRangeWidthAtr { get; set; }

        [Parameter("Breakout Buffer ATR", Group = "03 · Breakout", DefaultValue = 0.12, MinValue = 0.0, MaxValue = 0.50, Step = 0.01)]
        public double BreakoutBufferAtr { get; set; }

        [Parameter("Minimum Breakout Body ATR", Group = "03 · Breakout", DefaultValue = 0.45, MinValue = 0.10, MaxValue = 2.0, Step = 0.05)]
        public double MinimumBreakoutBodyAtr { get; set; }

        [Parameter("Minimum Close Location", Group = "03 · Breakout", DefaultValue = 0.68, MinValue = 0.50, MaxValue = 0.95, Step = 0.01)]
        public double MinimumCloseLocation { get; set; }

        [Parameter("Use Tick Volume", Group = "03 · Breakout", DefaultValue = true)]
        public bool UseTickVolume { get; set; }

        [Parameter("Volume Expansion", Group = "03 · Breakout", DefaultValue = 1.15, MinValue = 1.0, MaxValue = 3.0, Step = 0.05)]
        public double VolumeExpansion { get; set; }

        [Parameter("Use Range Expansion", Group = "03 · Breakout", DefaultValue = true)]
        public bool UseRangeExpansion { get; set; }

        [Parameter("Minimum Breakout Range ATR", Group = "03 · Breakout", DefaultValue = 0.75, MinValue = 0.20, MaxValue = 3.0, Step = 0.05)]
        public double MinimumBreakoutRangeAtr { get; set; }

        [Parameter("Manipulation Sweep ATR", Group = "04 · Manipulation", DefaultValue = 0.06, MinValue = 0.0, MaxValue = 1.0, Step = 0.01)]
        public double ManipulationSweepAtr { get; set; }

        [Parameter("Reclaim Buffer ATR", Group = "04 · Manipulation", DefaultValue = 0.02, MinValue = 0.0, MaxValue = 0.50, Step = 0.01)]
        public double ReclaimBufferAtr { get; set; }

        [Parameter("Manipulation Lookback Bars", Group = "04 · Manipulation", DefaultValue = 12, MinValue = 1, MaxValue = 50)]
        public int ManipulationLookbackBars { get; set; }

        [Parameter("Minimum Wick Fraction", Group = "04 · Manipulation", DefaultValue = 0.25, MinValue = 0.05, MaxValue = 0.80, Step = 0.05)]
        public double MinimumWickFraction { get; set; }

        [Parameter("Use Higher Timeframe Filter", Group = "05 · Direction", DefaultValue = true)]
        public bool UseHigherTimeframeFilter { get; set; }

        [Parameter("Higher Timeframe", Group = "05 · Direction", DefaultValue = "Minute15")]
        public TimeFrame HigherTimeFrame { get; set; }

        [Parameter("HTF Fast EMA", Group = "05 · Direction", DefaultValue = 20, MinValue = 5, MaxValue = 100)]
        public int HtfFastEma { get; set; }

        [Parameter("HTF Slow EMA", Group = "05 · Direction", DefaultValue = 50, MinValue = 10, MaxValue = 200)]
        public int HtfSlowEma { get; set; }

        [Parameter("Prediction Threshold", Group = "05 · Direction", DefaultValue = 70, MinValue = 55, MaxValue = 90)]
        public int PredictionThreshold { get; set; }

        [Parameter("Use Momentum Bias", Group = "05 · Direction", DefaultValue = true)]
        public bool UseMomentumBias { get; set; }

        [Parameter("Momentum Lookback Bars", Group = "05 · Direction", DefaultValue = 4, MinValue = 2, MaxValue = 20)]
        public int MomentumLookbackBars { get; set; }

        [Parameter("Retest Window Bars", Group = "06 · Retest", DefaultValue = 6, MinValue = 1, MaxValue = 20)]
        public int RetestWindowBars { get; set; }

        [Parameter("Retest Tolerance ATR", Group = "06 · Retest", DefaultValue = 0.15, MinValue = 0.02, MaxValue = 0.50, Step = 0.01)]
        public double RetestToleranceAtr { get; set; }

        [Parameter("Show Manipulation Arrows", Group = "07 · Display", DefaultValue = true)]
        public bool ShowManipulationArrows { get; set; }

        [Parameter("Show Breakout Arrows", Group = "07 · Display", DefaultValue = true)]
        public bool ShowBreakoutArrows { get; set; }

        [Parameter("Show Retest Arrows", Group = "07 · Display", DefaultValue = true)]
        public bool ShowRetestArrows { get; set; }

        [Parameter("Show Status Panel", Group = "07 · Display", DefaultValue = true)]
        public bool ShowStatusPanel { get; set; }

        [Parameter("Projection Bars", Group = "07 · Display", DefaultValue = 40, MinValue = 5, MaxValue = 200)]
        public int ProjectionBars { get; set; }

        private readonly Dictionary<DateTime, SessionInfo> _sessions =
            new Dictionary<DateTime, SessionInfo>();

        private Bars _higherTimeFrameBars;
        private int _lastSignalBar = -1;
        private RangeState _lastState = RangeState.WaitingForSession;

        /// <summary>Current state machine state for cBot/other consumers.</summary>
        public RangeState CurrentState { get; private set; }

        /// <summary>+1 BUY bias, -1 SELL bias, 0 neutral.</summary>
        public int SignalDirection { get; private set; }

        /// <summary>0..100 directional confidence.</summary>
        public int SignalScore { get; private set; }

        /// <summary>Current confirmed opening-range high.</summary>
        public double CurrentRangeHigh { get; private set; }

        /// <summary>Current confirmed opening-range low.</summary>
        public double CurrentRangeLow { get; private set; }

        /// <summary>Price above this value is the bullish breakout zone.</summary>
        public double BullishTriggerPrice { get; private set; }

        /// <summary>Price below this value is the bearish breakout zone.</summary>
        public double BearishTriggerPrice { get; private set; }

        /// <summary>Latest confirmed manipulation direction.</summary>
        public int ManipulationDirection { get; private set; }

        /// <summary>Last confirmed manipulation bar index.</summary>
        public int ManipulationBarIndex { get; private set; } = -1;

        /// <summary>Human-readable current state for a panel/cBot.</summary>
        public string SignalText { get; private set; } = "WAITING FOR SESSION";

        protected override void Initialize()
        {
            _higherTimeFrameBars =
                UseHigherTimeframeFilter
                    ? MarketData.GetBars(HigherTimeFrame)
                    : null;

            CurrentState = RangeState.WaitingForSession;

            if (ShowStatusPanel)
            {
                Chart.DrawStaticText(
                    "CFIP_OR_STATUS",
                    "CFIP OR • WAITING",
                    VerticalAlignment.Top,
                    HorizontalAlignment.Right,
                    Color.White);

                var text = Chart.FindObject("CFIP_OR_STATUS") as ChartStaticText;
                if (text != null)
                {
                    text.FontSize = 11;
                    text.IsBold = false;
                }
            }
        }

        public override void Calculate(int index)
        {
            if (index < Math.Max(AtrPeriod + 5, 20) ||
                index >= Bars.Count)
                return;

            // We deliberately work from the last fully closed chart bar.
            int closedIndex = index - 1;
            if (closedIndex < Math.Max(AtrPeriod + 5, 20))
                return;

            DateTime time = Bars.OpenTimes[closedIndex];
            DateTime sessionStart = GetSessionStart(time);

            SessionInfo session = GetOrCreateSession(sessionStart);
            UpdateSessionRange(session, closedIndex);

            DrawSessionRange(session, closedIndex);

            if (!session.RangeReady)
            {
                CurrentState =
                    time < session.Start
                        ? RangeState.WaitingForSession
                        : RangeState.BuildingRange;

                SignalDirection = 0;
                SignalScore = 0;
                ManipulationDirection = 0;
                ManipulationBarIndex = -1;
                SignalText = CurrentState == RangeState.WaitingForSession
                    ? "WAITING FOR SESSION"
                    : "BUILDING OPENING RANGE";

                UpdateStatus(session, closedIndex);
                return;
            }

            CurrentRangeHigh = session.High;
            CurrentRangeLow = session.Low;

            double atr = Atr(closedIndex);
            if (!IsFinitePositive(atr))
            {
                SignalDirection = 0;
                SignalScore = 0;
                CurrentState = RangeState.Neutral;
                SignalText = "RANGE READY • ATR UNAVAILABLE";
                UpdateStatus(session, closedIndex);
                return;
            }

            double rangeWidth = session.High - session.Low;
            double rangeWidthAtr =
                rangeWidth > 0
                    ? rangeWidth / atr
                    : 0;

            if (rangeWidthAtr < MinRangeWidthAtr ||
                rangeWidthAtr > MaxRangeWidthAtr)
            {
                SignalDirection = 0;
                SignalScore = 0;
                CurrentState = RangeState.Neutral;
                SignalText =
                    "RANGE REJECTED • WIDTH " +
                    Math.Round(rangeWidthAtr, 2) +
                    " ATR";

                UpdateStatus(session, closedIndex);
                return;
            }

            BullishTriggerPrice =
                session.High +
                atr * BreakoutBufferAtr;

            BearishTriggerPrice =
                session.Low -
                atr * BreakoutBufferAtr;

            UpdateManipulation(session, closedIndex, atr);

            int htfDirection =
                UseHigherTimeframeFilter
                    ? HigherTimeFrameDirection(time)
                    : 0;

            int momentumDirection =
                UseMomentumBias
                    ? MomentumDirection(closedIndex)
                    : 0;

            int predictionScore;
            int predictionDirection;
            string predictionReason;

            CalculatePrediction(
                session,
                closedIndex,
                htfDirection,
                momentumDirection,
                out predictionDirection,
                out predictionScore,
                out predictionReason);

            SignalDirection = predictionDirection;
            SignalScore = predictionScore;

            if (TryConfirmBreakout(
                    session,
                    closedIndex,
                    atr,
                    htfDirection,
                    out int breakoutDirection,
                    out int breakoutScore))
            {
                SignalDirection = breakoutDirection;
                SignalScore = breakoutScore;

                if (breakoutDirection == 1)
                {
                    session.BullBreakoutBar = closedIndex;
                    session.BearBreakoutBar = -1;
                    CurrentState = RangeState.BullishBreakout;
                    SignalText =
                        "BUY BREAKOUT • " +
                        breakoutScore +
                        "/100";
                }
                else
                {
                    session.BearBreakoutBar = closedIndex;
                    session.BullBreakoutBar = -1;
                    CurrentState = RangeState.BearishBreakout;
                    SignalText =
                        "SELL BREAKOUT • " +
                        breakoutScore +
                        "/100";
                }

                DrawBreakoutArrow(
                    closedIndex,
                    breakoutDirection,
                    atr,
                    breakoutScore);

                _lastSignalBar = closedIndex;
                session.Reason = predictionReason + " | BREAKOUT";
            }
            else if (TryConfirmRetest(
                         session,
                         closedIndex,
                         atr,
                         out int retestDirection))
            {
                SignalDirection = retestDirection;
                SignalScore = Math.Max(SignalScore, 78);

                if (retestDirection == 1)
                {
                    CurrentState = RangeState.BullishRetest;
                    SignalText = "BUY RETEST • 78+/100";
                }
                else
                {
                    CurrentState = RangeState.BearishRetest;
                    SignalText = "SELL RETEST • 78+/100";
                }

                if (ShowRetestArrows &&
                    _lastSignalBar != closedIndex)
                {
                    DrawRetestArrow(
                        closedIndex,
                        retestDirection,
                        atr);

                    _lastSignalBar = closedIndex;
                }
            }
            else if (session.BullBreakoutBar >= 0 &&
                     closedIndex > session.BullBreakoutBar &&
                     Bars.ClosePrices[closedIndex] <
                     session.High -
                     atr * RetestToleranceAtr)
            {
                CurrentState = RangeState.FailedBullishBreakout;
                SignalText = "FAILED BULL BREAKOUT";
                SignalDirection = -1;
                SignalScore = Math.Min(75, Math.Max(55, SignalScore));
            }
            else if (session.BearBreakoutBar >= 0 &&
                     closedIndex > session.BearBreakoutBar &&
                     Bars.ClosePrices[closedIndex] >
                     session.Low +
                     atr * RetestToleranceAtr)
            {
                CurrentState = RangeState.FailedBearishBreakout;
                SignalText = "FAILED BEAR BREAKOUT";
                SignalDirection = 1;
                SignalScore = Math.Min(75, Math.Max(55, SignalScore));
            }
            else if (session.LastBullManipulationBar >= 0 &&
                     closedIndex -
                     session.LastBullManipulationBar <=
                     ManipulationLookbackBars)
            {
                CurrentState = RangeState.BullishManipulation;
                SignalText =
                    "BUY SETUP • SWEEP/RECLAIM • " +
                    SignalScore +
                    "/100";
            }
            else if (session.LastBearManipulationBar >= 0 &&
                     closedIndex -
                     session.LastBearManipulationBar <=
                     ManipulationLookbackBars)
            {
                CurrentState = RangeState.BearishManipulation;
                SignalText =
                    "SELL SETUP • SWEEP/RECLAIM • " +
                    SignalScore +
                    "/100";
            }
            else if (predictionDirection == 1 &&
                     predictionScore >= PredictionThreshold)
            {
                CurrentState = RangeState.BullishSetup;
                SignalText =
                    "BUY SETUP • " +
                    predictionScore +
                    "/100 • WAIT BREAKOUT";
            }
            else if (predictionDirection == -1 &&
                     predictionScore >= PredictionThreshold)
            {
                CurrentState = RangeState.BearishSetup;
                SignalText =
                    "SELL SETUP • " +
                    predictionScore +
                    "/100 • WAIT BREAKOUT";
            }
            else
            {
                CurrentState = RangeState.RangeReady;

                SignalText =
                    "RANGE READY • " +
                    (predictionDirection == 1
                        ? "BUY BIAS "
                        : predictionDirection == -1
                            ? "SELL BIAS "
                            : "NEUTRAL ") +
                    predictionScore +
                    "/100";
            }

            if (CurrentState != _lastState)
            {
                _lastState = CurrentState;
                UpdateStatus(session, closedIndex);
            }
            else
            {
                UpdateStatus(session, closedIndex);
            }
        }

        protected override void OnDestroy()
        {
            foreach (DateTime key in new List<DateTime>(_sessions.Keys))
            {
                RemoveSessionObjects(key);
            }

            Chart.RemoveObject("CFIP_OR_STATUS");
            base.OnDestroy();
        }

        private SessionInfo GetOrCreateSession(DateTime sessionStart)
        {
            if (_sessions.TryGetValue(sessionStart, out SessionInfo existing))
                return existing;

            DateTime end =
                sessionStart.AddMinutes(
                    Math.Max(1, OpeningRangeMinutes));

            SessionInfo session =
                new SessionInfo
                {
                    Start = sessionStart,
                    End = end
                };

            _sessions[sessionStart] = session;
            TrimOldSessions();
            return session;
        }

        private void TrimOldSessions()
        {
            if (_sessions.Count <= Math.Max(1, MaxHistoricalSessions))
                return;

            DateTime oldest = DateTime.MaxValue;

            foreach (DateTime key in _sessions.Keys)
            {
                if (key < oldest)
                    oldest = key;
            }

            if (oldest != DateTime.MaxValue)
            {
                RemoveSessionObjects(oldest);
                _sessions.Remove(oldest);
            }
        }

        private void UpdateSessionRange(
            SessionInfo session,
            int closedIndex)
        {
            int first =
                FindFirstIndexAtOrAfter(
                    session.Start);

            int last =
                FindLastIndexBefore(
                    session.End);

            if (first < 0 ||
                last < first ||
                first >= Bars.Count)
            {
                session.RangeBars = 0;
                session.RangeReady = false;
                return;
            }

            last = Math.Min(last, closedIndex);

            session.StartIndex = first;
            session.EndIndex = last;

            if (closedIndex < first)
            {
                session.RangeBars = 0;
                session.RangeReady = false;
                return;
            }

            double high = double.MinValue;
            double low = double.MaxValue;
            int count = 0;

            for (int i = first; i <= last; i++)
            {
                high = Math.Max(high, Bars.HighPrices[i]);
                low = Math.Min(low, Bars.LowPrices[i]);
                count++;
            }

            session.High = high;
            session.Low = low;
            session.RangeBars = count;

            bool windowComplete =
                Bars.OpenTimes[closedIndex] >= session.End;

            session.RangeReady =
                windowComplete &&
                count >= 2 &&
                high > low;

            if (session.RangeReady)
            {
                session.AtrAtRangeEnd =
                    Atr(
                        Math.Min(
                            last,
                            Math.Max(
                                first,
                                FindIndexAtOrBefore(session.End))));

                session.State =
                    RangeState.RangeReady;
            }
        }

        private void DrawSessionRange(
            SessionInfo session,
            int closedIndex)
        {
            if (session.StartIndex < 0 ||
                session.EndIndex < 0 ||
                session.High == double.MinValue ||
                session.Low == double.MaxValue)
                return;

            int start = session.StartIndex;
            int end =
                Math.Max(
                    start,
                    Math.Min(
                        closedIndex + ProjectionBars,
                        Bars.Count - 1));

            string prefix =
                "CFIP_OR_" +
                session.Start.ToString(
                    "yyyyMMddHHmm",
                    System.Globalization.CultureInfo.InvariantCulture);

            var highLine =
                Chart.DrawTrendLine(
                    prefix + "_H",
                    start,
                    session.High,
                    end,
                    session.High,
                    Color.Orange,
                    1,
                    LineStyle.Solid);

            highLine.ExtendToInfinity = false;

            var lowLine =
                Chart.DrawTrendLine(
                    prefix + "_L",
                    start,
                    session.Low,
                    end,
                    session.Low,
                    Color.DodgerBlue,
                    1,
                    LineStyle.Solid);

            lowLine.ExtendToInfinity = false;

            DateTime labelTime =
                start > 0
                    ? Bars.OpenTimes[start - 1]
                    : Bars.OpenTimes[start];

            var highText =
                Chart.DrawText(
                    prefix + "_HT",
                    "ORH " + FormatPrice(session.High),
                    labelTime,
                    session.High,
                    Color.Orange);

            highText.HorizontalAlignment = HorizontalAlignment.Right;
            highText.VerticalAlignment = VerticalAlignment.Center;
            highText.FontSize = 9;
            highText.IsBold = false;

            var lowText =
                Chart.DrawText(
                    prefix + "_LT",
                    "ORL " + FormatPrice(session.Low),
                    labelTime,
                    session.Low,
                    Color.DodgerBlue);

            lowText.HorizontalAlignment = HorizontalAlignment.Right;
            lowText.VerticalAlignment = VerticalAlignment.Center;
            lowText.FontSize = 9;
            lowText.IsBold = false;
        }

        private void UpdateManipulation(
            SessionInfo session,
            int closedIndex,
            double atr)
        {
            if (closedIndex <= session.EndIndex)
                return;

            double minSweep =
                Math.Max(
                    Symbol.PipSize * 2,
                    atr * ManipulationSweepAtr);

            double reclaim =
                Math.Max(
                    Symbol.PipSize,
                    atr * ReclaimBufferAtr);

            double high = Bars.HighPrices[closedIndex];
            double low = Bars.LowPrices[closedIndex];
            double open = Bars.OpenPrices[closedIndex];
            double close = Bars.ClosePrices[closedIndex];

            double barRange = Math.Max(Symbol.PipSize, high - low);

            double lowerWick =
                Math.Max(
                    0,
                    Math.Min(open, close) - low);

            double upperWick =
                Math.Max(
                    0,
                    high - Math.Max(open, close));

            bool bullSweep =
                low <
                session.Low -
                minSweep &&
                close >=
                session.Low +
                reclaim &&
                close > open &&
                lowerWick / barRange >=
                MinimumWickFraction;

            bool bearSweep =
                high >
                session.High +
                minSweep &&
                close <=
                session.High -
                reclaim &&
                close < open &&
                upperWick / barRange >=
                MinimumWickFraction;

            // When one candle sweeps both sides, there is no directional edge.
            if (bullSweep && bearSweep)
                return;

            if (bullSweep)
            {
                session.LastBullManipulationBar = closedIndex;
                session.LastBearManipulationBar = -1;
                ManipulationDirection = 1;
                ManipulationBarIndex = closedIndex;

                if (ShowManipulationArrows &&
                    session.LastProcessedSignalBar != closedIndex)
                {
                    var icon =
                        Chart.DrawIcon(
                            "CFIP_OR_SWEEP_B_" + closedIndex,
                            ChartIconType.UpArrow,
                            closedIndex,
                            low -
                            atr * 0.12,
                            Color.Lime);

                    session.LastProcessedSignalBar = closedIndex;

                    var text =
                        Chart.DrawText(
                            "CFIP_OR_SWEEP_BT_" + closedIndex,
                            "SWEEP BUY",
                            closedIndex,
                            low -
                            atr * 0.22,
                            Color.Lime);

                    text.HorizontalAlignment = HorizontalAlignment.Center;
                    text.VerticalAlignment = VerticalAlignment.Top;
                    text.FontSize = 8;
                    text.IsBold = false;
                }
            }
            else if (bearSweep)
            {
                session.LastBearManipulationBar = closedIndex;
                session.LastBullManipulationBar = -1;
                ManipulationDirection = -1;
                ManipulationBarIndex = closedIndex;

                if (ShowManipulationArrows &&
                    session.LastProcessedSignalBar != closedIndex)
                {
                    var icon =
                        Chart.DrawIcon(
                            "CFIP_OR_SWEEP_S_" + closedIndex,
                            ChartIconType.DownArrow,
                            closedIndex,
                            high +
                            atr * 0.12,
                            Color.Red);

                    session.LastProcessedSignalBar = closedIndex;

                    var text =
                        Chart.DrawText(
                            "CFIP_OR_SWEEP_ST_" + closedIndex,
                            "SWEEP SELL",
                            closedIndex,
                            high +
                            atr * 0.22,
                            Color.Red);

                    text.HorizontalAlignment = HorizontalAlignment.Center;
                    text.VerticalAlignment = VerticalAlignment.Bottom;
                    text.FontSize = 8;
                    text.IsBold = false;
                }
            }

            if (session.LastBullManipulationBar >= 0 &&
                closedIndex -
                session.LastBullManipulationBar >
                ManipulationLookbackBars)
            {
                session.LastBullManipulationBar = -1;
            }

            if (session.LastBearManipulationBar >= 0 &&
                closedIndex -
                session.LastBearManipulationBar >
                ManipulationLookbackBars)
            {
                session.LastBearManipulationBar = -1;
            }
        }

        private void CalculatePrediction(
            SessionInfo session,
            int closedIndex,
            int htfDirection,
            int momentumDirection,
            out int direction,
            out int score,
            out string reason)
        {
            int bull = 0;
            int bear = 0;
            List<string> reasons = new List<string>();

            double width =
                session.High -
                session.Low;

            if (width <= 0)
            {
                direction = 0;
                score = 0;
                reason = "INVALID RANGE";
                return;
            }

            double close = Bars.ClosePrices[closedIndex];
            double position =
                Math.Max(
                    0,
                    Math.Min(
                        1,
                        (close - session.Low) / width));

            // A sweep/reclaim is the strongest pre-break manipulation clue.
            if (session.LastBullManipulationBar >= 0 &&
                closedIndex -
                session.LastBullManipulationBar <=
                ManipulationLookbackBars)
            {
                bull += 32;
                reasons.Add("LOW SWEEP RECLAIM");
            }

            if (session.LastBearManipulationBar >= 0 &&
                closedIndex -
                session.LastBearManipulationBar <=
                ManipulationLookbackBars)
            {
                bear += 32;
                reasons.Add("HIGH SWEEP RECLAIM");
            }

            // Location is useful only as a supporting clue, never as a trade trigger.
            if (position <= 0.30)
            {
                bull += 10;
                reasons.Add("LOW-RANGE LOCATION");
            }
            else if (position >= 0.70)
            {
                bear += 10;
                reasons.Add("HIGH-RANGE LOCATION");
            }

            if (htfDirection > 0)
            {
                bull += 15;
                reasons.Add("HTF BULL");
            }
            else if (htfDirection < 0)
            {
                bear += 15;
                reasons.Add("HTF BEAR");
            }

            if (momentumDirection > 0)
            {
                bull += 12;
                reasons.Add("MOMENTUM BULL");
            }
            else if (momentumDirection < 0)
            {
                bear += 12;
                reasons.Add("MOMENTUM BEAR");
            }

            // Candle rejection at an edge adds only a small, symmetric amount.
            int rejection =
                EdgeRejectionDirection(
                    closedIndex,
                    session.High,
                    session.Low,
                    width);

            if (rejection > 0)
            {
                bull += 8;
                reasons.Add("LOW REJECTION");
            }
            else if (rejection < 0)
            {
                bear += 8;
                reasons.Add("HIGH REJECTION");
            }

            // Conflict penalty: recent evidence on both sides means no clean prediction.
            if (bull >= 20 && bear >= 20)
            {
                bull -= 10;
                bear -= 10;
                reasons.Add("SIDE CONFLICT");
            }

            bull = Math.Max(0, Math.Min(100, bull));
            bear = Math.Max(0, Math.Min(100, bear));

            if (bull > bear)
            {
                direction = 1;
                score = bull;
            }
            else if (bear > bull)
            {
                direction = -1;
                score = bear;
            }
            else
            {
                direction = 0;
                score = 50;
            }

            // Without manipulation evidence we intentionally cap early prediction.
            // This keeps "likely breakout side" distinct from an actual trade trigger.
            bool hasManipulation =
                session.LastBullManipulationBar >= 0 ||
                session.LastBearManipulationBar >= 0;

            if (!hasManipulation)
                score = Math.Min(score, PredictionThreshold + 4);

            reason =
                string.Join(
                    " + ",
                    reasons.ToArray());
        }

        private bool TryConfirmBreakout(
            SessionInfo session,
            int closedIndex,
            double atr,
            int htfDirection,
            out int direction,
            out int score)
        {
            direction = 0;
            score = 0;

            if (closedIndex <= session.EndIndex)
                return false;

            double open = Bars.OpenPrices[closedIndex];
            double high = Bars.HighPrices[closedIndex];
            double low = Bars.LowPrices[closedIndex];
            double close = Bars.ClosePrices[closedIndex];

            double range = Math.Max(Symbol.PipSize, high - low);
            double body = Math.Abs(close - open);

            if (range <= 0)
                return false;

            double closeLocationBull =
                (close - low) / range;

            double closeLocationBear =
                (high - close) / range;

            bool bullish =
                close >
                session.High +
                atr * BreakoutBufferAtr &&
                close > open &&
                body >=
                atr * MinimumBreakoutBodyAtr &&
                closeLocationBull >=
                MinimumCloseLocation;

            bool bearish =
                close <
                session.Low -
                atr * BreakoutBufferAtr &&
                close < open &&
                body >=
                atr * MinimumBreakoutBodyAtr &&
                closeLocationBear >=
                MinimumCloseLocation;

            if (!bullish && !bearish)
                return false;

            if (UseRangeExpansion &&
                range < atr * MinimumBreakoutRangeAtr)
                return false;

            if (UseTickVolume &&
                !VolumeExpansionConfirmed(closedIndex))
                return false;

            int baseScore =
                bullish
                    ? 70
                    : 70;

            if (bullish)
            {
                baseScore +=
                    Math.Min(
                        10,
                        (int)Math.Round(
                            10 *
                            closeLocationBull));

                if (session.LastBullManipulationBar >= 0 &&
                    closedIndex -
                    session.LastBullManipulationBar <=
                    ManipulationLookbackBars)
                    baseScore += 10;

                if (htfDirection == 1)
                    baseScore += 8;
                else if (htfDirection == -1)
                    baseScore -= 10;

                direction = 1;
            }
            else
            {
                baseScore +=
                    Math.Min(
                        10,
                        (int)Math.Round(
                            10 *
                            closeLocationBear));

                if (session.LastBearManipulationBar >= 0 &&
                    closedIndex -
                    session.LastBearManipulationBar <=
                    ManipulationLookbackBars)
                    baseScore += 10;

                if (htfDirection == -1)
                    baseScore += 8;
                else if (htfDirection == 1)
                    baseScore -= 10;

                direction = -1;
            }

            score =
                Math.Max(
                    60,
                    Math.Min(
                        100,
                        baseScore));

            // One candle cannot be both a bullish and bearish breakout.
            return direction != 0;
        }

        private bool TryConfirmRetest(
            SessionInfo session,
            int closedIndex,
            double atr,
            out int direction)
        {
            direction = 0;

            if (closedIndex <= 0)
                return false;

            double tolerance =
                Math.Max(
                    Symbol.PipSize * 2,
                    atr * RetestToleranceAtr);

            if (session.BullBreakoutBar >= 0 &&
                closedIndex >
                session.BullBreakoutBar &&
                closedIndex -
                session.BullBreakoutBar <=
                RetestWindowBars)
            {
                double low = Bars.LowPrices[closedIndex];
                double close = Bars.ClosePrices[closedIndex];

                if (low <= session.High + tolerance &&
                    close > session.High &&
                    close > Bars.OpenPrices[closedIndex])
                {
                    direction = 1;
                    return true;
                }
            }

            if (session.BearBreakoutBar >= 0 &&
                closedIndex >
                session.BearBreakoutBar &&
                closedIndex -
                session.BearBreakoutBar <=
                RetestWindowBars)
            {
                double high = Bars.HighPrices[closedIndex];
                double close = Bars.ClosePrices[closedIndex];

                if (high >= session.Low - tolerance &&
                    close < session.Low &&
                    close < Bars.OpenPrices[closedIndex])
                {
                    direction = -1;
                    return true;
                }
            }

            return false;
        }

        private void DrawBreakoutArrow(
            int index,
            int direction,
            double atr,
            int score)
        {
            if (!ShowBreakoutArrows)
                return;

            string suffix =
                direction > 0
                    ? "B"
                    : "S";

            var icon =
                Chart.DrawIcon(
                    "CFIP_OR_BREAK_" + suffix + "_" + index,
                    direction > 0
                        ? ChartIconType.UpArrow
                        : ChartIconType.DownArrow,
                    index,
                    direction > 0
                        ? Bars.LowPrices[index] -
                          atr * 0.15
                        : Bars.HighPrices[index] +
                          atr * 0.15,
                    direction > 0
                        ? Color.Lime
                        : Color.Red);

            var text =
                Chart.DrawText(
                    "CFIP_OR_BREAK_T_" + suffix + "_" + index,
                    direction > 0
                        ? "BUY " + score
                        : "SELL " + score,
                    index,
                    direction > 0
                        ? Bars.LowPrices[index] -
                          atr * 0.27
                        : Bars.HighPrices[index] +
                          atr * 0.27,
                    direction > 0
                        ? Color.Lime
                        : Color.Red);

            text.HorizontalAlignment = HorizontalAlignment.Center;
            text.VerticalAlignment =
                direction > 0
                    ? VerticalAlignment.Top
                    : VerticalAlignment.Bottom;
            text.FontSize = 9;
            text.IsBold = false;
        }

        private void DrawRetestArrow(
            int index,
            int direction,
            double atr)
        {
            string suffix =
                direction > 0
                    ? "B"
                    : "S";

            Chart.DrawIcon(
                "CFIP_OR_RETEST_" +
                suffix +
                "_" +
                index,
                direction > 0
                    ? ChartIconType.UpArrow
                    : ChartIconType.DownArrow,
                index,
                direction > 0
                    ? Bars.LowPrices[index] -
                      atr * 0.10
                    : Bars.HighPrices[index] +
                      atr * 0.10,
                direction > 0
                    ? Color.Lime
                    : Color.Red);

            var text =
                Chart.DrawText(
                    "CFIP_OR_RETEST_T_" +
                    suffix +
                    "_" +
                    index,
                    direction > 0
                        ? "RETEST BUY"
                        : "RETEST SELL",
                    index,
                    direction > 0
                        ? Bars.LowPrices[index] -
                          atr * 0.20
                        : Bars.HighPrices[index] +
                          atr * 0.20,
                    direction > 0
                        ? Color.Lime
                        : Color.Red);

            text.HorizontalAlignment = HorizontalAlignment.Center;
            text.VerticalAlignment =
                direction > 0
                    ? VerticalAlignment.Top
                    : VerticalAlignment.Bottom;
            text.FontSize = 8;
            text.IsBold = false;
        }

        private void UpdateStatus(
            SessionInfo session,
            int closedIndex)
        {
            if (!ShowStatusPanel)
                return;

            var panel =
                Chart.FindObject("CFIP_OR_STATUS") as ChartStaticText;

            if (panel == null)
                return;

            string rangeText =
                session.High != double.MinValue &&
                session.Low != double.MaxValue &&
                session.High > session.Low
                    ? "OR " +
                      FormatPrice(session.Low) +
                      " - " +
                      FormatPrice(session.High)
                    : "OR --";

            string bias =
                SignalDirection > 0
                    ? "BUY"
                    : SignalDirection < 0
                        ? "SELL"
                        : "NEUTRAL";

            panel.Text =
                "CFIP OR • " +
                SignalText +
                "\n" +
                rangeText +
                "\n" +
                "TRIG B " +
                (BullishTriggerPrice > 0
                    ? FormatPrice(BullishTriggerPrice)
                    : "--") +
                " | S " +
                (BearishTriggerPrice > 0
                    ? FormatPrice(BearishTriggerPrice)
                    : "--") +
                "\n" +
                "BIAS " +
                bias +
                " • SCORE " +
                SignalScore;

            panel.Color =
                SignalDirection > 0
                    ? Color.Lime
                    : SignalDirection < 0
                        ? Color.Red
                        : Color.White;
        }

        private int HigherTimeFrameDirection(DateTime currentTime)
        {
            if (_higherTimeFrameBars == null ||
                _higherTimeFrameBars.Count < HtfSlowEma + 5)
                return 0;

            int index =
                _higherTimeFrameBars.OpenTimes.GetIndexByTime(
                    currentTime);

            if (index < 1)
                return 0;

            // Do not use an HTF candle whose next open has not happened yet.
            if (index + 1 <
                _higherTimeFrameBars.Count &&
                _higherTimeFrameBars.OpenTimes[index + 1] >
                currentTime)
            {
                index--;
            }

            if (index < HtfSlowEma + 2)
                return 0;

            double fast =
                Ema(
                    _higherTimeFrameBars,
                    index,
                    HtfFastEma);

            double slow =
                Ema(
                    _higherTimeFrameBars,
                    index,
                    HtfSlowEma);

            double previousFast =
                Ema(
                    _higherTimeFrameBars,
                    Math.Max(
                        HtfFastEma,
                        index - 2),
                    HtfFastEma);

            double spread = Math.Abs(fast - slow);

            if (fast > slow &&
                fast >= previousFast &&
                spread >=
                Symbol.PipSize * 1.0)
                return 1;

            if (fast < slow &&
                fast <= previousFast &&
                spread >=
                Symbol.PipSize * 1.0)
                return -1;

            return 0;
        }

        private int MomentumDirection(int closedIndex)
        {
            int lookback =
                Math.Max(
                    2,
                    MomentumLookbackBars);

            if (closedIndex < lookback)
                return 0;

            double move =
                Bars.ClosePrices[closedIndex] -
                Bars.ClosePrices[closedIndex - lookback];

            double atr = Atr(closedIndex);

            if (!IsFinitePositive(atr))
                return 0;

            if (move > atr * 0.10)
                return 1;

            if (move < -atr * 0.10)
                return -1;

            return 0;
        }

        private int EdgeRejectionDirection(
            int index,
            double high,
            double low,
            double width)
        {
            double open = Bars.OpenPrices[index];
            double close = Bars.ClosePrices[index];
            double barHigh = Bars.HighPrices[index];
            double barLow = Bars.LowPrices[index];

            double barRange =
                Math.Max(
                    Symbol.PipSize,
                    barHigh - barLow);

            double lowerWick =
                Math.Max(
                    0,
                    Math.Min(open, close) - barLow);

            double upperWick =
                Math.Max(
                    0,
                    barHigh - Math.Max(open, close));

            bool nearLow =
                barLow <=
                low +
                width * 0.20;

            bool nearHigh =
                barHigh >=
                high -
                width * 0.20;

            bool bull =
                nearLow &&
                close > open &&
                lowerWick / barRange >=
                MinimumWickFraction;

            bool bear =
                nearHigh &&
                close < open &&
                upperWick / barRange >=
                MinimumWickFraction;

            if (bull && bear)
                return 0;

            return bull
                ? 1
                : bear
                    ? -1
                    : 0;
        }

        private bool VolumeExpansionConfirmed(int index)
        {
            int sample =
                Math.Max(
                    5,
                    Math.Min(
                        20,
                        index));

            if (sample < 5)
                return false;

            double baseline = 0;
            int count = 0;

            for (int i = index - sample;
                 i < index;
                 i++)
            {
                double volume =
                    Bars.TickVolumes[i];

                if (volume <= 0)
                    continue;

                baseline += volume;
                count++;
            }

            if (count == 0)
                return false;

            baseline /= count;

            if (baseline <= 0)
                return false;

            return
                Bars.TickVolumes[index] >=
                baseline * VolumeExpansion;
        }

        private double Atr(int index)
        {
            if (index <= 0 ||
                index >= Bars.Count)
                return 0;

            int first =
                Math.Max(
                    1,
                    index - AtrPeriod + 1);

            int count = 0;
            double sum = 0;

            for (int i = first;
                 i <= index;
                 i++)
            {
                double previousClose =
                    Bars.ClosePrices[i - 1];

                double high =
                    Bars.HighPrices[i];

                double low =
                    Bars.LowPrices[i];

                double trueRange =
                    Math.Max(
                        high - low,
                        Math.Max(
                            Math.Abs(high - previousClose),
                            Math.Abs(low - previousClose)));

                if (!double.IsNaN(trueRange) &&
                    !double.IsInfinity(trueRange) &&
                    trueRange > 0)
                {
                    sum += trueRange;
                    count++;
                }
            }

            return count > 0
                ? sum / count
                : 0;
        }

        private static double Ema(
            Bars bars,
            int index,
            int period)
        {
            if (bars == null ||
                index < 0 ||
                index >= bars.Count ||
                period < 2)
                return 0;

            int seedStart =
                Math.Max(
                    0,
                    index -
                    period * 4);

            double ema =
                bars.ClosePrices[seedStart];

            double alpha =
                2.0 /
                (period + 1.0);

            for (int i = seedStart + 1;
                 i <= index;
                 i++)
            {
                ema =
                    alpha *
                    bars.ClosePrices[i] +
                    (1.0 - alpha) *
                    ema;
            }

            return ema;
        }

        private DateTime GetSessionStart(DateTime time)
        {
            DateTime candidate =
                new DateTime(
                    time.Year,
                    time.Month,
                    time.Day,
                    SessionStartUtcHour,
                    SessionStartUtcMinute,
                    0,
                    DateTimeKind.Utc);

            if (time >= candidate)
                return candidate;

            DateTime previous = candidate.AddDays(-1);
            DateTime previousRangeEnd =
                previous.AddMinutes(
                    Math.Max(1, OpeningRangeMinutes));

            // Before today's opening window, keep the previous session only
            // while its own opening window is still being built. Once that
            // window is over, expose today's session as WAITING instead of
            // carrying yesterday's range into a new trading day.
            return time < previousRangeEnd
                ? previous
                : candidate;
        }

        private int FindFirstIndexAtOrAfter(DateTime time)
        {
            int index =
                Bars.OpenTimes.GetIndexByTime(time);

            if (index < 0)
                return -1;

            if (Bars.OpenTimes[index] < time)
                index++;

            return index < Bars.Count
                ? index
                : -1;
        }

        private int FindLastIndexBefore(DateTime time)
        {
            int index =
                Bars.OpenTimes.GetIndexByTime(time);

            if (index < 0)
                return -1;

            if (Bars.OpenTimes[index] >= time)
                index--;

            return Math.Max(0, index);
        }

        private int FindIndexAtOrBefore(DateTime time)
        {
            int index =
                Bars.OpenTimes.GetIndexByTime(time);

            if (index < 0)
                return -1;

            if (Bars.OpenTimes[index] > time)
                index--;

            return Math.Max(0, index);
        }

        private string FormatPrice(double price)
        {
            int digits =
                Symbol.Digits;

            return price.ToString(
                "F" + digits,
                System.Globalization.CultureInfo.InvariantCulture);
        }

        private static bool IsFinitePositive(double value)
        {
            return value > 0 &&
                   !double.IsNaN(value) &&
                   !double.IsInfinity(value);
        }

        private void RemoveSessionObjects(DateTime sessionStart)
        {
            string prefix =
                "CFIP_OR_" +
                sessionStart.ToString(
                    "yyyyMMddHHmm",
                    System.Globalization.CultureInfo.InvariantCulture);

            Chart.RemoveObject(prefix + "_H");
            Chart.RemoveObject(prefix + "_L");
            Chart.RemoveObject(prefix + "_HT");
            Chart.RemoveObject(prefix + "_LT");
        }
    }
}
