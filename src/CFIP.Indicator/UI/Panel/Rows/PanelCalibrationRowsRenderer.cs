namespace cAlgo
{
    public partial class CFIPIndicator
    {
        private void RenderPanelCalibrationRows(
            ref int slot,
            int contentWidth)
        {
            if (_decision == null ||
                _decision.EmpiricalCalibrationSamples <= 0)
                return;

            string adjustmentText =
                _decision.EmpiricalCalibrationAdjustment > 0
                    ? "+" + _decision.EmpiricalCalibrationAdjustment
                    : _decision.EmpiricalCalibrationAdjustment.ToString();

            AddPanelRow(
                ref slot,
                "CAL " +
                _decision.Confidence +
                "  •  BASE " +
                _decision.BaseConfidence +
                "  •  ADJ " +
                adjustmentText +
                "  •  OBS WIN " +
                (_decision.EmpiricalCalibrationObservedWinRate * 100.0).ToString("F0") +
                "%  •  N" +
                _decision.EmpiricalCalibrationSamples +
                "  •  " +
                _decision.EmpiricalCalibrationSource,
                PanelAccentColor,
                false,
                contentWidth);
        }
    }
}