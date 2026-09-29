using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void RenderPanelDecisionRows(
            ref int slot,
            int contentWidth)
        {
                                                if (_decision != null)
                                                {
                                                    int direction =
                                                        _decision.Direction;
                                    
                                                    string decisionState =
                                                        direction == 1
                                                            ? "BUY"
                                                            : direction == -1
                                                                ? "SELL"
                                                                : "NEUTRAL";
                                    
                                                    AddPanelRow(
                                                        ref slot,
                                                        "DECISION  •  " +
                                                        decisionState +
                                                        "  •  " +
                                                        (_decision.EntryAllowed
                                                            ? "READY"
                                                            : "WATCH / BLOCKED"),
                                                        PanelDirectionColor(
                                                            direction),
                                                        true,
                                                        contentWidth);
                                    
                                                    AddPanelRow(
                                                        ref slot,
                                                        "CONF " +
                                                        _decision.Confidence +
                                                        "  •  EDGE " +
                                                        _decision.Edge +
                                                        "  •  SMART " +
                                                        _decision.SmartQuality,
                                                        PanelDirectionColor(
                                                            direction),
                                                        true,
                                                        contentWidth);

                                                    RenderPanelCalibrationRows(
                                                        ref slot,
                                                        contentWidth);

                                   
                                                    AddPanelRow(
                                                        ref slot,
                                                        "MTF " +
                                                        _decision.TimeframeAgreement +
                                                        "  •  EVID " +
                                                        _decision.IndependentEvidence +
                                                        "  •  STRUCT " +
                                                        _decision.StructuralConfirmations,
                                                        PanelSecondaryTextColor,
                                                        false,
                                                        contentWidth);
                                    
                                                    AddPanelRow(
                                                        ref slot,
                                                        "TOP-DOWN  " +
                                                        (_decision.TopDownStage ?? "HTF SEARCH") +
                                                        "  •  HTF " +
                                                        _decision.HtfAnchorDirection +
                                                        "/" +
                                                        _decision.HtfAlignment +
                                                        "  •  MID " +
                                                        _decision.MidframeDirection +
                                                        "/" +
                                                        _decision.MidframeAlignment +
                                                        "  •  ENTRY " +
                                                        _decision.EntryFrameAlignment,
                                                        _decision.TopDownEligible &&
                                                        string.Equals(
                                                            _decision.TopDownStage,
                                                            "ENTRY CALIBRATED",
                                                            StringComparison.OrdinalIgnoreCase)
                                                            ? TpLineColor
                                                            : PanelWarningColor,
                                                        true,
                                                        contentWidth);

                                                    if (UseWaveTrendEvidence)
                                                    {
                                                        AddPanelRow(
                                                            ref slot,
                                                            "WAVETREND  " +
                                                            _m5Frame.WaveTrend.ToString("F1") +
                                                            " / " +
                                                            _m5Frame.WaveTrendSignal.ToString("F1") +
                                                            "  •  Q " +
                                                            _m5Frame.WaveTrendQuality +
                                                            "  •  " +
                                                            (_m5Frame.WaveTrendBullCross
                                                                ? "BULL CROSS"
                                                                : _m5Frame.WaveTrendBearCross
                                                                    ? "BEAR CROSS"
                                                                    : _m5Frame.WaveTrendBull
                                                                        ? "BULL"
                                                                        : _m5Frame.WaveTrendBear
                                                                            ? "BEAR"
                                                                            : "NEUTRAL"),
                                                            _m5Frame.WaveTrendDirection == direction
                                                                ? PanelDirectionColor(direction)
                                                                : PanelSecondaryTextColor,
                                                            false,
                                                            contentWidth);
                                                    }

                                                    if (EnableParallelOpportunities &&
                                                        _opportunityCandidates.Count > 0)
                                                    {
                                                        string laneSummary = "";

                                                        for (int oi = 0;
                                                             oi < _opportunityCandidates.Count;
                                                             oi++)
                                                        {
                                                            TradeOpportunityCandidate opportunity =
                                                                _opportunityCandidates[oi];

                                                            if (opportunity == null)
                                                                continue;

                                                            if (laneSummary.Length > 0)
                                                                laneSummary += "  •  ";

                                                            laneSummary +=
                                                                opportunity.LabelPrefix +
                                                                " " +
                                                                (opportunity.Direction == 1 ? "BUY" : "SELL") +
                                                                " " +
                                                                opportunity.Tp1RR.ToString("F2") +
                                                                "R/" +
                                                                opportunity.Quality;
                                                        }

                                                        AddPanelRow(
                                                            ref slot,
                                                            "LANES  " +
                                                            laneSummary,
                                                            PanelAccentColor,
                                                            false,
                                                            contentWidth);
                                                    }

                                                    MarketRegimeSnapshot regime =
                                                        _m5RegimeSnapshot;

                                                    string regimeMetrics =
                                                        regime == null
                                                            ? string.Empty
                                                            : "  •  ADX " +
                                                              Math.Round(regime.Adx, 1) +
                                                              "  •  CHOP " +
                                                              Math.Round(regime.Choppiness, 1) +
                                                              "  •  ATRx " +
                                                              Math.Round(regime.AtrRatio, 2) +
                                                              "  •  EFF " +
                                                              Math.Round(regime.RangeEfficiency, 2) +
                                                              "  •  RANGE " +
                                                              Math.Round(regime.RangeWidthAtr, 2) +
                                                              "ATR  •  STAB " +
                                                              regime.Stability;

                                                    AddPanelRow(
                                                        ref slot,
                                                        "REGIME " +
                                                        _decision.Regime +
                                                        "  •  Q" +
                                                        _decision.RegimeQuality +
                                                        "  •  RETEST " +
                                                        _decision.RetestQuality +
                                                        "  •  SHARE " +
                                                        _decision.BuyShare +
                                                        "/" +
                                                        _decision.SellShare +
                                                        regimeMetrics,
                                                        PanelSecondaryTextColor,
                                                        false,
                                                        contentWidth);
                                    
                                                    AddPanelRow(
                                                        ref slot,
                                                        "CONFLUENCE  " +
                                                        ConfluenceText(
                                                            _m5Frame),
                                                        PanelAccentColor,
                                                        false,
                                                        contentWidth);
                                    
                                                    
                                                    if (UseOssExtendedIndicatorConfluence &&
                                                        _m5Frame != null &&
                                                        _m5Frame.OssIndicatorCount > 0)
                                                    {
                                                        AddPanelRow(
                                                            ref slot,
                                                            "OSS INDICATORS  " +
                                                            _m5Frame.OssBullVotes +
                                                            "/" +
                                                            _m5Frame.OssBearVotes +
                                                            " B/S  •  " +
                                                            _m5Frame.OssIndicatorCount +
                                                            " CHECKS",
                                                            _m5Frame.OssBull
                                                                ? TpLineColor
                                                                : _m5Frame.OssBear
                                                                    ? SlLineColor
                                                                    : PanelSecondaryTextColor,
                                                            false,
                                                            contentWidth);
                                                    }

AddPanelRow(
                                                        ref slot,
                                                        _decision.TriggerReady
                                                            ? "TRIGGER  CONFIRMED"
                                                            : "TRIGGER  WAITING",
                                                        _decision.TriggerReady
                                                            ? TriggerLineColor
                                                            : PanelWarningColor,
                                                        true,
                                                        contentWidth);
                                    
                                                    if (!string.IsNullOrWhiteSpace(
                                                            _decision.BlockReason))
                                                    {
                                                        AddPanelRow(
                                                            ref slot,
                                                            "BLOCK  " +
                                                            _decision.BlockReason,
                                                            SlLineColor,
                                                            true,
                                                            contentWidth);
                                                    }
                                    
                                                    if (_prediction != null &&
                                                        _prediction.Direction != 0 &&
                                                        _prediction.Confidence >=
                                                        Math.Max(
                                                            MinimumEarlyConfidence,
                                                            EarlySetupConfidence))
                                                    {
                                                        AddPanelRow(
                                                            ref slot,
                                                            "EARLY ANALYSIS  •  " +
                                                            (_prediction.Direction == 1
                                                                ? "BUY"
                                                                : "SELL") +
                                                            "  •  CONF " +
                                                            _prediction.Confidence,
                                                            PanelDirectionColor(
                                                                _prediction.Direction),
                                                            true,
                                                            contentWidth);
                                    
                                                        AddPanelRow(
                                                            ref slot,
                                                            "PREDICTION  " +
                                                            ExecutionModeText(
                                                                _prediction.Mode) +
                                                            "  •  ENTRY " +
                                                            Price(_prediction.Entry) +
                                                            "  •  TRIGGER " +
                                                            Price(_prediction.Trigger),
                                                            PanelDirectionColor(
                                                                _prediction.Direction),
                                                            false,
                                                            contentWidth);
                                    
                                                        AddPanelRow(
                                                            ref slot,
                                                            "PRED TARGETS  " +
                                                            Price(_prediction.Target1) +
                                                            "  /  " +
                                                            Price(_prediction.Target2) +
                                                            "  /  " +
                                                            Price(_prediction.Target3) +
                                                            "  /  " +
                                                            Price(_prediction.Target4),
                                                            PanelSecondaryTextColor,
                                                            false,
                                                            contentWidth);
                                                    }
                                                }
                                    
            
        }
    }
}
