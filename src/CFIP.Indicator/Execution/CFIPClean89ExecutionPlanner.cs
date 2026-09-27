// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public sealed class CFIPClean89ExecutionPlanner : ICFIPClean89ExecutionPlanner
        {
            public CFIPClean89ExecutionIntent CreateIntent(
                CFIPClean89TradePlan plan,
                CFIPClean89EntrySnapshot entry,
                CFIPClean89RuntimeSnapshot runtime,
                CFIPClean89ConfigSnapshot configuration,
                CFIPClean89ExecutionReadiness readiness)
            {
                if (plan == null || entry == null || runtime == null ||
                    configuration == null || readiness == null || !readiness.Eligible)
                    throw new ArgumentException(
                        "Execution intent requires eligible execution state.");
    
                if (entry.Mode != plan.EntryMode ||
                    entry.Direction != plan.Direction ||
                    readiness.Kind == CFIPClean89ExecutionKind.None)
                    throw new ArgumentException(
                        "Execution intent must match the authoritative TradePlan.");
    
                var requested =
                    new CFIPClean89PriceLevel(
                        readiness.RequestedPrice,
                        "EXECUTION_REQUESTED_ENTRY",
                        CFIPClean89Provenance.Direct(
                            "EXECUTION",
                            readiness.Kind.ToString().ToUpperInvariant()));
    
                CFIPClean89TargetStage requestedStage =
                    configuration.Get(
                        "AutoTpStage",
                        CFIPClean89TargetStage.TP1);
    
                CFIPClean89TargetLevel target =
                    plan.TargetLadder.Find(requestedStage);
    
                if (target == null)
                    throw new ArgumentException(
                        "Requested execution target stage is unavailable.",
                        "AutoTpStage");
    
                string seed =
                    plan.Identity.PlanId +
                    "|" +
                    readiness.Kind.ToString();
    
                return new CFIPClean89ExecutionIntent(
                    new CFIPClean89ExecutionIdentity(
                        seed,
                        "CFIP89|EXEC|" + seed,
                        runtime.ServerUtc),
                    plan.Identity,
                    plan.Direction,
                    readiness.Kind,
                    entry.Mode,
                    requested,
                    entry.Model.Trigger,
                    plan.StructuralStop,
                    target.Level,
                    readiness.VolumeInUnits,
                    new CFIPClean89RiskRequest(
                        configuration.Get("RiskPercentEquity", 0.50),
                        readiness.VolumeInUnits,
                        readiness.RiskAmount),
                    entry.Decision.PolicyMode,
                    runtime.ServerUtc,
                    readiness.Kind == CFIPClean89ExecutionKind.Market
                        ? null
                        : runtime.ServerUtc +
                          TimeSpan.FromMinutes(
                              Math.Max(
                                  15,
                                  configuration.Get(
                                      "PendingOrderExpiryMinutes",
                                      120))));
            }
        }
}
