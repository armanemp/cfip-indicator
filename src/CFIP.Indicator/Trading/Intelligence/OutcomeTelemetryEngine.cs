// ============================================================================
 // CFIP Indicator — OutcomeTelemetryEngine.cs
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
         private const int MaxOutcomeHistory = 128;
         private const int MaxExecutionTelemetryHistory = 64;
         private const int PanelOutcomeWindow = 24;
         private const int PanelExecutionWindow = 20;
 
         private void RegisterOutcome(
                             int direction,
                             bool win)
                         {
                             if (!EnableOutcomeTelemetry)
                                 return;
 
                             if (!_directionSamples.ContainsKey(
                                     direction))
                                 _directionSamples[direction] = 0;
 
                             if (!_directionWins.ContainsKey(
                                     direction))
                                 _directionWins[direction] = 0;
 
                             _directionSamples[direction]++;
 
                             if (win)
                                 _directionWins[direction]++;
                         }
 
         private void RegisterCalibratedOutcome(
                             Plan plan,
                             double realizedR)
                         {
                             if (!EnableOutcomeTelemetry ||
                                 plan == null ||
                                 !plan.CalibrationEligible ||
                                 (plan.CalibrationDirection != 1 &&
                                  plan.CalibrationDirection != -1))
                                 return;
 
                             ConfidenceCalibrationKey key =
                                 new ConfidenceCalibrationKey(
                                     plan.CalibrationDirection,
                                     plan.CalibrationLane,
                                     plan.CalibrationRegime,
                                     EmpiricalConfidenceCalibrator.ConfidenceBucket(
                                         plan.CalibrationConfidence));
 
                             if (!_calibrationSamples.ContainsKey(key))
                                 _calibrationSamples[key] = 0;
 
                             if (!_calibrationWins.ContainsKey(key))
                                 _calibrationWins[key] = 0;
 
                             _calibrationSamples[key]++;
 
                             if (CalibrationOutcomeRule.IsPositiveRealizedR(realizedR))
                                 _calibrationWins[key]++;
                         }
 
         private OutcomeRegistrationResult RecordManagedOutcome(
                             Plan plan,
                             Position position,
                             int closedM5)
                         {
                             if (!EnableOutcomeTelemetry ||
                                 position == null ||
                                 position.Id <= 0 ||
                                 HasRecordedOutcome(position.Id))
                                 return OutcomeRegistrationResult.NotRecorded;

                             int direction =
                                 position.TradeType == TradeType.Buy
                                     ? 1
                                     : -1;

                             HistoricalOutcomeAggregate aggregate =
                                 AggregateHistoricalOutcome(
                                     position);

                             double realizedNetProfit =
                                 aggregate.Available
                                     ? aggregate.NetProfit
                                     : position.NetProfit;

                             double realizedPips =
                                 aggregate.Available
                                     ? aggregate.Pips
                                     : position.Pips;

                             bool profitable =
                                 realizedNetProfit > 0;

                             int createdM5 =
                                 plan == null
                                     ? -1
                                     : plan.CreatedM5;

                             int lifecycleBars =
                                 createdM5 >= 0 &&
                                 closedM5 >= createdM5
                                     ? closedM5 - createdM5
                                     : 0;

                             double riskPips =
                                 plan != null &&
                                 IsFinitePositive(plan.Risk) &&
                                 Symbol.PipSize > 0
                                     ? plan.Risk /
                                       Symbol.PipSize
                                     : 0;

                             double riskVolume =
                                 plan != null &&
                                 IsFinitePositive(
                                     plan.OriginalVolume)
                                     ? plan.OriginalVolume
                                     : 0;

                             double riskAmount =
                                 IsFinitePositive(
                                     riskPips) &&
                                 IsFinitePositive(
                                     riskVolume)
                                     ? Symbol.AmountRisked(
                                         riskVolume,
                                         riskPips)
                                     : 0;

                             double realizedR =
                                 IsFinitePositive(
                                     riskAmount) &&
                                 !double.IsNaN(
                                     realizedNetProfit) &&
                                 !double.IsInfinity(
                                     realizedNetProfit)
                                     ? realizedNetProfit /
                                       riskAmount
                                     : 0;

                             OpportunityLane lane =
                                 plan == null
                                     ? OpportunityLane.Strategic
                                     : plan.CalibrationEligible
                                         ? plan.CalibrationLane
                                         : plan.Lane;

                             string regime =
                                 plan != null &&
                                 !string.IsNullOrWhiteSpace(
                                     plan.CalibrationRegime)
                                     ? plan.CalibrationRegime
                                     : "UNKNOWN";

                             int confidence =
                                 plan != null &&
                                 plan.CalibrationEligible
                                     ? NumericGuards.ClampInt(
                                         plan.CalibrationConfidence,
                                         0,
                                         100)
                                     : 0;

                             OutcomeObservation observation =
                                 new OutcomeObservation
                                 {
                                     PositionId = position.Id,
                                     Direction = direction,
                                     Lane = lane,
                                     EntryMode = plan == null
                                         ? ExecutionMode.None
                                         : plan.EntryMode,
                                     Regime = regime,
                                     Confidence = confidence,
                                     ConfidenceBucket =
                                         EmpiricalConfidenceCalibrator.ConfidenceBucket(
                                             confidence),
                                     CreatedM5 = createdM5,
                                     ClosedM5 = closedM5,
                                     LifecycleBars = lifecycleBars,
                                     Pips = realizedPips,
                                     NetProfit = realizedNetProfit,
                                     RealizedR = realizedR,
                                     Profitable = profitable,
                                     CalibrationEligible =
                                         plan != null &&
                                         plan.CalibrationEligible,
                                     ProtectionRecoveryAtClose =
                                         _brokerProtectionRecoveryRequired,
                                     ServerSideTakeProfitLadderActive =
                                         _serverSideTakeProfitLadderActive,
                                     ObservedUtcTicks =
                                         Server.TimeInUtc.Ticks
                                 };

                             _outcomeHistory.Add(observation);
                             TrimOutcomeHistory();
                             PersistOutcomeHistory();
                             ArchiveOutcomeObservation(observation);
                             RegisterArchiveLearningObservation(observation);

                             RegisterOutcome(
                                 direction,
                                 profitable);

                             RegisterCalibratedOutcome(
                                 plan,
                                 realizedR);

                             ArchiveRuntimeExecution(
                                 "OUTCOME",
                                 closedM5,
                                 "FINALIZED",
                                 "POSITION=" +
                                 position.Id +
                                 " • HISTORY=" +
                                 (aggregate.Available
                                     ? aggregate.TradeCount.ToString(
                                         CultureInfo.InvariantCulture)
                                     : "FALLBACK") +
                                 " • NET=" +
                                 realizedNetProfit.ToString(
                                     "R",
                                     CultureInfo.InvariantCulture) +
                                 " • R=" +
                                 realizedR.ToString(
                                     "F4",
                                     CultureInfo.InvariantCulture));

                             return new OutcomeRegistrationResult(
                                 true,
                                 profitable,
                                 realizedNetProfit,
                                 realizedR,
                                 aggregate.Available
                                     ? aggregate.TradeCount
                                     : 1);
                         }

         private bool HasRecordedOutcome(
                             long positionId)
                         {
                             if (positionId <= 0 ||
                                 _outcomeHistory == null)
                                 return false;
 
                             for (int i = 0;
                                  i < _outcomeHistory.Count;
                                  i++)
                             {
                                 OutcomeObservation item =
                                     _outcomeHistory[i];
 
                                 if (item != null &&
                                     item.PositionId ==
                                     positionId)
                                     return true;
                             }
 
                             return false;
                         }
 
         private void TrimOutcomeHistory()
                         {
                             while (_outcomeHistory.Count >
                                    MaxOutcomeHistory)
                                 _outcomeHistory.RemoveAt(0);
                         }
 
         private bool RecordPlanRewardRejection(
                             int direction,
                             string reason)
                         {
                             if (!EnableOutcomeTelemetry)
                                 return false;

                             string normalizedReason =
                                 string.IsNullOrWhiteSpace(reason)
                                     ? "UNKNOWN"
                                     : reason.Trim();

                             RecordExecutionTelemetryHistory(
                                 "PLAN_REWARD",
                                 Math.Max(-1, _lastEvaluatedM5),
                                 "REJECTED",
                                 "DIR=" +
                                 direction.ToString(CultureInfo.InvariantCulture) +
                                 " • " +
                                 normalizedReason);

                             return false;
                         }

         private void RecordExecutionTelemetryHistory(
                             string path,
                             int m5,
                             string state,
                             string reason)
                         {
                             if (!EnableOutcomeTelemetry)
                                 return;
 
                             _executionTelemetryHistory.Add(
                                 new ExecutionTelemetryRecord
                                 {
                                     Utc = Server.TimeInUtc,
                                     M5 = m5,
                                     Path = string.IsNullOrWhiteSpace(path)
                                         ? "UNKNOWN"
                                         : path,
                                     State = string.IsNullOrWhiteSpace(state)
                                         ? "UNKNOWN"
                                         : state,
                                     Reason = string.IsNullOrWhiteSpace(reason)
                                         ? ""
                                         : reason
                                 });
 
                             while (_executionTelemetryHistory.Count >
                                    MaxExecutionTelemetryHistory)
                                 _executionTelemetryHistory.RemoveAt(0);

                             ArchiveRuntimeExecution(
                                 path,
                                 m5,
                                 state,
                                 reason);
                         }
 
         private void RecordLifecycleTelemetry(
                             LifecycleState state,
                             string reason)
                         {
                             if (!EnableOutcomeTelemetry)
                                 return;
 
                             RecordExecutionTelemetryHistory(
                                 "RECOVERY",
                                 Math.Max(
                                     -1,
                                     _lastEvaluatedM5),
                                 state == LifecycleState.RecoveryRequired
                                     ? "REQUIRED"
                                     : "RESOLVED",
                                 reason);
                         }
 
         private string CalibrationText()
                         {
                             int total =
                                 _wins +
                                 _losses;
 
                             return
                                 total <= 0
                                     ? "W0/L0"
                                     : (100.0 *
                                        _wins /
                                        total)
                                       .ToString("F0") +
                                       "%";
                         }
 
    }
 }
 