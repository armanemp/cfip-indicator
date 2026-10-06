// ============================================================================
// CFIP Indicator — PanelFooterFactory.cs
// ============================================================================

using System;
using System.Collections.Generic;
using cAlgo.API;

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
                    "BUY",
                    BuyArrowColor,
                    out _panelBuyPressureTrack,
                    out _panelBuyPressureFill,
                    out _panelBuyPressureLabel);

            _panelSellPressureRow =
                CreateFlowPressureRow(
                    "SELL",
                    SellArrowColor,
                    out _panelSellPressureTrack,
                    out _panelSellPressureFill,
                    out _panelSellPressureLabel);

            _panelFlowPressureRail.AddChild(_panelBuyPressureRow);
            _panelFlowPressureRail.AddChild(_panelSellPressureRow);

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
                        Color.FromArgb(0, Color.Black)
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
                _panelBuyPressureTrack == null ||
                _panelSellPressureTrack == null ||
                _panelBuyPressureFill == null ||
                _panelSellPressureFill == null ||
                _panelBuyPressureLabel == null ||
                _panelSellPressureLabel == null)
                return;

            _panelFlowPressureRail.IsVisible =
                ShowUnifiedPanel &&
                !_panelHidden;

            int contentWidth =
                Math.Max(
                    1,
                    EffectivePanelContentWidth());

            _panelFlowPressureRail.Width =
                contentWidth;

            _panelBuyPressureRow.Width =
                contentWidth;

            _panelSellPressureRow.Width =
                contentWidth;

            _panelBuyPressureLabel.Width =
                contentWidth;

            _panelSellPressureLabel.Width =
                contentWidth;

            _panelBuyPressureTrack.Width =
                contentWidth;

            _panelSellPressureTrack.Width =
                contentWidth;

            double buyLiquidity;
            double sellLiquidity;

            bool ready =
                TryResolveCanonicalBuySellLiquidity(
                    out buyLiquidity,
                    out sellLiquidity);

            Color buyColor =
                BuyArrowColor;

            Color sellColor =
                SellArrowColor;

            double totalLiquidity =
                buyLiquidity +
                sellLiquidity;

            double buyShare =
                ready &&
                totalLiquidity > 0
                    ? buyLiquidity / totalLiquidity
                    : 0;

            double sellShare =
                ready &&
                totalLiquidity > 0
                    ? sellLiquidity / totalLiquidity
                    : 0;

            if (!ready)
            {
                _panelBuyPressureLabel.Text = "BUY LIQ --";
                _panelSellPressureLabel.Text = "SELL LIQ --";

                buyColor =
                    Color.FromArgb(
                        100,
                        PanelMutedTextColor);

                sellColor =
                    Color.FromArgb(
                        100,
                        PanelMutedTextColor);
            }
            else
            {
                int buyPercent =
                    totalLiquidity > 0
                        ? (int)Math.Round(
                            buyLiquidity /
                            totalLiquidity *
                            100.0)
                        : 50;

                int sellPercent =
                    100 - buyPercent;

                _panelBuyPressureLabel.Text =
                    "BUY LIQ " +
                    FormatRealtimeVolume(
                        buyLiquidity) +
                    " u (" +
                    buyPercent.ToString(
                        System.Globalization.CultureInfo.InvariantCulture) +
                    "%)";

                _panelSellPressureLabel.Text =
                    "SELL LIQ " +
                    FormatRealtimeVolume(
                        sellLiquidity) +
                    " u (" +
                    sellPercent.ToString(
                        System.Globalization.CultureInfo.InvariantCulture) +
                    "%)";
            }

            _panelBuyPressureLabel.ForegroundColor = buyColor;
            _panelSellPressureLabel.ForegroundColor = sellColor;

            _panelBuyPressureFill.BackgroundColor =
                Color.FromArgb(
                    225,
                    buyColor);

            _panelSellPressureFill.BackgroundColor =
                Color.FromArgb(
                    225,
                    sellColor);

            _panelBuyPressureFill.Width =
                Math.Max(
                    2,
                    (int)Math.Round(
                        contentWidth *
                        NumericGuards.ClampDouble(
                            buyShare,
                            0,
                            1)));

            _panelSellPressureFill.Width =
                Math.Max(
                    2,
                    (int)Math.Round(
                        contentWidth *
                        NumericGuards.ClampDouble(
                            sellShare,
                            0,
                            1)));
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
                {
                    _marketDepth =
                        MarketData.GetMarketDepth(
                            Symbol.Name);
                }

                if (_marketDepth == null)
                    return false;

                foreach (MarketDepthEntry entry in
                         _marketDepth.BidEntries)
                {
                    double volume =
                        entry.VolumeInUnits;

                    if (!double.IsNaN(volume) &&
                        !double.IsInfinity(volume) &&
                        volume > 0)
                    {
                        buyLiquidity += volume;
                    }
                }

                foreach (MarketDepthEntry entry in
                         _marketDepth.AskEntries)
                {
                    double volume =
                        entry.VolumeInUnits;

                    if (!double.IsNaN(volume) &&
                        !double.IsInfinity(volume) &&
                        volume > 0)
                    {
                        sellLiquidity += volume;
                    }
                }

                double total =
                    buyLiquidity +
                    sellLiquidity;

                return
                    NumericGuards.IsFinitePositive(total);
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
