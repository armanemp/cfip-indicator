// ============================================================================
// CFIP Indicator — PanelFooterFactory.cs
// ============================================================================

using System;
using System.Collections.Generic;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private StackPanel _panelFooterActions;
        private StackPanel _panelFlowPressureRail;
        private StackPanel _panelBuyPressureRow;
        private Border _panelBuyPressureTrack;
        private Border _panelBuyPressureFill;
        private Border _panelSellPressureFill;
        private TextBlock _panelBuyPressureLabel;
        private TextBlock _panelBuyPressureLeftLabel;
        private TextBlock _panelBuyPressureRightLabel;
        private StackPanel _panelAggBuyFlowRow;
        private Border _panelAggBuyFlowTrack;
        private Border _panelAggBuyFlowFill;
        private Border _panelAggSellFlowFill;
        private TextBlock _panelAggBuyFlowLabel;
        private TextBlock _panelAggBuyFlowLeftLabel;
        private TextBlock _panelAggBuyFlowRightLabel;

        private void CreatePanelFooter()
        {
            _panelFlowPressureRail =
                new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Top,
                    Height = PanelFlowPressureRailHeight,
                    BackgroundColor = Color.FromArgb(0, Color.Black),
                    Margin = new Thickness(
                        0,
                        PanelFlowPressureTopSpacing,
                        0,
                        0),
                    IsHitTestVisible = false
                };

            _panelBuyPressureRow =
                CreateCombinedFlowPressureRow(
                    "DOM",
                    out _panelBuyPressureTrack,
                    out _panelBuyPressureFill,
                    out _panelSellPressureFill,
                    out _panelBuyPressureLabel,
                    out _panelBuyPressureLeftLabel,
                    out _panelBuyPressureRightLabel);

            _panelAggBuyFlowRow =
                CreateCombinedFlowPressureRow(
                    "FLOW TICKS",
                    out _panelAggBuyFlowTrack,
                    out _panelAggBuyFlowFill,
                    out _panelAggSellFlowFill,
                    out _panelAggBuyFlowLabel,
                    out _panelAggBuyFlowLeftLabel,
                    out _panelAggBuyFlowRightLabel);

            _panelBuyPressureRow.Margin =
                new Thickness(0, 0, 0, PanelFlowPressureRowGap);

            _panelFlowPressureRail.AddChild(_panelBuyPressureRow);
            _panelFlowPressureRail.AddChild(_panelAggBuyFlowRow);

            _panelFooterActions =
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Top,
                    Height = PanelFooterMinHeight -
                        PanelFlowPressureRailHeight -
                        PanelFlowPressureTopSpacing -
                        PanelFooterActionGap,
                    BackgroundColor = Color.FromArgb(0, Color.Black),
                    Margin = new Thickness(
                        0,
                        PanelFooterActionGap,
                        0,
                        0)
                };

            CreatePanelToggleButton();

            if (_panelToggleButton != null)
                _panelFooterActions.AddChild(_panelToggleButton);

            CreatePanelAlertMessageRail();

            if (_panelAlertMessageStack != null)
                _panelFooterActions.AddChild(_panelAlertMessageStack);

            _buttonStack.AddChild(_panelFlowPressureRail);
            _buttonStack.AddChild(_panelFooterActions);

            UpdatePanelFlowPressureRail();
        }

        private StackPanel CreateCombinedFlowPressureRow(
            string caption,
            out Border track,
            out Border buyFill,
            out Border sellFill,
            out TextBlock label,
            out TextBlock leftLabel,
            out TextBlock rightLabel)
        {
            label = null;
            leftLabel = CreatePressureSideLabel();
            rightLabel = CreatePressureSideLabel();

            buyFill = new Border
            {
                Height = PanelFlowPressureBarHeight,
                CornerRadius = 4,
                BorderThickness = 0,
                BackgroundColor = Color.FromArgb(225, BuyArrowColor),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center
            };

            sellFill = new Border
            {
                Height = PanelFlowPressureBarHeight,
                CornerRadius = 4,
                BorderThickness = 0,
                BackgroundColor = Color.FromArgb(225, SellArrowColor),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center
            };

            StackPanel segments = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Height = PanelFlowPressureBarHeight,
                BackgroundColor = Color.FromArgb(0, Color.Black)
            };
            segments.AddChild(buyFill);
            segments.AddChild(sellFill);

            track = new Border
            {
                Height = PanelFlowPressureBarHeight,
                CornerRadius = 4,
                BorderThickness = 0,
                BackgroundColor = Color.FromArgb(48, PanelMutedTextColor),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                Child = segments,
                Margin = new Thickness(0, PanelFlowPressureLabelToBarGap, 0, 0)
            };

            StackPanel labels = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Height = PanelFlowPressureLabelHeight
            };
            labels.AddChild(leftLabel);
            labels.AddChild(rightLabel);

            StackPanel row = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Height = PanelFlowPressureRowHeight,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                BackgroundColor = Color.FromArgb(0, Color.Black)
            };
            row.AddChild(labels);
            row.AddChild(track);
            return row;
        }

        private TextBlock CreatePressureSideLabel()
        {
            return new TextBlock
            {
                Text = string.Empty,
                Height = PanelFlowPressureLabelHeight,
                FontFamily = string.IsNullOrWhiteSpace(PanelFontFamily) ? "Arial" : PanelFontFamily,
                FontSize = Math.Max(8, PanelFontSize - 3),
                FontWeight = FontWeight.Bold,
                ForegroundColor = PanelMutedTextColor,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
                TextWrapping = TextWrapping.NoWrap,
                BackgroundColor = Color.FromArgb(0, Color.Black)
            };
        }

        private void UpdatePanelFlowPressureRail()
        {
            if (_panelFlowPressureRail == null ||
                _panelBuyPressureRow == null ||
                _panelAggBuyFlowRow == null)
                return;

            _panelFlowPressureRail.IsVisible =
                ShowUnifiedPanel &&
                !_panelHidden;

            int contentWidth = Math.Max(1, EffectivePanelContentWidth());

            SetCombinedFlowRowWidth(
                _panelBuyPressureRow,
                _panelBuyPressureTrack,
                _panelBuyPressureLabel,
                contentWidth);

            SetCombinedFlowRowWidth(
                _panelAggBuyFlowRow,
                _panelAggBuyFlowTrack,
                _panelAggBuyFlowLabel,
                contentWidth);

            double buyLiquidity;
            double sellLiquidity;
            bool ready = TryResolveCanonicalBuySellLiquidity(
                out buyLiquidity,
                out sellLiquidity);

            double domTotal = buyLiquidity + sellLiquidity;
            double domBuyShare =
                ready && domTotal > 0 ? buyLiquidity / domTotal : 0;
            double domSellShare =
                ready && domTotal > 0 ? sellLiquidity / domTotal : 0;

            if (!ready)
            {
                _panelBuyPressureLeftLabel.Text = "DOM BUY --";
                _panelBuyPressureRightLabel.Text = "SELL --";
            }
            else
            {
                int buyPercent = domTotal > 0
                    ? (int)Math.Round(buyLiquidity / domTotal * 100.0)
                    : 50;
                int sellPercent = 100 - buyPercent;

                _panelBuyPressureLeftLabel.Text = "DOM BUY " + FormatRealtimeVolume(buyLiquidity) + " (" + buyPercent.ToString(System.Globalization.CultureInfo.InvariantCulture) + "%)";
                _panelBuyPressureRightLabel.Text = "SELL " + FormatRealtimeVolume(sellLiquidity) + " (" + sellPercent.ToString(System.Globalization.CultureInfo.InvariantCulture) + "%)";
            }

            ApplyCombinedFlowBar(
                _panelBuyPressureFill,
                _panelSellPressureFill,
                _panelBuyPressureTrack,
                domBuyShare,
                domSellShare);

            AggressiveFlowSnapshot flow = GetAggressiveFlowSnapshot();
            double flowTotal = flow.BuyTicks + flow.SellTicks;
            double flowBuyShare =
                flowTotal > 0 ? flow.BuyTicks / flowTotal : 0;
            double flowSellShare =
                flowTotal > 0 ? flow.SellTicks / flowTotal : 0;

            if (flowTotal > 0)
            {
                _panelAggBuyFlowLeftLabel.Text = "FLOW BUY " + flow.BuyTicks.ToString(System.Globalization.CultureInfo.InvariantCulture) + " (" + ((int)Math.Round(flowBuyShare * 100.0)).ToString(System.Globalization.CultureInfo.InvariantCulture) + "%)";
                _panelAggBuyFlowRightLabel.Text = "SELL " + flow.SellTicks.ToString(System.Globalization.CultureInfo.InvariantCulture) + " (" + ((int)Math.Round(flowSellShare * 100.0)).ToString(System.Globalization.CultureInfo.InvariantCulture) + "%)";
            }
            else
            {
                _panelAggBuyFlowLeftLabel.Text = "FLOW BUY --";
                _panelAggBuyFlowRightLabel.Text = "SELL --";
            }

            ApplyCombinedFlowBar(
                _panelAggBuyFlowFill,
                _panelAggSellFlowFill,
                _panelAggBuyFlowTrack,
                flowBuyShare,
                flowSellShare);
        }

        private void SetCombinedFlowRowWidth(
            StackPanel row,
            Border track,
            TextBlock label,
            int width)
        {
            if (row == null || track == null)
                return;

            row.Width = width;
            track.Width = width;
            if (label != null)
                label.Width = width;

            TextBlock left = row == _panelBuyPressureRow
                ? _panelBuyPressureLeftLabel
                : _panelAggBuyFlowLeftLabel;
            TextBlock right = row == _panelBuyPressureRow
                ? _panelBuyPressureRightLabel
                : _panelAggBuyFlowRightLabel;

            if (left != null)
                left.Width = Math.Max(1, width / 2 - PanelFlowPressureLabelSideGap);
            if (right != null)
            {
                right.Width = Math.Max(1, width / 2);
                right.TextAlignment = TextAlignment.Right;
            }
        }

        private void ApplyCombinedFlowBar(
            Border buyFill,
            Border sellFill,
            Border track,
            double buyShare,
            double sellShare)
        {
            if (buyFill == null || sellFill == null || track == null)
                return;

            double normalizedBuy =
                NumericGuards.ClampDouble(buyShare, 0, 1);
            double normalizedSell =
                NumericGuards.ClampDouble(sellShare, 0, 1);

            double total =
                normalizedBuy + normalizedSell;

            if (total <= 0)
            {
                normalizedBuy = 0.5;
                normalizedSell = 0.5;
            }
            else
            {
                normalizedBuy /= total;
                normalizedSell /= total;
            }

            int width = Math.Max(2, (int)Math.Round(track.Width));
            int buyWidth =
                Math.Max(1, (int)Math.Round(width * normalizedBuy));
            int sellWidth =
                Math.Max(1, width - buyWidth);

            buyFill.Width = buyWidth;
            sellFill.Width = sellWidth;

            buyFill.BackgroundColor =
                Color.FromArgb(225, BuyArrowColor);
            sellFill.BackgroundColor =
                Color.FromArgb(225, SellArrowColor);
        }

        private bool TryResolveCanonicalBuySellLiquidity(
            out double buyLiquidity,
            out double sellLiquidity)
        {
            buyLiquidity = 0;
            sellLiquidity = 0;

            try
            {
                if (_marketDepth == null)
                    _marketDepth =
                        MarketData.GetMarketDepth(Symbol.Name);

                if (_marketDepth == null)
                    return false;

                foreach (MarketDepthEntry entry in _marketDepth.BidEntries)
                {
                    double volume = entry.VolumeInUnits;
                    if (!double.IsNaN(volume) &&
                        !double.IsInfinity(volume) &&
                        volume > 0)
                        buyLiquidity += volume;
                }

                foreach (MarketDepthEntry entry in _marketDepth.AskEntries)
                {
                    double volume = entry.VolumeInUnits;
                    if (!double.IsNaN(volume) &&
                        !double.IsInfinity(volume) &&
                        volume > 0)
                        sellLiquidity += volume;
                }

                return NumericGuards.IsFinitePositive(
                    buyLiquidity + sellLiquidity);
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP realtime market-depth snapshot failed: {0}",
                    ex.Message);
                buyLiquidity = 0;
                sellLiquidity = 0;
                return false;
            }
        }

        private string FormatRealtimeVolume(
            double volume)
        {
            if (double.IsNaN(volume) ||
                double.IsInfinity(volume) ||
                volume < 0)
                return "--";

            if (volume >= 1000000000.0)
                return (volume / 1000000000.0).ToString(
                    "0.00",
                    System.Globalization.CultureInfo.InvariantCulture) + "B";

            if (volume >= 1000000.0)
                return (volume / 1000000.0).ToString(
                    "0.00",
                    System.Globalization.CultureInfo.InvariantCulture) + "M";

            if (volume >= 1000.0)
                return (volume / 1000.0).ToString(
                    "0.00",
                    System.Globalization.CultureInfo.InvariantCulture) + "K";

            return volume.ToString(
                "0.##",
                System.Globalization.CultureInfo.InvariantCulture);
        }

    }
}
