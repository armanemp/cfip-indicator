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
        private void RenderPanelExecutionRows(
            ref int slot,
            int contentWidth)
        {
                                                AddPanelRow(
                                                    ref slot,
                                                    "READINESS  " +
                                                    PredictionReadinessText(),
                                                    PredictionReadinessColor(),
                                                    true,
                                                    contentWidth);
                                    
                                                if (_executionModel != null &&
                                                    _executionModel.Direction != 0)
                                                {
                                                    AddPanelRow(
                                                        ref slot,
                                                        "ENTRY MODEL  •  " +
                                                        ExecutionModeText(
                                                            _executionModel.Mode) +
                                                        "  •  " +
                                                        _executionModel.Source +
                                                        "  •  Q" +
                                                        _executionModel.Quality,
                                                        PanelDirectionColor(
                                                            _executionModel.Direction),
                                                        true,
                                                        contentWidth);
                                    
                                                    AddPanelRow(
                                                        ref slot,
                                                        "IDEAL ENTRY  " +
                                                        Price(
                                                            _executionModel.IdealEntry) +
                                                        "  •  ZONE " +
                                                        Price(
                                                            _executionModel.ZoneLow) +
                                                        " → " +
                                                        Price(
                                                            _executionModel.ZoneHigh),
                                                        PanelSecondaryTextColor,
                                                        false,
                                                        contentWidth);
                                    
                                                    AddPanelRow(
                                                        ref slot,
                                                        "ENTRY TRIGGER  " +
                                                        Price(
                                                            _executionModel.Trigger) +
                                                        "  •  INVALIDATION " +
                                                        Price(
                                                            _executionModel.Invalidation),
                                                        _executionModel.Ready
                                                            ? TriggerLineColor
                                                            : PanelWarningColor,
                                                        false,
                                                        contentWidth);
                                                }
                                    
            
        }
    }
}
