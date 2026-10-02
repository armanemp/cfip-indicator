using cAlgo.API;
namespace cAlgo
{
public partial class CFIPIndicator : Indicator
{
private void ExecuteAggressiveTrade(
int closedM5,
TradeType type,
double entry,
double atr,
double stop,
double target,
double stopPips,
double tpPips,
double volume)
{
try
{
if (!CanRunAutomaticEntry())
{
ApplyRuntimeEntryGate();
return;
}
if (!EnsureTradingPermission())
{
_autoExecutionBlockReason = "AGG PERMISSION";
SetAutoTradingState("BLOCKED", "AGG PERMISSION");
return;
}
string safetyReason;
if (!PassesAutoTradeSafetyGuards(
type,
volume,
out safetyReason))
{
_autoExecutionBlockReason = "AGG SAFETY " + safetyReason;
SetAutoTradingState("BLOCKED", _autoExecutionBlockReason);
return;
}
if (!TryValidateAggressiveFinalExecution(
closedM5,
type,
entry,
atr,
stop,
target,
volume,
out string guardReason))
{
SetAutoTradingState(
"BLOCKED",
guardReason);
return;
}
ExecutionIntent aggressiveIntent =
BuildExecutionIntent(
_reaction.Direction,
DecisionPolicyMode.Aggressive,
ExecutionIntentKind.Market,
entry,
0,
0,
0,
stop,
target,
volume,
closedM5,
"AGG MARKET");
string aggressiveIntentReason;
if (!ValidateExecutionIntent(
aggressiveIntent,
entry,
out aggressiveIntentReason))
{
_autoExecutionBlockReason =
"AGGRESSIVE - " +
aggressiveIntentReason;
SetAutoTradingState(
"BLOCKED",
_autoExecutionBlockReason);
return;
}
string submissionGateReason;
SubmissionAttemptIdentity submissionIdentity;
int aggressiveDirection =
_reaction == null
? 0
: _reaction.Direction;
string executionScenarioId =
ResolveDirectionExecutionScenarioId(
aggressiveDirection);
_activeExecutionScenarioId =
executionScenarioId;
if (!TryAcquireSubmission(
closedM5,
aggressiveDirection,
ExecutionSubmissionPath.AggressiveMarket,
executionScenarioId,
out submissionIdentity,
out submissionGateReason))
{
_autoExecutionBlockReason =
submissionGateReason;
SetAutoTradingState(
"BLOCKED",
submissionGateReason);
return;
}
RelativeTakeProfitProtections serverTakeProfits;
StopLossBreakEven serverBreakEven;
bool useServerTakeProfitLadder =
TryBuildServerSideTakeProfitLadder(
entry,
target,
volume,
out serverTakeProfits,
out serverBreakEven);
TradeResult result;
try
{
result =
useServerTakeProfitLadder
? TryExecuteMarketOrderWithTakeProfitLadder(
type,
SymbolName,
volume,
ManagedExecutionLabel(),
stopPips,
serverTakeProfits,
serverBreakEven,
TradeExecutionMetadata.DefaultExecutionComment,
false,
"AGG - TP LADDER")
: TryExecuteMarketOrder(
type,
SymbolName,
volume,
ManagedExecutionLabel(),
stopPips,
tpPips,
TradeExecutionMetadata.DefaultExecutionComment,
false,
"AGG MARKET");
}
catch
{
RecordSubmissionFailure(
submissionIdentity);
throw;
}
RecordSubmission(
submissionIdentity,
result,
aggressiveIntent);
if (!BrokerConfirmationPolicy.CanAdoptPosition(
result != null,
result != null &&
result.IsSuccessful,
result != null &&
result.Position != null))
{
_autoExecutionBlockReason =
result != null &&
result.Error.HasValue
? "AGGRESSIVE - " +
result.Error.Value.ToString()
: "AGG REJ";
SetAutoTradingState(
"ERROR",
_autoExecutionBlockReason);
return;
}
if (!TryProcessAcceptedAggressiveFill(
closedM5,
entry,
atr,
stop,
target,
aggressiveIntent,
result,
out double actualStop,
out double actualTarget))
return;
AdoptServerSideTakeProfitLadder(
result.Position);
bool protectionOk = true;
if (AutoBrokerProtection)
{
protectionOk =
EnsureBrokerProtectionForPosition(
result.Position,
actualStop,
actualTarget,
"AG ENTRY",
_reaction.Direction);
}
SetAutoTradingState(
protectionOk
? "EXECUTED"
: "RECOVERY",
protectionOk
? "POSITION #" +
result.Position.Id
: "POSITION #" +
result.Position.Id +
" - BROKER PROTECTION RECOVERY");
double confirmedStop =
GetActiveBrokerStopPrice();
double confirmedTarget =
GetActiveBrokerTargetPrice();
SendAggressiveConfirmationAlert(
closedM5,
_reaction.Direction,
result.Position.Id,
result.Position.EntryPrice,
confirmedStop,
confirmedTarget);
}
catch (System.Exception ex)
{
_autoExecutionBlockReason =
"AGG - EXCEPTION - " +
ex.Message;
Print(
"CFIP aggressive trade failed: {0}",
ex.Message);
}
}
}
}
