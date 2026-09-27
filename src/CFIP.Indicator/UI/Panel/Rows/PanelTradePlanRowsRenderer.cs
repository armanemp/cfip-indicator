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
        private void RenderPanelTradePlanRows(
            ref int slot,
            int contentWidth)
        {
                                                if (_plan != null &&
                                                    ShowTradePlanPanel)
                                                {
                                                    AddPanelRow(
                                                        ref slot,
                                                        "TRADE PLAN  •  " +
                                                        (_plan.Direction == 1
                                                            ? "BUY ACTIVE"
                                                            : "SELL ACTIVE"),
                                                        PanelDirectionColor(
                                                            _plan.Direction),
                                                        true,
                                                        contentWidth);
                                    
                                                    if (ShowLevelPricesInUnifiedPanel &&
                                                        ShowEntry)
                                                    {
                                                        AddPanelRow(
                                                            ref slot,
                                                            "ENTRY  " +
                                                            Price(_plan.Entry) +
                                                            "  •  IDEAL " +
                                                            Price(_plan.IdealEntry) +
                                                            "  •  Q" +
                                                            _plan.EntryQuality +
                                                            "  •  " +
                                                            _plan.EntrySource,
                                                            EntryLineColor,
                                                            true,
                                                            contentWidth);
                                                    }
                                    
                                                    if (ShowLevelPricesInUnifiedPanel &&
                                                        ShowSL)
                                                    {
                                                        AddPanelRow(
                                                            ref slot,
                                                            "STOP LOSS  " +
                                                            Price(_plan.Stop) +
                                                            "  •  " +
                                                            _plan.StopSource +
                                                            "  •  Q" +
                                                            _plan.StopQuality +
                                                            "  •  RISK " +
                                                            (_plan.Risk /
                                                             Math.Max(
                                                                 Symbol.PipSize,
                                                                 1e-9)).ToString("F1") +
                                                            "p",
                                                            SlLineColor,
                                                            true,
                                                            contentWidth);
                                                    }
                                    
                                                    if (ShowLevelPricesInUnifiedPanel &&
                                                        ShowTP1)
                                                    {
                                                        AddPanelRow(
                                                            ref slot,
                                                            "TAKE PROFIT 1  " +
                                                            Price(_plan.Tp1) +
                                                            "  •  RR " +
                                                            _plan.Tp1RR.ToString(
                                                                "F2") +
                                                            "  •  " +
                                                            _plan.Tp1Source +
                                                            "  •  Q" +
                                                            _plan.Tp1Quality +
                                                            (_tp1Hit != 0
                                                                ? "  •  HIT"
                                                                : ""),
                                                            TpLineColor,
                                                            true,
                                                            contentWidth);
                                                    }
                                    
                                                    if (ShowLevelPricesInUnifiedPanel &&
                                                        ShowTP2 &&
                                                        _plan.Tp2 > 0)
                                                    {
                                                        AddPanelRow(
                                                            ref slot,
                                                            "TAKE PROFIT 2  " +
                                                            Price(_plan.Tp2) +
                                                            "  •  RR " +
                                                            _plan.Tp2RR.ToString(
                                                                "F2") +
                                                            "  •  " +
                                                            _plan.Tp2Source +
                                                            "  •  Q" +
                                                            _plan.Tp2Quality +
                                                            (_tp2Hit != 0
                                                                ? "  •  HIT"
                                                                : ""),
                                                            Tp2LineColor,
                                                            true,
                                                            contentWidth);
                                                    }
                                    
                                                    if (ShowLevelPricesInUnifiedPanel &&
                                                        ShowTP3 &&
                                                        _plan.Tp3 > 0)
                                                    {
                                                        AddPanelRow(
                                                            ref slot,
                                                            "TAKE PROFIT 3  " +
                                                            Price(_plan.Tp3) +
                                                            "  •  RR " +
                                                            _plan.Tp3RR.ToString(
                                                                "F2") +
                                                            "  •  " +
                                                            _plan.Tp3Source +
                                                            "  •  Q" +
                                                            _plan.Tp3Quality +
                                                            (_tp3Hit != 0
                                                                ? "  •  HIT"
                                                                : ""),
                                                            Tp3LineColor,
                                                            true,
                                                            contentWidth);
                                                    }
                                    
                                                    if (ShowLevelPricesInUnifiedPanel &&
                                                        ShowTP4 &&
                                                        _plan.Tp4 > 0)
                                                    {
                                                        AddPanelRow(
                                                            ref slot,
                                                            "TAKE PROFIT 4  " +
                                                            Price(_plan.Tp4) +
                                                            "  •  RR " +
                                                            _plan.Tp4RR.ToString(
                                                                "F2") +
                                                            "  •  " +
                                                            _plan.Tp4Source +
                                                            "  •  Q" +
                                                            _plan.Tp4Quality +
                                                            (_tp4Hit != 0
                                                                ? "  •  HIT"
                                                                : ""),
                                                            Tp4LineColor,
                                                            true,
                                                            contentWidth);
                                                    }
                                    
                                                    AddPanelRow(
                                                        ref slot,
                                                        "REWARD MODEL  •  HTF TARGETS " +
                                                        _plan.HtfTargetCount +
                                                        "  •  MAX RR " +
                                                        Math.Max(
                                                            0,
                                                            MaximumRewardRR).ToString("F2"),
                                                        PanelAccentColor,
                                                        false,
                                                        contentWidth);
                                    
                                                    double liveRR =
                                                        _plan.Risk > 0
                                                            ? (_plan.Direction == 1
                                                                ? _lastMarket - _plan.Entry
                                                                : _plan.Entry - _lastMarket) /
                                                              _plan.Risk
                                                            : 0;
                                    
                                                    int exitPressure =
                                                        CalculateSmartExitPressure(
                                                            _lastMarket,
                                                            liveRR);
                                    
                                                    AddPanelRow(
                                                        ref slot,
                                                        "LIVE  RR " +
                                                        liveRR.ToString(
                                                            "F2") +
                                                        "  •  TP HIT " +
                                                        _tp1Hit +
                                                        "/" +
                                                        _tp2Hit +
                                                        "/" +
                                                        _tp3Hit +
                                                        "/" +
                                                        _tp4Hit,
                                                        liveRR >= 0
                                                            ? TpLineColor
                                                            : SlLineColor,
                                                        true,
                                                        contentWidth);
                                    
                                                     Position managedPosition =
                                                         GetManagedPosition();
                                    
                                                     if (managedPosition != null)
                                                     {
                                                         AddPanelRow(
                                                             ref slot,
                                                             "POSITION  •  " +
                                                             (managedPosition.TradeType ==
                                                                  TradeType.Buy
                                                                 ? "BUY"
                                                                 : "SELL") +
                                                             "  #" +
                                                             managedPosition.Id +
                                                             "  •  VOL " +
                                                             managedPosition.VolumeInUnits.ToString("F0") +
                                                             "  •  P/L " +
                                                             managedPosition.NetProfit.ToString("F2") +
                                                             "  •  SL " +
                                                             (managedPosition.StopLoss.HasValue
                                                                 ? Price(
                                                                     managedPosition.StopLoss.Value)
                                                                 : "-") +
                                                             "  •  TP " +
                                                             (managedPosition.TakeProfit.HasValue
                                                                 ? Price(
                                                                     managedPosition.TakeProfit.Value)
                                                                 : "-") +
                                                             "  •  " +
                                                             BrokerTargetStageText(
                                                                 managedPosition.TakeProfit.HasValue
                                                                     ? managedPosition.TakeProfit.Value
                                                                     : 0),
                                                             managedPosition.NetProfit >= 0
                                                                 ? TpLineColor
                                                                 : SlLineColor,
                                                             true,
                                                             contentWidth);
                                                     }
                                    
                                                    AddPanelRow(
                                                        ref slot,
                                                        "SMART EXIT  " +
                                                        GetSmartExitMode() +
                                                        "  •  PRESSURE " +
                                                        exitPressure,
                                                        exitPressure >=
                                                            SmartExitPressureThreshold
                                                            ? SlLineColor
                                                            : exitPressure >=
                                                              LiveReactionWatchThreshold
                                                                ? PanelWarningColor
                                                                : TpLineColor,
                                                        true,
                                                        contentWidth);
                                                }
                                    
            
        }
    }
}
