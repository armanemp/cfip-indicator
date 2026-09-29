using System;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool TryAdvanceServerSideTakeProfitLadderAfterTp1(
            Position position,
            int closedM5,
            double market)
        {
            if (!_serverSideTakeProfitLadderActive ||
                position == null ||
                _plan == null ||
                _tp1Hit == 0 ||
                _tp2Hit != 0)
                return false;

            if (_m5Bars == null ||
                _m5Bars.Count < 3)
                return false;

            double atr =
                Atr(
                    _m5Bars,
                    Math.Max(
                        1,
                        _m5Bars.Count - 2));

            double minimumForwardDistance =
                Math.Max(
                    MinimumTakeProfitDistancePrice(),
                    Math.Max(
                        Symbol.PipSize,
                        Math.Max(
                            Symbol.TickSize,
                            atr *
                            Math.Max(
                                0.05,
                                MinimumTpSpacingAtr))));

            if (!IsFinitePositive(market) ||
                !IsFinitePositive(position.EntryPrice))
                return false;

            double tp2 =
                _plan.Tp2;

            double finalTarget =
                FurthestForwardPlanTarget(
                    market,
                    minimumForwardDistance);

            if (!LiveExitGeometryRule.ShouldAdvanceLiveTarget(
                    _plan.Direction,
                    0,
                    tp2,
                    market,
                    minimumForwardDistance))
                return false;

            if (!LiveExitGeometryRule.ShouldAdvanceLiveTarget(
                    _plan.Direction,
                    tp2,
                    finalTarget,
                    market,
                    minimumForwardDistance))
                return false;

            double tp2Volume =
                Symbol.NormalizeVolumeInUnits(
                    _plan.OriginalVolume *
                    PartialCloseTp2Percent /
                    100.0,
                    RoundingMode.Down);

            if (tp2Volume < Symbol.VolumeInUnitsMin ||
                tp2Volume >= position.VolumeInUnits)
                return false;

            double remainingAfterTp2 =
                position.VolumeInUnits -
                tp2Volume;

            if (remainingAfterTp2 < Symbol.VolumeInUnitsMin)
                return false;

            double entry =
                position.EntryPrice;

            double tp2Pips =
                Math.Abs(
                    tp2 -
                    entry) /
                Math.Max(
                    Symbol.PipSize,
                    1e-9);

            double finalPips =
                Math.Abs(
                    finalTarget -
                    entry) /
                Math.Max(
                    Symbol.PipSize,
                    1e-9);

            try
            {
                RelativeTakeProfitProtections protections =
                    new RelativeTakeProfitProtections(
                        new RelativeTakeProfitProtection(
                            tp2Volume,
                            tp2Pips),
                        new RelativeTakeProfitLastProtection(
                            finalPips));

                if (!TryModifyTakeProfitLadder(
                        position,
                        protections,
                        "LIVE TARGET PROGRESSION • AFTER TP1"))
                    return false;

                _activeBrokerTarget =
                    NormalizePrice(finalTarget);

                return true;
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP server TP1-followup ladder failed at M5 {0}: {1}",
                    closedM5,
                    ex.Message);
                return false;
            }
        }

        private bool TryAdvanceServerSideTakeProfitLadder(
            Position position,
            double market)
        {
            if (!_serverSideTakeProfitLadderActive ||
                position == null ||
                _plan == null ||
                _tp1Hit != 0 ||
                _tp2Hit != 0)
                return false;

            double minimumForwardDistance =
                Math.Max(
                    Symbol.PipSize,
                    Math.Max(
                        Symbol.TickSize,
                        (Atr(
                            _m5Bars,
                            Math.Max(
                                1,
                                _m5Bars.Count - 2)) *
                         Math.Max(
                             0.05,
                             MinimumTpSpacingAtr))));

            if (!IsFinitePositive(market) ||
                !IsFinitePositive(position.EntryPrice))
                return false;

            double finalTarget =
                FurthestForwardPlanTarget(
                    market,
                    minimumForwardDistance);

            if (!IsFinitePositive(finalTarget))
                return false;

            if (_activeBrokerTarget > 0 &&
                !LiveExitGeometryRule.ShouldAdvanceLiveTarget(
                    _plan.Direction,
                    _activeBrokerTarget,
                    finalTarget,
                    market,
                    minimumForwardDistance))
                return false;

            double tp1 =
                _plan.Tp1;

            double tp2 =
                _plan.Tp2;

            if (!LiveExitGeometryRule.ShouldAdvanceLiveTarget(
                    _plan.Direction,
                    0,
                    tp1,
                    market,
                    minimumForwardDistance) ||
                !LiveExitGeometryRule.ShouldAdvanceLiveTarget(
                    _plan.Direction,
                    tp1,
                    tp2,
                    market,
                    minimumForwardDistance) ||
                !LiveExitGeometryRule.ShouldAdvanceLiveTarget(
                    _plan.Direction,
                    tp2,
                    finalTarget,
                    market,
                    minimumForwardDistance))
                return false;

            double tp1Volume =
                Symbol.NormalizeVolumeInUnits(
                    _plan.OriginalVolume *
                    PartialCloseTp1Percent /
                    100.0,
                    RoundingMode.Down);

            double tp2Volume =
                Symbol.NormalizeVolumeInUnits(
                    _plan.OriginalVolume *
                    PartialCloseTp2Percent /
                    100.0,
                    RoundingMode.Down);

            if (tp1Volume < Symbol.VolumeInUnitsMin ||
                tp2Volume < Symbol.VolumeInUnitsMin ||
                tp1Volume + tp2Volume >=
                    position.VolumeInUnits)
                return false;

            double entry =
                position.EntryPrice;

            double tp1Pips =
                Math.Abs(tp1 - entry) /
                Math.Max(Symbol.PipSize, 1e-9);

            double tp2Pips =
                Math.Abs(tp2 - entry) /
                Math.Max(Symbol.PipSize, 1e-9);

            double finalPips =
                Math.Abs(finalTarget - entry) /
                Math.Max(Symbol.PipSize, 1e-9);

            try
            {
                RelativeTakeProfitProtections protections =
                    new RelativeTakeProfitProtections(
                        new RelativeTakeProfitProtection(
                            tp1Volume,
                            tp1Pips),
                        new RelativeTakeProfitProtection(
                            tp2Volume,
                            tp2Pips),
                        new RelativeTakeProfitLastProtection(
                            finalPips));

                if (!TryModifyTakeProfitLadder(
                        position,
                        protections,
                        "LIVE TARGET PROGRESSION"))
                    return false;

                _activeBrokerTarget =
                    NormalizePrice(finalTarget);

                return true;
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP dynamic server TP ladder failed: {0}",
                    ex.Message);
                return false;
            }
        }

        private double FurthestForwardPlanTarget(
            double market,
            double minimumForwardDistance)
        {
            if (_plan == null)
                return 0;

            double[] targets =
                new[]
                {
                    _plan.Tp1,
                    _plan.Tp2,
                    _plan.Tp3,
                    _plan.Tp4
                };

            double best = 0;

            for (int i = 0; i < targets.Length; i++)
            {
                double target =
                    targets[i];

                if (!IsFinitePositive(target))
                    continue;

                if (!LiveExitGeometryRule.ShouldAdvanceLiveTarget(
                        _plan.Direction,
                        0,
                        target,
                        market,
                        minimumForwardDistance))
                    continue;

                if (!IsFinitePositive(best) ||
                    (_plan.Direction == 1
                        ? target > best
                        : target < best))
                {
                    best = target;
                }
            }

            return best;
        }

        private bool TryCollapseServerSideTakeProfitLadderToFinal(
            Position position)
        {
            if (!_serverSideTakeProfitLadderActive ||
                position == null ||
                _plan == null)
                return false;

            double market =
                _plan.Direction == 1
                    ? Symbol.Bid
                    : Symbol.Ask;

            double minimumForwardDistance =
                Math.Max(
                    Symbol.PipSize,
                    Math.Max(
                        Symbol.TickSize,
                        Atr(
                            _m5Bars,
                            Math.Max(
                                1,
                                _m5Bars.Count - 2)) *
                        Math.Max(
                            0.05,
                            MinimumTpSpacingAtr)));

            double finalTarget =
                FurthestForwardPlanTarget(
                    market,
                    minimumForwardDistance);

            if (!IsFinitePositive(finalTarget) ||
                !LiveExitGeometryRule.ShouldAdvanceLiveTarget(
                    _plan.Direction,
                    0,
                    finalTarget,
                    market,
                    minimumForwardDistance))
                return false;

            double targetPips =
                Math.Abs(
                    finalTarget -
                    position.EntryPrice) /
                Math.Max(
                    Symbol.PipSize,
                    1e-9);

            if (!TryModifyTakeProfitPips(
                    position,
                    targetPips,
                    "POST-TP2 FINAL TARGET"))
                return false;

            _activeBrokerTarget =
                NormalizePrice(finalTarget);
            _serverSideTakeProfitLadderActive = false;
            _serverSideBreakEvenActive = false;

            return true;
        }


    }
}
