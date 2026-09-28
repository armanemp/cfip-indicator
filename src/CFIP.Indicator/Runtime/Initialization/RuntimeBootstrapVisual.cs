using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private const string RuntimeBootstrapObjectName = "CFIP_RUNTIME_BOOT";

        private void CreateRuntimeBootstrapVisual()
        {
            try
            {
                _runtimeBootstrapText =
                    Chart.DrawStaticText(
                        RuntimeBootstrapObjectName,
                        "CFIP  •  STARTING",
                        VerticalAlignment.Top,
                        HorizontalAlignment.Left,
                        Color.White);

                _runtimeBootstrapText.FontSize = 12;
                _runtimeBootstrapText.IsBold = true;
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP bootstrap visual creation failed: {0}",
                    ex.ToString());

                _runtimeBootstrapText = null;
            }
        }

        private void UpdateRuntimeBootstrapVisual(string message)
        {
            try
            {
                if (_runtimeBootstrapText == null)
                    CreateRuntimeBootstrapVisual();

                if (_runtimeBootstrapText != null)
                    _runtimeBootstrapText.Text =
                        string.IsNullOrWhiteSpace(message)
                            ? "CFIP  •  STARTING"
                            : "CFIP  •  " + message;
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP bootstrap visual update failed: {0}",
                    ex.ToString());
            }
        }

        private void RemoveRuntimeBootstrapVisual()
        {
            try
            {
                Chart.RemoveObject(
                    RuntimeBootstrapObjectName);
            }
            catch
            {
            }

            _runtimeBootstrapText = null;
        }
    }
}
