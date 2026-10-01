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
                                                        "ENTRY GATE  " +
                                                        (_decision.ActionableNow
                                                            ? "ACTIONABLE"
                                                            : "BLOCKED") +
                                                        "  •  " +
                                                        (_decision.ActionabilityReason ??
                                                         "NOT EVALUATED") +
                                                        "  •  LOC " +
                                                        _decision.EntryLocationQuality +
                                                        "  •  TIMING " +
                                                        _decision.EntryTimingQuality +
                                                        "  •  RR " +
                                                        _decision.ActionableTp1RR.ToString("F2"),
                                                        _decision.ActionableNow
                                                            ? TpLineColor
                                                            : PanelWarningColor,
                                                        true,
                                                        contentWidth);

                                                    AddPanelRow(
                                                        ref slot,
                                                        "DIVERGENCE  " +
                                                        (_decision.DivergenceType ?? "NONE") +
                                                        "  •  Q" +
                                                        _decision.DivergenceQuality +
                                                        "  •  DIR " +
                                                        (_decision.DivergenceDirection == 1
                                                            ? "BUY"
                                                            : _decision.DivergenceDirection == -1
                                                                ? "SELL"
                                                                : "NONE"),
                                                        _decision.DivergenceDirection == -direction &&
                                                        _decision.DivergenceQuality >= 70
                                                            ? SlLineColor
                                                            : _decision.DivergenceDirection == direction &&
                                                              _decision.DivergenceQuality >= 70
                                                                ? TpLineColor
                                                                : PanelSecondaryTextColor,
                                                        false,
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
                                                        "/S" +
                                                        _decision.HtfAbsoluteStrength +
                                                        "  •  MID " +
                                                        _decision.MidframeDirection +
                                                        "/" +
                                                        _decision.MidframeAlignment +
                                                        "/S" +
                                                        _decision.MidframeAbsoluteStrength +
                                                        "  •  ENTRY " +
                                                        _decision.EntryFrameAlignment +
                                                        "/S" +
                                                        _decision.EntryFrameAbsoluteStrength,
                                                        _decision.TopDownEligible &&
                                                        string.Equals(
                                                            _decision.TopDownStage,
                                                            "ENTRY CALIBRATED",
                                                            StringComparison.OrdinalIgnoreCase)
                                                            ? TpLineColor
                                                            : PanelWarningColor,
                                                        true,
                                                        contentWidth);

                                                    RenderPanelWaveTrendAndOpportunityRows(
                                                        ref slot,
                                                        contentWidth,
                                                        direction);

                                                    MarketStateFrameSnapshot regime =
                                                        _marketStateSnapshot == null
                                                            ? null
                                                            : _marketStateSnapshot.M5;

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
                                                              regime.RegimeStability;

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
                                                            "EARLY WATCH  •  " +
                                                            (_prediction.Direction == 1
                                                                ? "BUY"
                                                                : "SELL") +
                                                            "  •  SHARE " +
                                                            _prediction.DirectionalShare +
                                                            "  •  STRENGTH " +
                                                            _prediction.AbsoluteStrength.ToString("F1", CultureInfo.InvariantCulture),
                                                            PanelSecondaryTextColor,
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
