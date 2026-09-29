using System;
using cAlgo.API;
namespace cAlgo
{
  public partial class CFIPIndicator : Indicator
  {
      private bool PlaceReversalLimit(
          int closedM5)
      {
          if (!CanRunAutomaticEntry())
          {
              ApplyRuntimeEntryGate();
              return false;
          }
          if (!TryPrepareReversalLimit(closedM5, out int direction, out _, out double targetEntry, out double stop, out double target, out _, out _, out double volume, out ExecutionIntent pendingIntent))
              return false;
          string reason;
          if (!ValidatePendingSubmission(
                  pendingIntent,
                  direction == 1
                      ? TradeType.Buy
                      : TradeType.Sell,
                  direction == 1
                      ? Symbol.Bid
                      : Symbol.Ask,
                  volume,
                  "PENDING LIMIT • ",
                  out reason))
          {
              _autoOrdersBlockReason = reason;
              return false;
          }
          try
          {
              string submissionGateReason;
              SubmissionAttemptIdentity submissionIdentity;

              if (!TryAcquireSubmission(
                      closedM5,
                      direction,
                      ExecutionSubmissionPath.PendingLimit,
                      out submissionIdentity,
                      out submissionGateReason))
              {
                  _autoOrdersBlockReason = submissionGateReason;
                  return false;
              }

              RelativeTakeProfitProtections serverTakeProfits;
              bool useServerTakeProfitLadder =
                  TryBuildServerSideTakeProfitLadder(
                      targetEntry,
                      target,
                      volume,
                      out serverTakeProfits);

              TradeResult result;
              try
              {
                  result =
                      useServerTakeProfitLadder
                          ? TryPlaceLimitOrderWithTakeProfitLadder(
                              direction == 1
                                  ? TradeType.Buy
                                  : TradeType.Sell,
                              SymbolName,
                              volume,
                              targetEntry,
                              PendingOrderLabel(),
                              pendingIntent.StopPips,
                              serverTakeProfits,
                              ProtectionType.Relative,
                              PendingExpiration(),
                              TradeExecutionMetadata.DefaultExecutionComment,
                              false,
                              "REVERSAL LIMIT • SERVER TP LADDER")
                          : TryPlaceLimitOrder(
                              direction == 1
                                  ? TradeType.Buy
                                  : TradeType.Sell,
                              SymbolName,
                              volume,
                              targetEntry,
                              PendingOrderLabel(),
                              pendingIntent.StopPips,
                              pendingIntent.TargetPips,
                              ProtectionType.Relative,
                              PendingExpiration(),
                              TradeExecutionMetadata.DefaultExecutionComment,
                              false,
                              "REVERSAL LIMIT");
              }
              catch
              {
                  RecordSubmissionFailure(
                      submissionIdentity);
                  throw;
              }
              RecordSubmission(
                  submissionIdentity,
                  result);
              if (!BrokerConfirmationPolicy.CanAdoptPendingOrder(result != null, result?.IsSuccessful == true, result?.PendingOrder != null))
              {
                  _autoOrdersBlockReason =
                      result != null &&
                      result.Error.HasValue
                          ? result.Error.Value.ToString()
                          : "PENDING LIMIT REJECTED";
                  return false;
              }
              _lastPendingSignalM5 = closedM5;
              _plan = null;
              _executionModel = null;
              RemovePlanObjects();
              ReportConfirmedPendingOrderPlacement(
                  result.PendingOrder,
                  direction,
                  closedM5,
                  "LIMIT");
              return true;
          }
          catch (Exception ex)
          {
              _autoOrdersBlockReason = "PENDING LIMIT • " + ex.Message;
              return false;
          }
      }
  }
}
