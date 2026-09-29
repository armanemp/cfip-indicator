using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private const string AnalysisGuideObjectName =
            "CFIP_ANALYSIS_GUIDE";

        private void RenderAnalysisGuide()
        {
            if (!ShowOnChartAnalysisGuide ||
                Bars == null ||
                Bars.Count < 2)
            {
                RemoveAnalysisGuide();
                return;
            }

            try
            {
                int first =
                    Math.Max(
                        0,
                        Chart.FirstVisibleBarIndex);

                int last =
                    Math.Max(
                        first,
                        Math.Min(
                            Bars.Count - 1,
                            Chart.LastVisibleBarIndex));

                int anchor =
                    Math.Max(
                        0,
                        Math.Min(
                            Bars.Count - 1,
                            first));

                double visibleHigh =
                    Highest(
                        Bars,
                        first,
                        last);

                double atr =
                    Atr(
                        Bars,
                        Math.Max(
                            1,
                            Math.Min(
                                Bars.Count - 1,
                                last)));

                double referencePrice =
                    visibleHigh > 0
                        ? visibleHigh
                        : Bars.ClosePrices[anchor];

                double offset =
                    Math.Max(
                        Symbol.PipSize * 8,
                        atr > 0
                            ? atr * 0.45
                            : Symbol.PipSize * 8);

                SignalVisualSnapshot snapshot =
                    BuildSignalVisualSnapshot(
                        Math.Max(
                            1,
                            _lastEvaluatedM5));

                string dataState =
                    _initializationReady
                        ? "READY"
                        : _initializationDataReady
                            ? "FINALIZING"
                            : _initializationPendingDataLoads > 0
                                ? "LOADING"
                                : "WAITING";

                string decisionState;

                if (_decision == null)
                {
                    decisionState = "NONE";
                }
                else if (_decision.Direction == 1)
                {
                    decisionState =
                        _decision.EntryAllowed
                            ? "BUY READY"
                            : "BUY WATCH/BLOCKED";
                }
                else if (_decision.Direction == -1)
                {
                    decisionState =
                        _decision.EntryAllowed
                            ? "SELL READY"
                            : "SELL WATCH/BLOCKED";
                }
                else
                {
                    decisionState = "NEUTRAL";
                }

                string reactionState;

                if (!EnableLiveReaction)
                {
                    reactionState = "OFF";
                }
                else if (_reaction == null ||
                         _reaction.Direction == 0)
                {
                    reactionState = "NONE";
                }
                else
                {
                    reactionState =
                        (_reaction.Direction == 1
                            ? "BUY"
                            : "SELL") +
                        (_reaction.EntryAllowed
                            ? " READY"
                            : " WATCH") +
                        " • INTRABAR";
                }

                string visualState =
                    snapshot == null
                        ? "NONE"
                        : snapshot.Stage ?? "WAIT";

                string planState =
                    _plan == null
                        ? "NONE"
                        : _plan.IsLivePosition
                            ? "ACTIVE"
                            : "PLAN";

                string mtfState =
                    "M1 " +
                    GuideFrameDirection(_m1Frame) +
                    "  M5 " +
                    GuideFrameDirection(_m5Frame) +
                    "  M15 " +
                    GuideFrameDirection(_m15Frame) +
                    "  M30 " +
                    GuideFrameDirection(_m30Frame) +
                    "  H1 " +
                    GuideFrameDirection(_h1Frame) +
                    "  H4 " +
                    GuideFrameDirection(_h4Frame);

                string calculationState =
                    _lastCalculationCompletedUtc ==
                    DateTime.MinValue
                        ? "NO CYCLE"
                        : CalculationAgeText();

                string guideText =
                    "CFIP ANALYSIS" +
                    "
ENGINE   " +
                    _status +
                    "
DATA     " +
                    dataState +
                    "
CALC     " +
                    calculationState +
                    "
DECISION " +
                    decisionState +
                    "
REACTION " +
                    reactionState +
                    "
VISUAL   " +
                    visualState +
                    "
PLAN     " +
                    planState +
                    "
MTF      " +
                    mtfState +
                    "
CLOSED M5 " +
                    _lastEvaluatedM5 +
                    " • " +
                    TimeInUtc.ToString(
                        "HH:mm:ss") +
                    " UTC";

                if (_decision != null &&
                    !string.IsNullOrWhiteSpace(
                        _decision.BlockReason))
                {
                    guideText +=
                        "
BLOCK    " +
                        _decision.BlockReason;
                }

                if (_runtimeFaultStateMachine.State !=
                    RuntimeFaultState.Healthy)
                {
                    guideText +=
                        "
RUNTIME  " +
                        CurrentRuntimeFaultState;
                }

                Color color =
                    _runtimeFaultStateMachine.State ==
                    RuntimeFaultState.Healthy
                        ? PanelAccentColor
                        : PanelWarningColor;

                ChartText guide =
                    Chart.DrawText(
                        AnalysisGuideObjectName,
                        guideText,
                        Bars.OpenTimes[anchor],
                        NormalizePrice(
                            referencePrice +
                            offset),
                        color);

                guide.FontSize =
                    Math.Max(
                        9,
                        Math.Min(
                            14,
                            PanelFontSize));

                guide.FontFamily =
                    string.IsNullOrWhiteSpace(
                        PanelFontFamily)
                        ? "Arial"
                        : PanelFontFamily;

                guide.IsBold = true;
                guide.HorizontalAlignment =
                    HorizontalAlignment.Left;
                guide.VerticalAlignment =
                    VerticalAlignment.Top;
                guide.IsInteractive = false;
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP analysis guide render failed: {0}",
                    ex.ToString());
            }
        }

        private string GuideFrameDirection(
            Frame frame)
        {
            if (frame == null)
                return "--";

            string direction =
                frame.Direction == 1
                    ? "B"
                    : frame.Direction == -1
                        ? "S"
                        : "N";

            return
                direction +
                frame.Quality.ToString(
                    "00");
        }

        private void RemoveAnalysisGuide()
        {
            try
            {
                Chart.RemoveObject(
                    AnalysisGuideObjectName);
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP analysis guide cleanup failed: {0}",
                    ex.ToString());
            }
        }
    }
}
