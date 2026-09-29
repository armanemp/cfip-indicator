using System;
using cAlgo.API;
namespace cAlgo
{
  public partial class CFIPIndicator : Indicator
  {
      private bool PlaceContinuationStop(
          int closedM5)
      {
          if (!CanRunAutomaticEntry())
          {
              ApplyRuntimeEntryGate();
              return false;
          }
          if (!TryPrepareContinuationStop(closedM5, out int direction, out _, out double trigger, out double stop, out double target, out _, out _, out double volume, out ExecutionIntent pendingIntent))
              return false;
          string reason;
          if (!ValidatePendingSubmission(
                  pendingIntent,
                  direction == 1
                      ? TradeType.Buy
                      : TradeType.Sell,
                  direction == 1
                      ? Symbol.Ask
                      : Symbol.Bid,
                  volume,
                  "PENDING STOP • ",
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
                      ExecutionSubmissionPath.PendingStop,
                      out submissionIdentity,
                      out submissionGateReason))
              {
                  _autoOrdersBlockReason = submissionGateReason;
                  return false;
              }

              RelativeTakeProfitProtections serverTakeProfits;
              bool useServerTakeProfitLadder =
                  TryBuildServerSideTakeProfitLadder(
                      trigger,
                      target,
                      volume,
                      out serverTakeProfits);

              TradeResult result;
              try
              {
                  result =
                      useServerTakeProfitLadder
                          ? TryPlaceStopOrderWithTakeProfitLadder(
                              direction == 1
                                  ? TradeType.Buy
                                  : TradeType.Sell,
                              SymbolName,
                              volume,
                              trigger,
                              PendingOrderLabel(),
                              pendingIntent.StopPips,
                              serverTakeProfits,
                              ProtectionType.Relative,
                              PendingExpiration(),
                              TradeExecutionMetadata.DefaultExecutionComment,
                              false,
                              "CONTINUATION STOP • SERVER TP LADDER")
                          : TryPlaceStopOrder(
                              direction == 1
                                  ? TradeType.Buy
                                  : TradeType.Sell,
                              SymbolName,
                              volume,
                              trigger,
                              PendingOrderLabel(),
                              pendingIntent.StopPips,
                              pendingIntent.TargetPips,
                              ProtectionType.Relative,
                              PendingExpiration(),
                              TradeExecutionMetadata.DefaultExecutionComment,
                              false,
                              "CONTINUATION STOP");
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
                          : "PENDING STOP REJECTED";
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
                  "STOP");
              return true;
          }
          catch (Exception ex)
          {
              _autoOrdersBlockReason = "PENDING STOP • " + ex.Message;
              return false;
          }
      }
  }
}
