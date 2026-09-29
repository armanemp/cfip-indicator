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
        private void ApplyPanelQuickExecutionLayout(
                                                    int contentWidth,
                                                    int buttonGap)
                                                {
                                                    _quickExecutionStack.IsVisible =
                                                        true;
                                        
                                                    if (_quickExecutionStack != null)
                                                    {
                                                        int quickGap =
                                                            Math.Max(
                                                                2,
                                                                buttonGap);
                                        
                                                        int quickWidth =
                                                            Math.Max(
                                                                108,
                                                                (contentWidth -
                                                                 quickGap) / 2);
                                        
                                                        int quickRightMargin =
                                                            quickGap / 2;
                                        
                                                        int quickLeftMargin =
                                                            quickGap -
                                                            quickRightMargin;
                                        
                                                        _quickExecutionStack.Width =
                                                            contentWidth;
                                                        _quickExecutionStack.Height =
                                                            QuickExecutionRowHeight;
                                        
                                                        if (_autoTradingQuickStatus != null)
                                                        {
                                                            _autoTradingQuickStatus.Width =
                                                                quickWidth;
                                                            _autoTradingQuickStatus.Height =
                                                                QuickExecutionButtonHeight;
                                                            _autoTradingQuickStatus.Margin =
                                                                new Thickness(
                                                                    0,
                                                                    QuickExecutionVerticalMargin,
                                                                    quickRightMargin,
                                                                    QuickExecutionVerticalMargin);
                                                        }
                                        
                                                        if (_automaticOrdersQuickStatus != null)
                                                        {
                                                            _automaticOrdersQuickStatus.Width =
                                                                quickWidth;
                                                            _automaticOrdersQuickStatus.Height =
                                                                QuickExecutionButtonHeight;
                                                            _automaticOrdersQuickStatus.Margin =
                                                                new Thickness(
                                                                    quickLeftMargin,
                                                                    QuickExecutionVerticalMargin,
                                                                    0,
                                                                    QuickExecutionVerticalMargin);
                                                        }
                                                    }
                                        
                                                }
    }
}
