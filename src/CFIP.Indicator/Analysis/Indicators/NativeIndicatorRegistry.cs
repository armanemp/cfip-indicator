// ============================================================================
// CFIP Indicator — NativeIndicatorRegistry.cs
// ============================================================================

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
        private void RegisterAllNative()
                        {
                            RegisterNative(_m1Bars);
                            RegisterNative(_m5Bars);
                            RegisterNative(_m15Bars);
                            RegisterNative(_m30Bars);
                            RegisterNative(_h1Bars);
                            RegisterNative(_h4Bars);
                            RegisterNative(_d1Bars);
                            RegisterNative(_w1Bars);
                        }
        
        private Native FindRegisteredNative(Bars bars)
        {
            if (bars == null)
                return null;

            if (ReferenceEquals(_lastNativeBars, bars))
                return _lastNative;

            for (int i = 0; i < _native.Count; i++)
            {
                Native candidate = _native[i];

                if (candidate != null &&
                    ReferenceEquals(candidate.Bars, bars))
                {
                    _lastNativeBars = bars;
                    _lastNative = candidate;
                    return candidate;
                }
            }

            return null;
        }

        private void RememberNative(
            Bars bars,
            Native set)
        {
            _lastNativeBars = bars;
            _lastNative = set;
        }

        private Native RegisterNative(Bars bars)
        {
            if (bars == null)
                return null;

            Native existing =
                FindRegisteredNative(bars);

            if (existing != null)
                return existing;

            Native set = new Native();
            set.Bars = bars;

            try
            {
                InitializeExponentialMovingAverages(set, bars);
                InitializeAverageTrueRange(set, bars);
                InitializeRelativeStrengthIndex(set, bars);
                InitializeDirectionalMovementSystem(set, bars);

                if (UseMacdBias)
                    InitializeMacd(
                        set,
                        bars);
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP indicator initialization failed: {0}",
                    ex.Message);
            }

            _native.Add(set);
            RememberNative(bars, set);
            return set;
        }

        private Native GetNative(Bars bars)
        {
            if (bars == null)
                return null;

            Native set =
                FindRegisteredNative(bars);

            return
                set ??
                RegisterNative(bars);
        }
    }
}
