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

            double buyPressure;
            double sellPressure;

            bool ready =
                TryResolveCanonicalBuySellPressure(
                    out buyPressure,
                    out sellPressure);

            Color buyColor =
                BuyArrowColor;

            Color sellColor =
                SellArrowColor;

            if (!ready)
            {
                buyPressure = 0.5;
                sellPressure = 0.5;

                _panelBuyPressureLabel.Text = "BUY --";
                _panelSellPressureLabel.Text = "SELL --";

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
                    (int)Math.Round(
                        buyPressure * 100.0);

                int sellPercent =
                    100 - buyPercent;

                _panelBuyPressureLabel.Text =
                    "BUY " +
                    buyPercent.ToString(
                        System.Globalization.CultureInfo.InvariantCulture) +
                    "%";

                _panelSellPressureLabel.Text =
                    "SELL " +
                    sellPercent.ToString(
                        System.Globalization.CultureInfo.InvariantCulture) +
                    "%";
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
                            buyPressure,
                            0,
                            1)));

            _panelSellPressureFill.Width =
                Math.Max(
                    2,
                    (int)Math.Round(
                        contentWidth *
                        NumericGuards.ClampDouble(
                            sellPressure,
                            0,
                            1)));
        }

        private bool TryResolveCanonicalBuySellPressure(
            out double buyPressure,
            out double sellPressure)
        {
            buyPressure = 0.5;
            sellPressure = 0.5;

            Bars bars = _m15Bars;

            if (bars == null ||
                bars.Count < 1)
                return false;

            // Use the latest three M15 bars, including the active bar. This is
            // intentionally realtime: cTrader updates TickVolumes/High/Low/Close
            // on the current bar while the panel refresh remains bounded.
            int latestBar =
                bars.Count - 1;

            int first =
                Math.Max(
                    0,
                    latestBar - 2);

            double buyVolume = 0;
            double sellVolume = 0;

            for (int i = first;
                 i <= latestBar;
                 i++)
            {
                double high =
                    bars.HighPrices[i];

                double low =
                    bars.LowPrices[i];

                double close =
                    bars.ClosePrices[i];

                double volume =
                    bars.TickVolumes[i];

                if (double.IsNaN(high) ||
                    double.IsInfinity(high) ||
                    double.IsNaN(low) ||
                    double.IsInfinity(low) ||
                    double.IsNaN(close) ||
                    double.IsInfinity(close) ||
                    double.IsNaN(volume) ||
                    double.IsInfinity(volume) ||
                    volume <= 0 ||
                    high <= low)
                    continue;

                double buyShare =
                    NumericGuards.ClampDouble(
                        (close - low) /
                        Math.Max(
                            Symbol.TickSize,
                            high - low),
                        0,
                        1);

                buyVolume +=
                    volume *
                    buyShare;

                sellVolume +=
                    volume *
                    (1.0 - buyShare);
            }

            double total =
                buyVolume +
                sellVolume;

            if (!NumericGuards.IsFinitePositive(total))
                return false;

            buyPressure =
                NumericGuards.ClampDouble(
                    buyVolume / total,
                    0,
                    1);

            sellPressure =
                NumericGuards.ClampDouble(
                    1.0 - buyPressure,
                    0,
                    1);

            return true;
        }
    }
}
