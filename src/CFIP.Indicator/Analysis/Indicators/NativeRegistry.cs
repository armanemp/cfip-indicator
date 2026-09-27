using System;
using System.Collections.Generic;
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

        private Native RegisterNative(Bars bars)
        {
            if (bars == null)
                return null;

            Native existing =
                _native.FirstOrDefault(
                    x => ReferenceEquals(x.Bars, bars));

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
                InitializeMacd(set, bars);
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP indicator initialization failed: {0}",
                    ex.Message);
            }

            _native.Add(set);
            return set;
        }

private Native GetNative(Bars bars)
                {
                    if (bars == null)
                        return null;
        
                    Native set =
                        _native.FirstOrDefault(
                            x => ReferenceEquals(x.Bars, bars));
        
                    return set ?? RegisterNative(bars);
                }
    }
}
