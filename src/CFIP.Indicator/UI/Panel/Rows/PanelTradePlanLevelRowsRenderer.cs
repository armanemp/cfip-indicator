using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void RenderPanelTradePlanLevelRows(
            ref int slot,
            int contentWidth)
        {
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
                    _plan.Tp1RR.ToString("F2") +
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
                    _plan.Tp2RR.ToString("F2") +
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
                    _plan.Tp3RR.ToString("F2") +
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
                    _plan.Tp4RR.ToString("F2") +
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
                    MaximumRewardRR).ToString("F2") +
                "  •  TP1 SOURCE " +
                (_plan.Tp1Source ?? "NONE") +
                "  •  TP1 Q " +
                _plan.Tp1Quality,
                PanelAccentColor,
                false,
                contentWidth);
        }
    }
}