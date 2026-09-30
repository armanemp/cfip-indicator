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
     internal readonly struct OutcomeRegistrationResult
     {
         public bool Recorded { get; }
         public bool Profitable { get; }
         public double NetProfit { get; }
         public double RealizedR { get; }
         public int HistoricalTradeCount { get; }

         public OutcomeRegistrationResult(
             bool recorded,
             bool profitable,
             double netProfit,
             double realizedR,
             int historicalTradeCount)
         {
             Recorded = recorded;
             Profitable = profitable;
             NetProfit = netProfit;
             RealizedR = realizedR;
             HistoricalTradeCount =
                 Math.Max(
                     0,
                     historicalTradeCount);
         }

         public static OutcomeRegistrationResult NotRecorded
         {
             get
             {
                 return new OutcomeRegistrationResult(
                     false,
                     false,
                     0,
                     0,
                     0);
             }
         }
     }

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
                             bool win)
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
 
                             if (win)
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
                                 profitable);

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

         private HistoricalOutcomeAggregate AggregateHistoricalOutcome(
                             Position position)
                         {
                             if (position == null ||
                                 position.Id <= 0 ||
                                 position.Id > int.MaxValue)
                                 return
                                     HistoricalOutcomeAggregationRule
                                         .FromPositionFallback(
                                             position == null
                                                 ? 0
                                                 : position.NetProfit,
                                             position == null
                                                 ? 0
                                                 : position.Pips);

                             try
                             {
                                 HistoricalTrade[] historicalTrades =
                                     History.FindByPositionId(
                                         (int)position.Id);

                                 if (historicalTrades == null ||
                                     historicalTrades.Length == 0)
                                     return
                                         HistoricalOutcomeAggregationRule
                                             .FromPositionFallback(
                                                 position.NetProfit,
                                                 position.Pips);

                                 List<HistoricalOutcomeRecord> records =
                                     new List<HistoricalOutcomeRecord>(
                                         historicalTrades.Length);

                                 for (int i = 0;
                                      i < historicalTrades.Length;
                                      i++)
                                 {
                                     HistoricalTrade trade =
                                         historicalTrades[i];

                                     if (trade == null)
                                         continue;

                                     records.Add(
                                         new HistoricalOutcomeRecord
                                         {
                                             NetProfit =
                                                 trade.NetProfit,
                                             GrossProfit =
                                                 trade.GrossProfit,
                                             Swap =
                                                 trade.Swap,
                                             Commissions =
                                                 trade.Commissions,
                                             Pips =
                                                 trade.Pips,
                                             ClosingTime =
                                                 trade.ClosingTime
                                         });
                                 }

                                 HistoricalOutcomeAggregate aggregate =
                                     HistoricalOutcomeAggregationRule.Aggregate(
                                         records);

                                 return aggregate.Available
                                     ? aggregate
                                     : HistoricalOutcomeAggregationRule
                                         .FromPositionFallback(
                                             position.NetProfit,
                                             position.Pips);
                             }
                             catch (Exception ex)
                             {
                                 Print(
                                     "CFIP historical outcome aggregation failed for #{0}: {1}",
                                     position.Id,
                                     ex.Message);

                                 return
                                     HistoricalOutcomeAggregationRule
                                         .FromPositionFallback(
                                             position.NetProfit,
                                             position.Pips);
                             }
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
 
         private string OutcomeHistoryPanelText()
                         {
                             if (!EnableOutcomeTelemetry ||
                                 _outcomeHistory == null ||
                                 _outcomeHistory.Count == 0)
                                 return "HIST 0";
 
                             int start =
                                 Math.Max(
                                     0,
                                     _outcomeHistory.Count -
                                     PanelOutcomeWindow);
 
                             int count = 0;
                             int wins = 0;
                             double totalR = 0;
                             int totalLifecycle = 0;
 
                             for (int i = start;
                                  i < _outcomeHistory.Count;
                                  i++)
                             {
                                 OutcomeObservation observation =
                                     _outcomeHistory[i];
 
                                 if (observation == null)
                                     continue;
 
                                 count++;
                                 if (observation.Profitable)
                                     wins++;
 
                                 totalR += observation.RealizedR;
                                 totalLifecycle +=
                                     observation.LifecycleBars;
                             }
 
                             if (count == 0)
                                 return "HIST 0";
 
                             double winRate =
                                 100.0 * wins / count;
 
                             return
                                 "HIST " +
                                 count +
                                 "  •  WR " +
                                 winRate.ToString("F0") +
                                 "%  •  AVG R " +
                                 (totalR / count).ToString("F2") +
                                 "  •  LIFE " +
                                 ((double)totalLifecycle / count).ToString("F1") +
                                 " M5";
                         }
 
         private string ExecutionTelemetryPanelText()
                         {
                             if (!EnableOutcomeTelemetry ||
                                 _executionTelemetryHistory == null ||
                                 _executionTelemetryHistory.Count == 0)
                                 return "BROKER TRACE 0";
 
                             int start =
                                 Math.Max(
                                     0,
                                     _executionTelemetryHistory.Count -
                                     PanelExecutionWindow);
 
                             int count = 0;
                             int rejected = 0;
                             int recovery = 0;
                             ExecutionTelemetryRecord latest = null;
 
                             for (int i = start;
                                  i < _executionTelemetryHistory.Count;
                                  i++)
                             {
                                 ExecutionTelemetryRecord record =
                                     _executionTelemetryHistory[i];
 
                                 if (record == null)
                                     continue;
 
                                 count++;
                                 latest = record;
 
                                 if (string.Equals(
                                         record.State,
                                         "REJECTED",
                                         StringComparison.OrdinalIgnoreCase) ||
                                     string.Equals(
                                         record.State,
                                         "FAILED",
                                         StringComparison.OrdinalIgnoreCase) ||
                                     string.Equals(
                                         record.State,
                                         "NULL RESULT",
                                         StringComparison.OrdinalIgnoreCase))
                                     rejected++;
 
                                 if (string.Equals(
                                         record.Path,
                                         "RECOVERY",
                                         StringComparison.OrdinalIgnoreCase))
                                     recovery++;
                             }
 
                             if (latest == null)
                                 return "BROKER TRACE 0";
 
                             return
                                 "BROKER TRACE " +
                                 count +
                                 "  •  LAST " +
                                 latest.State +
                                 " / " +
                                 latest.Path +
                                 "  •  ERR " +
                                 rejected +
                                 "  •  REC " +
                                 recovery;
                         }
 
         private void MonitorOutcome(
                             int closedM5)
                         {
                             if (!EnableOutcomeTelemetry ||
                                 _plan == null ||
                                 !_plan.IsLivePosition ||
                                 OutcomeMaximumM5Bars <= 0)
                                 return;
 
                             if (_outcomeTelemetryTimedOut ||
                                 closedM5 -
                                 _plan.CreatedM5 <
                                 OutcomeMaximumM5Bars)
                                 return;
 
                             // Telemetry timeout is an observation boundary, not a position
                             // lifecycle boundary. Keep broker ownership and live protection
                             // active until the position is actually closed.
                             _outcomeTelemetryTimedOut = true;
 
                             SendUnifiedAlert(
                                 "OUTCOME-TIMEOUT|" +
                                 _plan.PositionId,
                                 "CFIP OUTCOME WINDOW ELAPSED | POSITION #" +
                                 _plan.PositionId +
                                 " remains under live management",
                                 _plan.Direction,
                                 false);
                         }}
 }
 