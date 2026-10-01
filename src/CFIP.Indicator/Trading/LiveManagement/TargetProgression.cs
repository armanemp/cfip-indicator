using System;
using System.Collections.Generic;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void UpdateUnhitTargetsLive(
            int closedM5,
            double market,
            bool forceStructuralUpdate = false)
        {
            if (_plan == null)
                return;

            if (StructuralTargetUpdatesOnly &&
                !forceStructuralUpdate &&
                _lastTargetRepriceM5 ==
                closedM5)
                return;

            double atr =
                Atr(
                    _m5Bars,
                    closedM5);

            if (atr <= 0)
                return;

            List<Level> levels =
                BuildTargetLevels(
                    closedM5,
                    _plan.Direction,
                    _plan.Entry,
                    atr);

            bool changed = false;

            for (int stage = 0;
                 stage < 4;
                 stage++)
            {
                int hit =
                    stage == 0
                        ? _tp1Hit
                        : stage == 1
                            ? _tp2Hit
                            : stage == 2
                                ? _tp3Hit
                                : _tp4Hit;

                if (hit != 0)
                    continue;

                double current =
                    stage == 0
                        ? _plan.Tp1
                        : stage == 1
                            ? _plan.Tp2
                            : stage == 2
                                ? _plan.Tp3
                                : _plan.Tp4;

                if (!IsFinitePositive(current))
                    continue;

                bool requireHtf =
                    RequiresHtfRewardForTargetStage(
                        stage,
                        _plan.Lane);

                double previousTarget =
                    stage == 0
                        ? _plan.Entry
                        : stage == 1
                            ? _plan.Tp1
                            : stage == 2
                                ? _plan.Tp2
                                : _plan.Tp3;

                double nextTarget =
                    stage == 0
                        ? _plan.Tp2
                        : stage == 1
                            ? _plan.Tp3
                            : stage == 2
                                ? _plan.Tp4
                                : 0;

                Level bestLevel =
                    FindImprovedLiveTarget(
                        levels,
                        closedM5,
                        current,
                        previousTarget,
                        nextTarget,
                        market,
                        atr,
                        0,
                        requireHtf);

                if (bestLevel == null ||
                    !IsFinitePositive(bestLevel.Price) ||
                    Math.Abs(
                        bestLevel.Price -
                        current) <
                    Symbol.PipSize)
                    continue;

                double normalizedBest =
                    NormalizePrice(bestLevel.Price);

                if (stage == 0)
                {
                    _plan.Tp1 = normalizedBest;
                    ApplyExactTargetMeta(
                        bestLevel,
                        _plan.Tp1,
                        out _plan.Tp1Source,
                        out _plan.Tp1Quality);
                }
                else if (stage == 1)
                {
                    _plan.Tp2 = normalizedBest;
                    ApplyExactTargetMeta(
                        bestLevel,
                        _plan.Tp2,
                        out _plan.Tp2Source,
                        out _plan.Tp2Quality);
                }
                else if (stage == 2)
                {
                    _plan.Tp3 = normalizedBest;
                    ApplyExactTargetMeta(
                        bestLevel,
                        _plan.Tp3,
                        out _plan.Tp3Source,
                        out _plan.Tp3Quality);
                }
                else
                {
                    _plan.Tp4 = normalizedBest;
                    ApplyExactTargetMeta(
                        bestLevel,
                        _plan.Tp4,
                        out _plan.Tp4Source,
                        out _plan.Tp4Quality);
                }

                changed = true;
            }

            _lastTargetRepriceM5 =
                closedM5;

            if (changed)
            {
                _plan.HtfTargetCount =
                    CountHtfTargetsInPlan(
                        _plan);
            }

            RecalculatePlanRR();

            if (_serverSideTakeProfitLadderActive)
            {
                Position position =
                    GetManagedLivePositionForPlan();

                if (position != null)
                    TryAdvanceServerSideTakeProfitLadder(
                        position,
                        closedM5,
                        market);
            }
        }
    }
}
