using System;
using cAlgo.API;
using cAlgo.API.Internals;

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

            double market =
                _plan.Direction == 1
                    ? Symbol.Bid
                    : Symbol.Ask;

            double atr =
                _m5Frame != null && _m5Frame.Atr > 0
                    ? _m5Frame.Atr
                    : Symbol.PipSize * 10;

            double minimumForwardDistance =
                MinimumLiveTargetDistancePrice(
                    _plan.Direction,
                    atr);

            if (!IsLiveTargetBrokerSafe(
                    _plan.Direction,
                    executionEntry,
                    market,
                    tp1,
                    atr) ||
                !IsLiveTargetBrokerSafe(
                    _plan.Direction,
                    executionEntry,
                    market,
                    tp2,
                    atr) ||
                !IsLiveTargetBrokerSafe(
                    _plan.Direction,
                    executionEntry,
                    market,
                    finalTarget,
                    atr))
                return false;

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

            // Server protection is accepted only when the live structural SL is
            // protective against the executable quote and the TP ladder is strictly
            // progressive and still forward of the live market.
            if (!IsValidManagedStop(
                    _plan.Direction,
                    executionEntry,
                    market,
                    _plan.Stop) ||
                stopDistance >= -Symbol.PipSize ||
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

            double minimumSpacingPrice =
                minimumForwardDistance;

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

            _lastBreakEvenDiagnostic =
                MoveSlToBreakEven
                    ? (smartBreakEven.Allowed
                        ? smartBreakEven.Reason
                        : "NOT APPLICABLE • " +
                          smartBreakEven.Reason)
                    : "DISABLED • MOVE SL TO BREAK EVEN";

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

                if (_cfipProviderExecutionIntent != null &&
                    _cfipProviderExecutionIntent.CreatedM5 ==
                    _cfipProviderExecutionIntentM5)
                {
                    _cfipProviderExecutionIntent.UseServerTakeProfitLadder =
                        true;
                    _cfipProviderExecutionIntent.Tp1Pips =
                        d1 / Symbol.PipSize;
                    _cfipProviderExecutionIntent.Tp1Volume =
                        tp1Volume;
                    _cfipProviderExecutionIntent.Tp2Pips =
                        d2 / Symbol.PipSize;
                    _cfipProviderExecutionIntent.Tp2Volume =
                        tp2Volume;
                    _cfipProviderExecutionIntent.FinalTpPips =
                        dFinal / Symbol.PipSize;
                    _cfipProviderExecutionIntent.BreakEvenTriggerPips =
                        smartBreakEven.Allowed && MoveSlToBreakEven
                            ? smartBreakEven.TriggerPips
                            : (double?)null;
                    _cfipProviderExecutionIntent.BreakEvenOffsetPips =
                        smartBreakEven.Allowed && MoveSlToBreakEven
                            ? smartBreakEven.OffsetPips
                            : (double?)null;
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

                if (_serverSideTakeProfitLadderActive)
                    _serverSideTakeProfitLadderOwned = true;

                _serverSideBreakEvenActive =
                    _serverSideTakeProfitLadderActive &&
                    position.StopLossBreakEven != null;

                if (_serverSideTakeProfitLadderActive)
                {
                    double market =
                        position.TradeType == TradeType.Buy
                            ? Symbol.Bid
                            : Symbol.Ask;

                    double brokerFinalTarget =
                        protections.LastTakeProfit != null
                            ? protections.LastTakeProfit.Price
                            : 0;

                    if (IsLiveTargetBrokerSafe(
                            position.TradeType == TradeType.Buy
                                ? 1
                                : -1,
                            position.EntryPrice,
                            market,
                            brokerFinalTarget,
                            0))
                    {
                        // The broker's actual last protection is authoritative.
                        // Never reconstruct it from the indicator's structural plan.
                        _activeBrokerTarget =
                            NormalizePrice(brokerFinalTarget);
                    }
                    else
                    {
                        _activeBrokerTarget = 0;
                    }
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

            if (position.Deals == null)
                return;

            int dealCount =
                position.Deals.Count;

            if (dealCount ==
                _lastServerPartialObservationDealCount)
                return;

            _lastServerPartialObservationDealCount =
                dealCount;

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

            double priceTolerance =
                Math.Max(
                    Symbol.PipSize * 2.0,
                    Symbol.TickSize * 5.0);

            double volumeTolerance =
                Math.Max(
                    Symbol.VolumeInUnitsStep,
                    Symbol.VolumeInUnitsMin * 0.5);

            bool tp1Evidence =
                _tp1Hit == 0 &&
                HasServerPartialTakeProfitDealEvidence(
                    position,
                    1,
                    _plan.Tp1,
                    tp1Volume,
                    priceTolerance,
                    volumeTolerance);

            if (tp1Evidence)
            {
                _tp1Hit = 1;

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

            bool tp2Evidence =
                _tp2Hit == 0 &&
                HasServerPartialTakeProfitDealEvidence(
                    position,
                    2,
                    _plan.Tp2,
                    tp2Volume,
                    priceTolerance,
                    volumeTolerance);

            if (tp2Evidence)
            {
                // A confirmed TP2 closing deal proves that the earlier TP1 stage
                // has already occurred even if the local process missed its event.
                _tp1Hit = 1;
                _tp2Hit = 1;

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

        private bool HasServerPartialTakeProfitDealEvidence(
            Position position,
            int stage,
            double expectedPrice,
            double expectedVolume,
            double priceTolerance,
            double volumeTolerance)
        {
            if (position == null ||
                _plan == null ||
                stage < 1 ||
                stage > 2 ||
                !IsFinitePositive(expectedPrice) ||
                !IsFinitePositive(expectedVolume))
                return false;

            int direction =
                position.TradeType == TradeType.Buy
                    ? 1
                    : -1;

            double expectedTarget =
                expectedPrice;

            double expectedRemainingAfterStage =
                _plan.OriginalVolume -
                expectedVolume;

            if (!NumericGuards.IsFinitePositive(
                    expectedRemainingAfterStage) ||
                !NumericGuards.IsFinitePositive(
                    position.VolumeInUnits))
                return false;

            double remainingVolumeTolerance =
                Math.Max(
                    Symbol.VolumeInUnitsStep,
                    Symbol.VolumeInUnitsMin * 0.5);

            if (position.VolumeInUnits >
                expectedRemainingAfterStage +
                remainingVolumeTolerance)
                return false;

            for (int i = 0;
                 i < position.Deals.Count;
                 i++)
            {
                Deal deal =
                    position.Deals[i];

                if (deal == null)
                    continue;

                int dealDirection =
                    deal.TradeType == TradeType.Buy
                        ? 1
                        : -1;

                if (ServerPartialTakeProfitEvidenceRule.IsMatchingClosingDeal(
                        position.Id,
                        direction,
                        deal.PositionId,
                        dealDirection,
                        deal.PositionImpact ==
                            DealPositionImpact.Closing,
                        deal.ExecutionPrice.HasValue
                            ? deal.ExecutionPrice.Value
                            : 0,
                        expectedTarget,
                        deal.VolumeInUnits,
                        expectedVolume,
                        priceTolerance,
                        volumeTolerance))
                    return true;
            }

            return false;
        }
    }
}
