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
                string sourceTimeframe,
                DateTime createdUtc,
                DateTime observedUtc,
                int closedM5)
        {
            cAlgo.ExecutionIntent sourceIntent =
                _cfipProviderExecutionIntent;

            if (sourceIntent == null ||
                _cfipProviderExecutionIntentM5 != closedM5)
            {
                if (_plan == null ||
                    _decision == null ||
                    !_decision.EntryAllowed ||
                    _plan.CreatedM5 != closedM5 ||
                    direction == 0 ||
                    _plan.Direction != direction)
                    return null;

                double planTarget =
                    IsFinitePositive(_plan.Tp1)
                        ? _plan.Tp1
                        : _plan.Tp4;

                if (!IsFinitePositive(_plan.Entry) ||
                    !IsFinitePositive(_plan.Stop) ||
                    !IsFinitePositive(planTarget) ||
                    !IsFinitePositive(_plan.OriginalVolume))
                    return null;

                sourceIntent =
                    new cAlgo.ExecutionIntent
                    {
                        Direction = direction,
                        Policy = DecisionPolicyMode.Confirmed,
                        Kind = ExecutionIntentKind.Market,
                        RequestedEntry = _plan.Entry,
                        Trigger = _plan.EntryTrigger,
                        ZoneLow = _plan.EntryZoneLow,
                        ZoneHigh = _plan.EntryZoneHigh,
                        Stop = _plan.Stop,
                        Target = planTarget,
                        StopPips =
                            Math.Abs(
                                _plan.Entry -
                                _plan.Stop) /
                            Math.Max(
                                Symbol.PipSize,
                                1e-9),
                        TargetPips =
                            Math.Abs(
                                planTarget -
                                _plan.Entry) /
                            Math.Max(
                                Symbol.PipSize,
                                1e-9),
                        Volume = _plan.OriginalVolume,
                        CreatedM5 = closedM5,
                        Source = "CANONICAL PLAN • CBOT HANDOFF"
                    };
            }

            ExecutionAction action =
                ResolveContractExecutionAction(
                    sourceIntent);

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
                    ProviderScenarioIdentityRule.ResolveSourceTimeframe(
                        null,
                        sourceTimeframe),
                    createdUtc,
                    closedM5,
                    expiryUtc,
                    Math.Max(
                        1,
                        _cfipProviderRevision),
                    signalId,
                    "");

            return new CFIP.Contracts.ExecutionIntent(
                identity,
                action,
                sourceIntent.RequestedEntry,
                sourceIntent.Stop,
                sourceIntent.Target,
                sourceIntent.Volume > 0
                    ? sourceIntent.Volume
                    : (double?)null,
                SizingMode.ToString(),
                observedUtc,
                expiryUtc,
                sourceIntent.Source ?? "");
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
