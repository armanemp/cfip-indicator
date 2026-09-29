using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool TryBuildServerSideTakeProfitLadder(
            double executionEntry,
            double finalTarget,
            double originalVolume,
            out RelativeTakeProfitProtections takeProfits,
            out StopLossBreakEven stopLossBreakEven)
        {
            takeProfits = null;
            stopLossBreakEven = null;

            if (!EnablePartialTakeProfit ||
                _plan == null ||
                !IsFinitePositive(executionEntry) ||
                !IsFinitePositive(finalTarget) ||
                !IsFinitePositive(originalVolume) ||
                originalVolume < Symbol.VolumeInUnitsMin)
                return false;

            if ((int)EffectiveAutoTpStage() < (int)TargetStage.TP3)
                return false;

            double tp1 =
                _plan.Tp1;

            double tp2 =
                _plan.Tp2;

            if (!IsFinitePositive(tp1) ||
                !IsFinitePositive(tp2))
                return false;

            double directionSign =
                _plan.Direction == 1
                    ? 1
                    : -1;

            double d1 =
                directionSign *
                (tp1 - executionEntry);

            double d2 =
                directionSign *
                (tp2 - executionEntry);

            double dFinal =
                directionSign *
                (finalTarget - executionEntry);

            double stopDistance =
                directionSign *
                (_plan.Stop - executionEntry);

            // Server protection is accepted only when the structural SL is on the
            // protective side of the actual execution price and the TP ladder is
            // strictly progressive. This prevents stale/misaligned plan geometry
            // from becoming broker-owned protection.
            if (stopDistance >= -Symbol.PipSize ||
                d1 <= Symbol.PipSize ||
                d2 <= d1 ||
                dFinal <= d2)
                return false;

            double tp1Volume =
                Symbol.NormalizeVolumeInUnits(
                    originalVolume *
                    PartialCloseTp1Percent /
                    100.0,
                    RoundingMode.Down);

            double tp2Volume =
                Symbol.NormalizeVolumeInUnits(
                    originalVolume *
                    PartialCloseTp2Percent /
                    100.0,
                    RoundingMode.Down);

            if (tp1Volume < Symbol.VolumeInUnitsMin ||
                tp2Volume < Symbol.VolumeInUnitsMin)
                return false;

            double partialTotal =
                tp1Volume +
                tp2Volume;

            double remaining =
                originalVolume -
                partialTotal;

            if (remaining < Symbol.VolumeInUnitsMin)
                return false;

            double atr =
                _m5Frame != null && _m5Frame.Atr > 0
                    ? _m5Frame.Atr
                    : Symbol.PipSize * 10;

            double minimumSpacingPrice =
                Math.Max(
                    Symbol.PipSize * 2,
                    atr *
                    Math.Max(
                        0.05,
                        MinimumTpSpacingAtr));

            if (d2 - d1 < minimumSpacingPrice ||
                dFinal - d2 < minimumSpacingPrice)
                return false;

            double riskPips =
                Math.Abs(
                    executionEntry -
                    _plan.Stop) /
                Math.Max(
                    Symbol.PipSize,
                    1e-9);

            double tp1Pips =
                d1 /
                Symbol.PipSize;

            double spreadPips =
                Math.Max(
                    0,
                    (Symbol.Ask - Symbol.Bid) /
                    Math.Max(
                        Symbol.PipSize,
                        1e-9));

            SmartBreakEvenResult smartBreakEven =
                SmartBreakEvenRule.Evaluate(
                    riskPips,
                    tp1Pips,
                    spreadPips,
                    BreakEvenTriggerRR,
                    BreakEvenBufferPips,
                    RiskFreeLockPips,
                    UseSpreadAwareBreakEven);

            try
            {
                takeProfits =
                    new RelativeTakeProfitProtections(
                        new RelativeTakeProfitProtection(
                            tp1Volume,
                            d1 / Symbol.PipSize),
                        new RelativeTakeProfitProtection(
                            tp2Volume,
                            d2 / Symbol.PipSize),
                        new RelativeTakeProfitLastProtection(
                            dFinal /
                            Symbol.PipSize));

                if (smartBreakEven.Allowed &&
                    MoveSlToBreakEven)
                {
                    stopLossBreakEven =
                        new StopLossBreakEven(
                            smartBreakEven.TriggerPips,
                            smartBreakEven.OffsetPips);
                }

                return takeProfits != null;
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP server TP ladder build failed: {0}",
                    ex.Message);
                takeProfits = null;
                stopLossBreakEven = null;
                return false;
            }
        }

        private bool AdoptServerSideTakeProfitLadder(
            Position position)
        {
            _serverSideTakeProfitLadderActive =
                false;
            _serverSideBreakEvenActive =
                false;

            if (position == null)
                return false;

            try
            {
                AbsoluteTakeProfitProtections protections =
                    position.AbsoluteTakeProfitProtections;

                _serverSideTakeProfitLadderActive =
                    protections != null &&
                    protections.FirstTakeProfit != null &&
                    protections.SecondTakeProfit != null &&
                    protections.LastTakeProfit != null;

                _serverSideBreakEvenActive =
                    _serverSideTakeProfitLadderActive &&
                    position.StopLossBreakEven != null;

                if (_serverSideTakeProfitLadderActive)
                {
                    double market =
                        position.TradeType == TradeType.Buy
                            ? Symbol.Bid
                            : Symbol.Ask;

                    double forwardDistance =
                        Math.Max(
                            Symbol.PipSize,
                            Symbol.TickSize);

                    double planTarget =
                        FurthestForwardPlanTarget(
                            market,
                            forwardDistance);

                    if (IsFinitePositive(planTarget))
                        _activeBrokerTarget =
                            NormalizePrice(planTarget);
                }

                return _serverSideTakeProfitLadderActive;
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP server TP ladder inspection failed: {0}",
                    ex.Message);
                return false;
            }
        }


        private void ObserveServerSidePartialTakeProfits(
            Position position,
            int closedM5,
            double market)
        {
            if (!_serverSideTakeProfitLadderActive ||
                position == null ||
                _plan == null ||
                _plan.OriginalVolume <= 0)
                return;

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
                tp2Volume < Symbol.VolumeInUnitsMin)
                return;

            double afterTp1 =
                Math.Max(
                    0,
                    _plan.OriginalVolume -
                    tp1Volume);

            double afterTp2 =
                Math.Max(
                    0,
                    afterTp1 -
                    tp2Volume);

            double tolerance =
                Math.Max(
                    Symbol.VolumeInUnitsStep,
                    Math.Max(
                        Symbol.VolumeInUnitsMin,
                        _plan.OriginalVolume * 0.001));

            if (_tp1Hit == 0 &&
                position.VolumeInUnits <=
                afterTp1 + tolerance)
            {
                _tp1Hit = 1;

                if (UpdateUnhitTargets)
                    UpdateUnhitTargetsLive(
                        closedM5,
                        market,
                        true);

                TryAdvanceServerSideTakeProfitLadderAfterTp1(
                    position,
                    closedM5,
                    market);

                if (EnableLevelHitAlerts &&
                    AlertOnLevelHit &&
                    AlertOnTp1)
                {
                    SendUnifiedAlert(
                        "TP1|" +
                        _plan.CreatedM5 +
                        "|SERVER",
                        "CFIP TP1 HIT • SERVER CONFIRMED | " +
                        Price(_plan.Tp1),
                        _plan.Direction,
                        true);
                }
            }

            if (_tp2Hit == 0 &&
                position.VolumeInUnits <=
                afterTp2 + tolerance)
            {
                _tp2Hit = 1;

                TryCollapseServerSideTakeProfitLadderToFinal(
                    position);

                if (EnableLevelHitAlerts &&
                    AlertOnLevelHit &&
                    AlertOnTp2)
                {
                    SendUnifiedAlert(
                        "TP2|" +
                        _plan.CreatedM5 +
                        "|SERVER",
                        "CFIP TP2 HIT • SERVER CONFIRMED | " +
                        Price(_plan.Tp2),
                        _plan.Direction,
                        true);
                }
            }
        }
    }
}
