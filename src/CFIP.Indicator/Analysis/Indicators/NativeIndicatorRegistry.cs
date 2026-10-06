// ============================================================================
// CFIP Indicator — NativeIndicatorRegistry.cs
// ============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    internal sealed class NativeBarsReferenceComparer :
        IEqualityComparer<Bars>
    {
        internal static readonly NativeBarsReferenceComparer Instance =
            new NativeBarsReferenceComparer();

        private NativeBarsReferenceComparer()
        {
        }

        bool IEqualityComparer<Bars>.Equals(
            Bars x,
            Bars y)
        {
            return ReferenceEquals(x, y);
        }

        int IEqualityComparer<Bars>.GetHashCode(
            Bars obj)
        {
            return
                obj == null
                    ? 0
                    : RuntimeHelpers.GetHashCode(obj);
        }
    }

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
        
        private Native RegisterNative(Bars bars)
                        {
                            if (bars == null)
                                return null;
                
                            Native existing;
                
                            if (_native.TryGetValue(
                                    bars,
                                    out existing))
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

                                set.IsInitialized = true;
                            }
                            catch (Exception ex)
                            {
                                set.IsInitialized = false;
                                Print(
                                    "CFIP indicator initialization failed: {0}",
                                    ex.Message);
                            }
                
                            _native[bars] = set;
                            return set;
                        }
        
        private Native GetNative(Bars bars)
                        {
                            if (bars == null)
                                return null;
                
                            Native set;
                
                            return
                                _native.TryGetValue(
                                    bars,
                                    out set)
                                    ? set
                                    : RegisterNative(bars);
                        }
    }
}
