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
                       _decision != null &&
                       _decision.ActionableNow
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
                // Trade arrows represent the final actionable state only.
                // Plan geometry has its own bounded lifecycle so Entry/Trigger/
                // SL/TP lines do not disappear merely because this instantaneous
                // arrow gate is not ready.
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
                return 0;

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

            return 0;
        }
    }
}