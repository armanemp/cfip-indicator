using System;
using System.Globalization;
using cAlgo.API;
using CFIP.Contracts;

namespace cAlgo
{
    public partial class CFIPIndicator
    {
        private const int ProviderHeartbeatPublishIntervalSeconds = 1;
        private DateTime _lastProviderHeartbeatBusPublishUtc = DateTime.MinValue;

        private int ResolvePlanQuality()
        {
            if (_decision == null)
                return 0;

            return Math.Max(
                0,
                Math.Min(
                    100,
                    _decision.SmartQuality));
        }

        private string ResolveProviderAnalyticalReason()
        {
            if (_decision == null)
                return "NO DECISION";

            if (!string.IsNullOrWhiteSpace(
                    _decision.ActionabilityReason))
                return _decision.ActionabilityReason;

            if (!string.IsNullOrWhiteSpace(
                    _decision.BlockReason))
                return _decision.BlockReason;

            return _decision.Reason ?? "";
        }

        private SignalStage ResolveProviderSignalStage(
            int direction,
            CFIP.Contracts.ExecutionIntent canonicalIntent)
        {
            if (direction == 0)
                return SignalStage.Watch;

            if (_plan != null &&
                _plan.IsLivePosition)
                return SignalStage.Active;

            if (_decision != null &&
                !_decision.EntryAllowed)
                return SignalStage.Blocked;

            if (canonicalIntent != null)
                return SignalStage.Confirmed;

            if (_decision != null &&
                _decision.TriggerReady)
                return SignalStage.Confirmed;

            return
                _decision != null
                    ? SignalStage.Prediction
                    : SignalStage.Unavailable;
        }

        private ExecutionAction ResolveContractExecutionAction(
            cAlgo.ExecutionIntent intent)
        {
            if (intent == null)
                return ExecutionAction.None;

            if (intent.Policy ==
                DecisionPolicyMode.Aggressive)
                return ExecutionAction.Aggressive;

            if (intent.Kind ==
                ExecutionIntentKind.Stop)
                return ExecutionAction.PendingStop;

            if (intent.Kind ==
                ExecutionIntentKind.Limit)
                return ExecutionAction.PendingLimit;

            if (intent.Kind ==
                ExecutionIntentKind.Market)
                return ExecutionAction.Market;

            return ExecutionAction.None;
        }

        private TradeDirection ResolveContractDirection(
            int direction)
        {
            if (direction > 0)
                return TradeDirection.Buy;

            if (direction < 0)
                return TradeDirection.Sell;

            return TradeDirection.None;
        }

        private CFIP.Contracts.OpportunityLane
            ResolveContractLane(OpportunityLane lane)
        {
            switch (lane)
            {
                case OpportunityLane.Strategic:
                    return CFIP.Contracts.OpportunityLane.Strategic;

                case OpportunityLane.CounterHtfTactical:
                    return CFIP.Contracts.OpportunityLane.CounterHtfTactical;

                case OpportunityLane.MicroReaction:
                    return CFIP.Contracts.OpportunityLane.MicroReaction;

                default:
                    return CFIP.Contracts.OpportunityLane.Tactical;
            }
        }

        private string BuildProviderIdempotencyKey(
            string signalId,
            string scenarioId,
            string planId,
            CFIP.Contracts.ExecutionIntent intent,
            long revision)
        {
            return
                string.Join(
                    "|",
                    "CFIP-P2",
                    signalId,
                    scenarioId,
                    planId,
                    intent == null
                        ? "NONE"
                        : intent.Action.ToString(),
                    revision.ToString(
                        CultureInfo.InvariantCulture));
        }

        private string ResolveProviderState(
            SignalEnvelope envelope)
        {
            if (envelope == null)
                return
                    _initializationReady
                        ? "NO SNAPSHOT"
                        : "INITIALIZING";

            if (envelope.Stage == SignalStage.Blocked)
                return "BLOCKED";

            if (envelope.Intent != null)
                return "EXECUTION INTENT";

            return
                envelope.Stage.ToString()
                    .ToUpperInvariant();
        }

        private void PublishProviderHeartbeat(
            DateTime now)
        {
            _cfipProviderUpdatedUtc = now;

            if (_lastProviderHeartbeatBusPublishUtc != DateTime.MinValue &&
                (now - _lastProviderHeartbeatBusPublishUtc).TotalSeconds <
                ProviderHeartbeatPublishIntervalSeconds)
                return;

            try
            {
                string payload =
                    _cfipProviderRevision.ToString(
                        CultureInfo.InvariantCulture) +
                    "|" +
                    now.Ticks.ToString(
                        CultureInfo.InvariantCulture);

                LocalStorage.SetString(
                    SignalBusKey.ForHeartbeat(InstanceId),
                    payload,
                    LocalStorageScope.Device);

                LocalStorage.Flush(
                    LocalStorageScope.Device);

                _lastProviderHeartbeatBusPublishUtc = now;
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP PROVIDER HEARTBEAT PUBLISH FAILED | revision={0} | {1}",
                    _cfipProviderRevision,
                    ex.Message);
            }
        }

        private void PublishProviderHeartbeatValue(
            int index)
        {
            if (ProviderHeartbeat != null &&
                index >= 0)
            {
                ProviderHeartbeat[index] =
                    _cfipProviderRevision;
            }
        }
    }
}
