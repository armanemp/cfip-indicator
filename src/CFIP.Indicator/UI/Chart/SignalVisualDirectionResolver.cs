using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private int ResolveCanonicalVisualDirection(
            int closedM5,
            int pendingDirection,
            bool livePlan,
            bool preTradePlanVisible,
            bool decisionReady,
            bool reactionReady,
            bool predictionReady)
        {
            if (!UseAuthoritativeSignalState)
            {
                return decisionReady &&
                       _decision != null
                    ? _decision.Direction
                    : 0;
            }
        
            if (livePlan &&
                _plan != null)
                return _plan.Direction;
        
            if (pendingDirection != 0)
                return pendingDirection;
        
            // Live Reaction is an internal M5/M1 precision signal only. It must
            // never become the authoritative chart trade direction while the
            // current M5 candle is still forming. The canonical visible direction
            // comes from the accepted Plan/Decision lifecycle below.
            if (preTradePlanVisible &&
                _plan != null &&
                (_plan.Direction == 1 ||
                 _plan.Direction == -1))
                return _plan.Direction;

            if (_decision != null)
            {
                // The chart arrow is a trade-signal visual, not a persistent
                // market-bias memory. It may only represent the current
                // canonical decision after both the decision gates and the
                // closed-M5 trigger have passed.
                if (_decision.Direction != 0 &&
                    _decision.EntryAllowed &&
                    _decision.TriggerReady &&
                    _decision.ActionableNow)
                    return _decision.Direction;

                return 0;
            }

            MarketRegimeSnapshot activeRegime =
                GetActiveM5Regime(
                    closedM5);
        
            if (activeRegime != null &&
                (activeRegime.Regime == "RANGE" ||
                 activeRegime.Regime == "COMPRESSION"))
            {
                // Prediction is informational only. Range/compression does not
                // receive an authoritative trade direction from the prediction
                // layer.
                return 0;
            }
        
            if (_m5Frame != null &&
                (_m5Frame.Direction == 1 ||
                 _m5Frame.Direction == -1) &&
                _m5Frame.Quality >=
                    Math.Max(
                        50,
                        LiveReactionStrongThreshold) &&
                ((_m5Frame.Direction == 1 &&
                  (_m5Frame.MssBull ||
                   _m5Frame.ChochBull)) ||
                 (_m5Frame.Direction == -1 &&
                  (_m5Frame.MssBear ||
                   _m5Frame.ChochBear))))
                return _m5Frame.Direction;
        
            // Prediction remains presentation-only and can never manufacture the
            // authoritative trade direction.
            return 0;
        }
        
        
    }
}
