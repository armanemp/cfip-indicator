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

            StackPanel buyRow;
            _panelBuyPressureTrack = CreateFlowPressureRow(
                "BUY",
                BuyArrowColor,
                out _panelBuyPressureLabel,
                out _panelBuyPressureFill);
            buyRow = _panelBuyPressureTrack.Child as StackPanel == null
                ? null
                : null;

            StackPanel sellRow;
            _panelSellPressureTrack = CreateFlowPressureRow(
                "SELL",
                SellArrowColor,
                out _panelSellPressureLabel,
                out _panelSellPressureFill);
            sellRow = _panelSellPressureTrack.Child as StackPanel == null
                ? null
                : null;

            // CreateFlowPressureRow returns the bar track; its parent row is
            // retained internally by attaching it to the rail here.
            _panelFlowPressureRail.AddChild(
                CreateFlowPressureRowContainer(
                    _panelBuyPressureTrack));
            _panelFlowPressureRail.AddChild(
                CreateFlowPressureRowContainer(
                    _panelSellPressureTrack));

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

        private Border CreateFlowPressureRow(
            string caption,
            Color accent,
            out TextBlock label,
            out Border fill)
        {
            label =
                new TextBlock
                {
                    Text = caption + " --",
                    Width = 0,
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
                    Width = 0,
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

            Border track =
                new Border
                {
                    Width = 0,
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
                    Width = 0,
                    Height = PanelFlowPressureRowHeight,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Top,
                    BackgroundColor = Color.FromArgb(0, Color.Black)
                };

            row.AddChild(label);
            row.AddChild(track);

            // Keep the row container associated with its track without adding
            // a second visual owner.
            track.Tag = row;

            return track;
        }

        private StackPanel CreateFlowPressureRowContainer(
            Border track)
        {
            if (track == null ||
                !(track.Tag is StackPanel row))
                return null;

            track.Tag = null;
            return row;
        }

        private void UpdatePanelFlowPressureRail()
        {
            if (_panelFlowPressureRail == null ||
                _panelBuyPressureTrack == null ||
                _panelSellPressureTrack == null)
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

            _panelBuyPressureTrack.Width =
                contentWidth;

            _panelSellPressureTrack.Width =
                contentWidth;

            if (_panelBuyPressureTrack.Child is Border buyFill)
                _panelBuyPressureFill = buyFill;

            if (_panelSellPressureTrack.Child is Border sellFill)
                _panelSellPressureFill = sellFill;

            double buyPressure;
            double sellPressure;
            bool ready =
                TryResolveCanonicalBuySellPressure(
                    out buyPressure,
                    out sellPressure);

            if (!ready)
            {
                buyPressure = 0.5;
                sellPressure = 0.5;
                _panelBuyPressureLabel.Text = "BUY --";
                _panelSellPressureLabel.Text = "SELL --";
                _panelBuyPressureFill.BackgroundColor =
                    Color.FromArgb(
                        70,
                        PanelMutedTextColor);
                _panelSellPressureFill.BackgroundColor =
                    Color.FromArgb(
                        70,
                        PanelMutedTextColor);
            }
            else
            {
                _panelBuyPressureLabel.Text =
                    "BUY " +
                    Math.Round(
                        buyPressure * 100.0)
                    .ToString(
                        "0",
                        System.Globalization.CultureInfo.InvariantCulture) +
                    "%";

                _panelSellPressureLabel.Text =
                    "SELL " +
                    Math.Round(
                        sellPressure * 100.0)
                    .ToString(
                        "0",
                        System.Globalization.CultureInfo.InvariantCulture) +
                    "%";

                _panelBuyPressureFill.BackgroundColor =
                    Color.FromArgb(
                        225,
                        BuyArrowColor);
                _panelSellPressureFill.BackgroundColor =
                    Color.FromArgb(
                        225,
                        SellArrowColor);
            }

            int buyWidth =
                Math.Max(
                    2,
                    (int)Math.Round(
                        contentWidth *
                        NumericGuards.ClampDouble(
                            buyPressure,
                            0,
                            1)));

            int sellWidth =
                Math.Max(
                    2,
                    (int)Math.Round(
                        contentWidth *
                        NumericGuards.ClampDouble(
                            sellPressure,
                            0,
                            1)));

            _panelBuyPressureFill.Width = buyWidth;
            _panelSellPressureFill.Width = sellWidth;
        }

        private bool TryResolveCanonicalBuySellPressure(
            out double buyPressure,
            out double sellPressure)
        {
            buyPressure = 0.5;
            sellPressure = 0.5;

            Bars bars = _m15Bars;

            if (bars == null ||
                bars.Count < 2)
                return false;

            int lastClosed =
                bars.Count - 2;

            int first =
                Math.Max(
                    0,
                    lastClosed - 2);

            double buyVolume = 0;
            double sellVolume = 0;

            for (int i = first;
                 i <= lastClosed;
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
