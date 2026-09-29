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
            if (Bars == null ||
                Bars.Count < 1)
            {
                RemoveAnalysisGuide();
                return;
            }

            try
            {
                SignalVisualSnapshot snapshot =
                    _initializationReady
                        ? BuildSignalVisualSnapshot(
                            Math.Max(
                                1,
                                _lastEvaluatedM5))
                        : null;

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

                string dataDetail =
                    "M5=" +
                    GuideBarsCount(_m5Bars) +
                    " M15=" +
                    GuideBarsCount(_m15Bars) +
                    " M30=" +
                    GuideBarsCount(_m30Bars) +
                    " H1=" +
                    GuideBarsCount(_h1Bars) +
                    " H4=" +
                    GuideBarsCount(_h4Bars);

                string guideText =
                    "CFIP ANALYSIS" +
                    "\nENGINE   " +
                    _status +
                    "\nDATA     " +
                    dataState +
                    " • PENDING " +
                    _initializationPendingDataLoads +
                    "\nDATASET  " +
                    dataDetail +
                    "\nCALC     " +
                    calculationState +
                    "\nDECISION " +
                    decisionState +
                    "\nREACTION " +
                    reactionState +
                    "\nVISUAL   " +
                    visualState +
                    "\nPLAN     " +
                    planState +
                    "\nMTF      " +
                    mtfState +
                    "\nCLOSED M5 " +
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
                        "\nBLOCK    " +
                        _decision.BlockReason;
                }

                if (_runtimeFaultStateMachine.State !=
                    RuntimeFaultState.Healthy)
                {
                    guideText +=
                        "\nRUNTIME  " +
                        CurrentRuntimeFaultState;
                }

                if (_initializationStartedUtc !=
                    DateTime.MinValue &&
                    !_initializationReady)
                {
                    TimeSpan elapsed =
                        TimeInUtc -
                        _initializationStartedUtc;

                    guideText +=
                        "\nINIT AGE " +
                        Math.Max(
                            0,
                            elapsed.TotalSeconds)
                            .ToString("F1") +
                        "s";
                }

                Color color =
                    _runtimeFaultStateMachine.State ==
                    RuntimeFaultState.Healthy
                        ? PanelAccentColor
                        : PanelWarningColor;

                ChartStaticText guide =
                    Chart.DrawStaticText(
                        AnalysisGuideObjectName,
                        guideText,
                        VerticalAlignment.Top,
                        HorizontalAlignment.Right,
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
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP analysis guide render failed: {0}",
                    ex.ToString());
            }
        }

        private string GuideBarsCount(
            Bars bars)
        {
            return bars == null
                ? "--"
                : bars.Count.ToString();
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
