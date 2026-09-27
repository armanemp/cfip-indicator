// CFIP Indicator — PopupExpirationCleaner.cs
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
        private void RemoveExpiredPopup()
                        {
                            if (_popup == null)
                                return;
                
                            if (KeepPopupUntilNextAlert)
                                return;
                
                            if (_popupUntilUtc >
                                TimeInUtc)
                                return;
                
                            RemovePopup();
                        }
    }
}
