using System;
using System.Globalization;
using cAlgo.API;
using CFIP.Contracts;

namespace cAlgo
{
    public partial class CFIPIndicator
    {
        private CFIP.Contracts.ExecutionIntent
            BuildCanonicalExecutionIntent(
                string signalId,
                string scenarioId,
                string planId,
                OpportunityLane lane,
                int direction,
                DateTime createdUtc,
                DateTime observedUtc,
                int closedM5)
        {
            if (_cfipProviderExecutionIntent == null ||
                _cfipProviderExecutionIntentM5 != closedM5)
                return null;

            ExecutionAction action =
                ResolveContractExecutionAction(
                    _cfipProviderExecutionIntent);

            if (action == ExecutionAction.None)
                return null;

            DateTime? expiryUtc =
                action == ExecutionAction.PendingStop ||
                action == ExecutionAction.PendingLimit
                    ? createdUtc.AddMinutes(
                        Math.Max(
                            15,
                            PendingOrderExpiryMinutes))
                    : (DateTime?)null;

            ContractIdentity identity =
                new ContractIdentity(
                    ContractVersion.Current,
                    signalId,
                    scenarioId,
                    planId,
                    SymbolName ?? "",
                    ResolveContractDirection(direction),
                    ResolveContractLane(lane),
                    Bars == null
                        ? "UNKNOWN"
                        : Bars.TimeFrame.ToString(),
                    createdUtc,
                    closedM5,
                    expiryUtc,
                    Math.Max(
                        1,
                        _cfipProviderRevision),
                    signalId,
                    "");

            string executionLabel =
                action == ExecutionAction.Market ||
                action == ExecutionAction.Aggressive
                    ? ManagedExecutionLabel()
                    : "";

            MarketExecutionProfile marketProfile =
                BuildCanonicalMarketExecutionProfile(
                    _cfipProviderExecutionIntent);

            return new CFIP.Contracts.ExecutionIntent(
                identity,
                action,
                _cfipProviderExecutionIntent.RequestedEntry,
                _cfipProviderExecutionIntent.Stop,
                _cfipProviderExecutionIntent.Target,
                _cfipProviderExecutionIntent.Volume > 0
                    ? _cfipProviderExecutionIntent.Volume
                    : (double?)null,
                SizingMode.ToString(),
                observedUtc,
                expiryUtc,
                _cfipProviderExecutionIntent.Source ?? "",
                executionLabel,
                marketProfile);
        }

        private MarketExecutionProfile BuildCanonicalMarketExecutionProfile(
            cAlgo.ExecutionIntent intent)
        {
            if (intent == null)
                return null;

            bool marketAction =
                intent.Kind == ExecutionIntentKind.Market;

            if (!marketAction)
                return null;

            bool hasRange =
                !double.IsNaN(intent.MarketRangePips) &&
                !double.IsInfinity(intent.MarketRangePips) &&
                intent.MarketRangePips >= 0;

            bool hasLadder =
                intent.UseServerTakeProfitLadder &&
                IsFinitePositive(intent.Tp1Pips) &&
                IsFinitePositive(intent.Tp2Pips) &&
                IsFinitePositive(intent.FinalTpPips) &&
                IsFinitePositive(intent.Tp1Volume) &&
                IsFinitePositive(intent.Tp2Volume);

            if (!hasRange &&
                !hasLadder)
                return null;

            return new MarketExecutionProfile(
                intent.MarketRangePips,
                intent.StopPips,
                intent.TargetPips,
                hasLadder,
                hasLadder ? intent.Tp1Pips : 0,
                hasLadder ? intent.Tp1Volume : 0,
                hasLadder ? intent.Tp2Pips : 0,
                hasLadder ? intent.Tp2Volume : 0,
                hasLadder ? intent.FinalTpPips : 0,
                hasLadder
                    ? intent.BreakEvenTriggerPips
                    : null,
                hasLadder
                    ? intent.BreakEvenOffsetPips
                    : null);
        }

        private PlanSnapshot BuildCanonicalPlanSnapshot(
            int direction,
            CFIP.Contracts.ExecutionIntent canonicalIntent)
        {
            if (_plan != null &&
                (direction == 0 ||
                 _plan.Direction == direction))
            {
                return new PlanSnapshot(
                    _plan.Entry,
                    _plan.IdealEntry,
                    _plan.EntryZoneLow,
                    _plan.EntryZoneHigh,
                    _plan.EntryTrigger,
                    _plan.EntryInvalidation,
                    _plan.Stop,
                    _plan.Tp1,
                    _plan.Tp2,
                    _plan.Tp3,
                    _plan.Tp4,
                    _plan.Risk,
                    _plan.Tp1RR,
                    _plan.Tp2RR,
                    _plan.Tp3RR,
                    _plan.Tp4RR,
                    _plan.EntryQuality,
                    ResolvePlanQuality(),
                    _plan.StopQuality,
                    _plan.Tp1Quality,
                    _plan.Tp2Quality,
                    _plan.Tp3Quality,
                    _plan.Tp4Quality,
                    _plan.HtfTargetCount,
                    _plan.EntrySource ?? "",
                    _plan.StopSource ?? "",
                    _plan.Tp1Source ?? "",
                    _plan.Tp2Source ?? "",
                    _plan.Tp3Source ?? "",
                    _plan.Tp4Source ?? "",
                    ResolveProviderAnalyticalReason());
            }

            if (_setupPreview != null &&
                (direction == 0 ||
                 _setupPreview.Direction == direction))
            {
                return new PlanSnapshot(
                    _setupPreview.Entry,
                    _setupPreview.IdealEntry,
                    _setupPreview.ZoneLow,
                    _setupPreview.ZoneHigh,
                    _setupPreview.Trigger,
                    _setupPreview.Invalidation,
                    _setupPreview.Stop,
                    _setupPreview.Tp1,
                    _setupPreview.Tp2,
                    _setupPreview.Tp3,
                    _setupPreview.Tp4,
                    _setupPreview.Risk,
                    0,
                    0,
                    0,
                    0,
                    _setupPreview.Direction == 0
                        ? 0
                        : (_decision == null
                            ? 0
                            : _decision.EntryLocationQuality),
                    ResolvePlanQuality(),
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    _executionModel == null
                        ? ""
                        : _executionModel.Source ?? "",
                    "",
                    "",
                    "",
                    "",
                    "",
                    ResolveProviderAnalyticalReason());
            }

            if (canonicalIntent != null)
            {
                double risk =
                    Math.Abs(
                        canonicalIntent.RequestedEntry -
                        canonicalIntent.Stop);

                return new PlanSnapshot(
                    canonicalIntent.RequestedEntry,
                    canonicalIntent.RequestedEntry,
                    0,
                    0,
                    canonicalIntent.RequestedEntry,
                    0,
                    canonicalIntent.Stop,
                    canonicalIntent.InitialTarget,
                    0,
                    0,
                    0,
                    risk,
                    0,
                    0,
                    0,
                    0,
                    _decision == null
                        ? 0
                        : _decision.EntryLocationQuality,
                    ResolvePlanQuality(),
                    0,
                    _decision == null
                        ? 0
                        : _decision.ActionableTp1RR > 0
                            ? 100
                            : 0,
                    0,
                    0,
                    0,
                    0,
                    "EXECUTION INTENT",
                    "EXECUTION INTENT",
                    "EXECUTION INTENT",
                    "",
                    "",
                    "",
                    ResolveProviderAnalyticalReason());
            }

            if (_executionModel != null &&
                (direction == 0 ||
                 _executionModel.Direction == direction))
            {
                return new PlanSnapshot(
                    _executionModel.ActualEntry,
                    _executionModel.IdealEntry,
                    _executionModel.ZoneLow,
                    _executionModel.ZoneHigh,
                    _executionModel.Trigger,
                    _executionModel.Invalidation,
                    _setupPreview == null
                        ? 0
                        : _setupPreview.Stop,
                    _setupPreview == null
                        ? 0
                        : _setupPreview.Tp1,
                    _setupPreview == null
                        ? 0
                        : _setupPreview.Tp2,
                    _setupPreview == null
                        ? 0
                        : _setupPreview.Tp3,
                    _setupPreview == null
                        ? 0
                        : _setupPreview.Tp4,
                    _setupPreview == null
                        ? 0
                        : _setupPreview.Risk,
                    0,
                    0,
                    0,
                    0,
                    _executionModel.Quality,
                    ResolvePlanQuality(),
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    _executionModel.Source ?? "",
                    "",
                    "",
                    "",
                    "",
                    "",
                    ResolveProviderAnalyticalReason());
            }

            return null;
        }


    }
}
