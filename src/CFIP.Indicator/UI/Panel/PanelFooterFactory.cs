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
        private StackPanel _panelSellPressureRow;
        private Border _panelBuyPressureTrack;
        private Border _panelSellPressureTrack;
        private Border _panelBuyPressureFill;
        private Border _panelSellPressureFill;
        private TextBlock _panelBuyPressureLabel;
        private TextBlock _panelSellPressureLabel;
        private StackPanel _panelAggBuyFlowRow;
        private StackPanel _panelAggSellFlowRow;
        private Border _panelAggBuyFlowTrack;
        private Border _panelAggSellFlowTrack;
        private Border _panelAggBuyFlowFill;
        private Border _panelAggSellFlowFill;
        private TextBlock _panelAggBuyFlowLabel;
        private TextBlock _panelAggSellFlowLabel;

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
                    IsHitTestVisible = false
                };

            _panelBuyPressureRow =
                CreateFlowPressureRow(
                    "DOM BUY",
                    BuyArrowColor,
                    out _panelBuyPressureTrack,
                    out _panelBuyPressureFill,
                    out _panelBuyPressureLabel);

            _panelSellPressureRow =
                CreateFlowPressureRow(
                    "DOM SELL",
                    SellArrowColor,
                    out _panelSellPressureTrack,
                    out _panelSellPressureFill,
                    out _panelSellPressureLabel);

            _panelAggBuyFlowRow =
                CreateFlowPressureRow(
                    "FLOW BUY TICKS",
                    BuyArrowColor,
                    out _panelAggBuyFlowTrack,
                    out _panelAggBuyFlowFill,
                    out _panelAggBuyFlowLabel);

            _panelAggSellFlowRow =
                CreateFlowPressureRow(
                    "FLOW SELL TICKS",
                    SellArrowColor,
                    out _panelAggSellFlowTrack,
                    out _panelAggSellFlowFill,
                    out _panelAggSellFlowLabel);

            _panelFlowPressureRail.AddChild(_panelBuyPressureRow);
            _panelFlowPressureRail.AddChild(_panelSellPressureRow);
            _panelFlowPressureRail.AddChild(_panelAggBuyFlowRow);
            _panelFlowPressureRail.AddChild(_panelAggSellFlowRow);

            _panelFooterActions =
                new StackPanel
                {
                    Orientation =
                        Orientation.Horizontal,
                    HorizontalAlignment =
                        HorizontalAlignment.Stretch,
                    VerticalAlignment =
                        VerticalAlignment.Top,
                    Height = PanelFooterMinHeight -
                        PanelFlowPressureRailHeight -
                        PanelFooterActionGap,
                    BackgroundColor =
                        Color.FromArgb(0, Color.Black),
                    Margin =
                        new Thickness(
                            0,
                            PanelFooterActionGap,
                            0,
                            0)
                };

            // The Indicator panel is analysis/presentation only.
            // Broker Close/Cancel actions belong to the cBot surface.
            // The hide/show toggle is the only interactive control
            // owned by the Indicator panel. Broker actions are cBot-owned.
            CreatePanelToggleButton();

            if (_panelToggleButton != null)
                _panelFooterActions.AddChild(
                    _panelToggleButton);

            CreatePanelAlertMessageRail();

            if (_panelAlertMessageStack != null)
                _panelFooterActions.AddChild(
                    _panelAlertMessageStack);

            _buttonStack.AddChild(
                _panelFlowPressureRail);

            _buttonStack.AddChild(
                _panelFooterActions);

            UpdatePanelFlowPressureRail();
        }

        private StackPanel CreateFlowPressureRow(
            string caption,
            Color accent,
            out Border track,
            out Border fill,
            out TextBlock label)
        {
            label =
                new TextBlock
                {
                    Text = caption + " --",
                    Height = PanelFlowPressureLabelHeight,
                    FontFamily =
                        string.IsNullOrWhiteSpace(PanelFontFamily)
                            ? "Arial"
                            : PanelFontFamily,
                    FontSize = Math.Max(8, PanelFontSize - 3),
                    FontWeight = FontWeight.Bold,
                    ForegroundColor = accent,
                    TextAlignment = TextAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    TextWrapping = TextWrapping.NoWrap,
                    BackgroundColor = Color.FromArgb(0, Color.Black)
                };

            fill =
                new Border
                {
                    Height = PanelFlowPressureBarHeight,
                    CornerRadius = 4,
                    BorderThickness = 0,
                    BorderColor = Color.FromArgb(0, Color.Black),
                    BackgroundColor =
                        Color.FromArgb(
                            225,
                            accent),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center
                };

            track =
                new Border
                {
                    Height = PanelFlowPressureBarHeight,
                    CornerRadius = 4,
                    BorderThickness = 0,
                    BorderColor = Color.FromArgb(0, Color.Black),
                    BackgroundColor =
                        Color.FromArgb(
                            48,
                            PanelMutedTextColor),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Center,
                    Child = fill
                };

            StackPanel row =
                new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Height = PanelFlowPressureRowHeight,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Top,
                    BackgroundColor = Color.FromArgb(0, Color.Black)
                };

            row.AddChild(label);
            row.AddChild(track);

            return row;
        }

        private void UpdatePanelFlowPressureRail()
        {
            if (_panelFlowPressureRail == null ||
                _panelBuyPressureRow == null ||
                _panelSellPressureRow == null ||
                _panelAggBuyFlowRow == null ||
                _panelAggSellFlowRow == null)
                return;

            _panelFlowPressureRail.IsVisible =
                ShowUnifiedPanel &&
                !_panelHidden;

            int contentWidth = Math.Max(1, EffectivePanelContentWidth());

            SetFlowRowWidth(
                _panelBuyPressureRow,
                _panelBuyPressureTrack,
                _panelBuyPressureLabel,
                contentWidth);
            SetFlowRowWidth(
                _panelSellPressureRow,
                _panelSellPressureTrack,
                _panelSellPressureLabel,
                contentWidth);
            SetFlowRowWidth(
                _panelAggBuyFlowRow,
                _panelAggBuyFlowTrack,
                _panelAggBuyFlowLabel,
                contentWidth);
            SetFlowRowWidth(
                _panelAggSellFlowRow,
                _panelAggSellFlowTrack,
                _panelAggSellFlowLabel,
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
                _panelBuyPressureLabel.Text = "DOM BUY --";
                _panelSellPressureLabel.Text = "DOM SELL --";
            }
            else
            {
                int buyPercent = domTotal > 0
                    ? (int)Math.Round(buyLiquidity / domTotal * 100.0)
                    : 50;
                int sellPercent = 100 - buyPercent;

                _panelBuyPressureLabel.Text =
                    "DOM BUY " + FormatRealtimeVolume(buyLiquidity) +
                    " u (" + buyPercent.ToString(
                        System.Globalization.CultureInfo.InvariantCulture) + "%)";

                _panelSellPressureLabel.Text =
                    "DOM SELL " + FormatRealtimeVolume(sellLiquidity) +
                    " u (" + sellPercent.ToString(
                        System.Globalization.CultureInfo.InvariantCulture) + "%)";
            }

            ApplyFlowBar(
                _panelBuyPressureFill,
                _panelBuyPressureTrack,
                domBuyShare,
                BuyArrowColor);
            ApplyFlowBar(
                _panelSellPressureFill,
                _panelSellPressureTrack,
                domSellShare,
                SellArrowColor);

            AggressiveFlowSnapshot flow = GetAggressiveFlowSnapshot();
            double flowTotal = flow.BuyTicks + flow.SellTicks;
            double flowBuyShare =
                flowTotal > 0 ? flow.BuyTicks / flowTotal : 0;
            double flowSellShare =
                flowTotal > 0 ? flow.SellTicks / flowTotal : 0;

            _panelAggBuyFlowLabel.Text =
                flowTotal > 0
                    ? "FLOW BUY TICKS " + flow.BuyTicks.ToString(
                        System.Globalization.CultureInfo.InvariantCulture) +
                      " (" + ((int)Math.Round(flowBuyShare * 100.0)).ToString(
                        System.Globalization.CultureInfo.InvariantCulture) + "%)"
                    : "FLOW BUY TICKS --";

            _panelAggSellFlowLabel.Text =
                flowTotal > 0
                    ? "FLOW SELL TICKS " + flow.SellTicks.ToString(
                        System.Globalization.CultureInfo.InvariantCulture) +
                      " (" + ((int)Math.Round(flowSellShare * 100.0)).ToString(
                        System.Globalization.CultureInfo.InvariantCulture) + "%)"
                    : "FLOW SELL TICKS --";

            ApplyFlowBar(
                _panelAggBuyFlowFill,
                _panelAggBuyFlowTrack,
                flowBuyShare,
                BuyArrowColor);
            ApplyFlowBar(
                _panelAggSellFlowFill,
                _panelAggSellFlowTrack,
                flowSellShare,
                SellArrowColor);
        }

        private void SetFlowRowWidth(
            StackPanel row,
            Border track,
            TextBlock label,
            int width)
        {
            if (row == null || track == null || label == null)
                return;

            row.Width = width;
            track.Width = width;
            label.Width = width;
        }

        private void ApplyFlowBar(
            Border fill,
            Border track,
            double share,
            Color color)
        {
            if (fill == null || track == null)
                return;

            fill.BackgroundColor = Color.FromArgb(225, color);
            fill.Width = Math.Max(
                2,
                (int)Math.Round(
                    track.Width *
                    NumericGuards.ClampDouble(share, 0, 1)));
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
            {
                return (volume / 1000000000.0).ToString(
                    "0.00",
                    System.Globalization.CultureInfo.InvariantCulture) +
                    "B";
            }

            if (volume >= 1000000.0)
            {
                return (volume / 1000000.0).ToString(
                    "0.00",
                    System.Globalization.CultureInfo.InvariantCulture) +
                    "M";
            }

            if (volume >= 1000.0)
            {
                return (volume / 1000.0).ToString(
                    "0.00",
                    System.Globalization.CultureInfo.InvariantCulture) +
                    "K";
            }

            return volume.ToString(
                "0.##",
                System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
