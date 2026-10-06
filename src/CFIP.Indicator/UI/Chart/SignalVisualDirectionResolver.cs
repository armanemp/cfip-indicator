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
        
            if (preTradePlanVisible &&
                _plan != null &&
                (_plan.Direction == 1 ||
                 _plan.Direction == -1))
                return _plan.Direction;
        
            if (_decision != null)
            {
                // The visible direction follows the lifecycle-accepted
                // decision, not a transient recomputation that is blocked
                // by the anti-flip gate.
                if (_decision.Direction != 0)
                {
                    if (_decision.EntryAllowed)
                        return _decision.Direction;

                    if (_lastConfirmedDirection != 0)
                        return _lastConfirmedDirection;

                    return 0;
                }

                // Once a direction has been accepted, a temporary neutral
                // decision cannot let the live reaction layer flip the UI.
                // A new opposite direction must pass DirectionAcceptanceGate.
                if (_lastConfirmedDirection != 0)
                    return _lastConfirmedDirection;
            }
        
            if (reactionReady &&
                _reaction != null)
                return _reaction.Direction;
        
            MarketRegimeSnapshot activeRegime =
                GetActiveM5Regime(
                    closedM5);
        
            if (activeRegime != null &&
                (activeRegime.Regime == "RANGE" ||
                 activeRegime.Regime == "COMPRESSION"))
            {
                if (predictionReady &&
                    _prediction != null)
                    return _prediction.Direction;
        
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
        
            if (predictionReady &&
                _prediction != null)
                return _prediction.Direction;
        
            return 0;
        }
        
        
    }
}
