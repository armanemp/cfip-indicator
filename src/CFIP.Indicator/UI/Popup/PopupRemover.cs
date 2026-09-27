// CFIP Indicator — PopupRemover.cs
Single-responsibility popup renderer.

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
        private void RemovePopup()
                        {
                            if (_popup != null)
                            {
                                try
                                {
                                    Chart.RemoveControl(
                                        _popup);
                                }
                                catch
                                {
                                }
                            }
                
                            _popup = null;
                            _popupText = null;
                            _popupCloseButton = null;
                            _popupUntilUtc =
                                DateTime.MinValue;
                        }
    }
}
