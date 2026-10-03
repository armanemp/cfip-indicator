using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private TextBlock _processingLamp;
        private int _processingLampPulseIndex;

        private void CreateProcessingHeartbeatLamp()
        {
            if (_processingLamp != null)
                return;

            _processingLamp =
                new TextBlock
                {
                    Text = "●",
                    Width = PanelStatusLampWidth,
                    Height = PanelStatusLampHeight,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextAlignment = TextAlignment.Center,
                    FontFamily =
                        string.IsNullOrWhiteSpace(PanelFontFamily)
                            ? "Arial"
                            : PanelFontFamily,
                    FontSize = PanelStatusLampFontSize,
                    FontWeight = FontWeight.ExtraBold,
                    ForegroundColor = Color.FromArgb(150, 80, 240, 150),
                    BackgroundColor = Color.FromArgb(0, 0, 0, 0)
                };
        }

        private void UpdateProcessingHeartbeatLamp()
        {
            if (_processingLamp == null)
                return;

            _processingLampPulseIndex =
                (_processingLampPulseIndex + 1) % 6;

            bool bright =
                _processingLampPulseIndex == 0 ||
                _processingLampPulseIndex == 1 ||
                _processingLampPulseIndex == 5;

            _processingLamp.Text =
                bright
                    ? "●"
                    : "○";

            _processingLamp.ForegroundColor =
                bright
                    ? Color.FromArgb(255, 70, 255, 145)
                    : Color.FromArgb(120, 70, 220, 135);
        }
    }
}
