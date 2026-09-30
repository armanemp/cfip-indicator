using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

// CFIP Indicator — PopupRemover.cs
// Single-responsibility popup renderer.


namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void RemovePopup()
                        {
                            if (_popup != null)
                            {
                                try
                                {
                                    Chart.RemoveControl(
                                        _popup);
                                }
                                catch (Exception ex)
                                {
                                    Print(
                                        "CFIP popup removal failed: {0}",
                                        ex.ToString());
                                }
                            }
                
                            _popup = null;
                            _popupText = null;
                            _popupCloseButton = null;
                            _popupCritical = false;

                            _popupUntilUtc =
                                DateTime.MinValue;
                        }
    }
}
