// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public sealed class CFIPClean89TradePlanBuilder :
        ICFIPClean89TradePlanBuilder
    {
        private sealed class StopCandidate
        {
            public double Price;
            public int Quality;
            public double Distance;
            public bool Structural;
            public CFIPClean89Provenance Provenance;
        }
    
        private sealed class TargetCandidate
        {
            public double Price;
            public int Quality;
            public bool Htf;
            public CFIPClean89Provenance Provenance;
        }
    
        public CFIPClean89TradePlan Build(
            CFIPClean89DecisionSnapshot decision,
            CFIPClean89EntrySnapshot entry,
            CFIPClean89MarketModel market,
            CFIPClean89StructureSnapshot structure,
            CFIPClean89MtfSnapshot mtf,
            CFIPClean89RuntimeSnapshot runtime,
            CFIPClean89ConfigSnapshot configuration)
        {
            if (decision == null)
                throw new ArgumentNullException("decision");
            if (entry == null)
                throw new ArgumentNullException("entry");
            if (market == null || structure == null || mtf == null ||
                runtime == null || configuration == null)
                return InvalidPlan(
                    decision,
                    entry,
                    mtf,
                    runtime,
                    "PHASE8_INPUT_INCOMPLETE");
    
            if (!decision.DecisionEligible ||
                entry.Model == null ||
                !CFIPClean89DirectionRules.IsDirectional(decision.Direction))
                return InvalidPlan(
                    decision,
                    entry,
                    mtf,
                    runtime,
                    "DECISION_OR_ENTRY_MODEL_INVALID");
    
            bool marketEntryReady =
                entry.Eligible &&
                entry.State == CFIPClean89EntryTriggerState.Ready &&
                (entry.Mode == CFIPClean89EntryMode.RetestMarket ||
                 entry.Mode == CFIPClean89EntryMode.BreakoutMarket);
    
            bool pendingProposal =
                !entry.Eligible &&
                entry.State == CFIPClean89EntryTriggerState.WaitingBreakout &&
                (entry.Mode == CFIPClean89EntryMode.ContinuationStop ||
                 entry.Mode == CFIPClean89EntryMode.ReversalLimit) &&
                (entry.Mode == CFIPClean89EntryMode.ReversalLimit ||
                 entry.Model.Trigger != null);
    
            if (!marketEntryReady && !pendingProposal)
                return InvalidPlan(
                    decision,
                    entry,
                    mtf,
                    runtime,
                    "ENTRY_NOT_PLAN_READY");
    
            if (entry.Decision != decision ||
                entry.Direction != decision.Direction)
                return InvalidPlan(
                    decision,
                    entry,
                    mtf,
                    runtime,
                    "DECISION_SNAPSHOT_MISMATCH");
    
            CFIPClean89MarketFrame m5 = market.FindFrame("M5");
            if (m5 == null || !m5.DataValid || m5.Atr <= 0)
                return InvalidPlan(
                    decision,
                    entry,
                    mtf,
                    runtime,
                    "M5_RISK_FRAME_UNAVAILABLE");
    
            double entryPrice = ResolvePlanEntryPrice(entry);
    
            if (entryPrice <= 0)
                return InvalidPlan(
                    decision,
                    entry,
                    mtf,
                    runtime,
                    "ENTRY_PRICE_INVALID");
    
            double atr = m5.Atr;
            double minimumRiskAtr =
                Math.Max(
                    0.1,
                    configuration.Get(
                        "MinimumSlAtr",
                        0.55));
            double maximumRiskAtr =
                Math.Max(
                    minimumRiskAtr,
                    configuration.Get(
                        "MaximumSlAtr",
                        1.80));
            double stopBufferAtr =
                Math.Max(
                    0.0,
                    configuration.Get(
                        "StopBufferAtr",
                        0.10));
            bool requireStructuralStop =
                configuration.Get(
                    "RequireStructuralStop",
                    true);
            bool useHtfStop =
                configuration.Get(
                    "UseHtfStructureForStop",
                    true);
    
            StopCandidate stop =
                FindStructuralStop(
                    decision.Direction,
                    entryPrice,
                    atr,
                    entry,
                    structure,
                    minimumRiskAtr,
                    maximumRiskAtr,
                    stopBufferAtr,
                    useHtfStop);
    
            bool structuralStop =
                stop != null && stop.Structural;
    
            if (stop == null ||
                !IsProtectiveStop(
                    decision.Direction,
                    entryPrice,
                    stop.Price))
            {
                if (requireStructuralStop)
                    return InvalidPlan(
                        decision,
                        entry,
                        mtf,
                        runtime,
                        "STRUCTURAL_STOP_UNAVAILABLE");
    
                double fallbackAtr =
                    Math.Max(
                        minimumRiskAtr,
                        configuration.Get(
                            "FallbackSlAtr",
                            1.00));
    
                double fallbackPrice =
                    decision.Direction == CFIPClean89Direction.Buy
                        ? entryPrice - fallbackAtr * atr
                        : entryPrice + fallbackAtr * atr;
    
                stop = new StopCandidate
                {
                    Price = fallbackPrice,
                    Quality = 50,
                    Distance = Math.Abs(entryPrice - fallbackPrice),
                    Structural = false,
                    Provenance =
                        CFIPClean89Provenance.FallbackFrom(
                            "RISK",
                            "ATR_STOP",
                            CFIPClean89FallbackKind.Atr,
                            "No valid structural stop was available.")
                };
                structuralStop = false;
            }
    
            double riskDistance =
                Math.Abs(entryPrice - stop.Price);
    
            if (riskDistance < minimumRiskAtr * atr)
            {
                double minimumDistance = minimumRiskAtr * atr;
                stop.Price =
                    decision.Direction == CFIPClean89Direction.Buy
                        ? entryPrice - minimumDistance
                        : entryPrice + minimumDistance;
                stop.Distance = minimumDistance;
                stop.Provenance =
                    stop.Structural
                        ? CFIPClean89Provenance.Direct(
                            "RISK",
                            "STRUCTURAL_STOP_EXPANDED_TO_MINIMUM_RISK")
                        : stop.Provenance;
                riskDistance = minimumDistance;
            }
    
            if (riskDistance > maximumRiskAtr * atr)
            {
                if (requireStructuralStop && structuralStop)
                    return InvalidPlan(
                        decision,
                        entry,
                        mtf,
                        runtime,
                        "STRUCTURAL_STOP_EXCEEDS_MAX_RISK");
    
                double fallbackAtr =
                    Math.Max(
                        minimumRiskAtr,
                        configuration.Get(
                            "FallbackSlAtr",
                            1.00));
                double boundedAtr =
                    Math.Min(
                        maximumRiskAtr,
                        Math.Max(
                            minimumRiskAtr,
                            fallbackAtr));
    
                double fallbackPrice =
                    decision.Direction == CFIPClean89Direction.Buy
                        ? entryPrice - boundedAtr * atr
                        : entryPrice + boundedAtr * atr;
    
                stop = new StopCandidate
                {
                    Price = fallbackPrice,
                    Quality = 50,
                    Distance = Math.Abs(entryPrice - fallbackPrice),
                    Structural = false,
                    Provenance =
                        CFIPClean89Provenance.FallbackFrom(
                            "RISK",
                            "ATR_STOP_RISK_CAP",
                            CFIPClean89FallbackKind.Atr,
                            "Structural stop exceeded configured maximum risk.")
                };
                structuralStop = false;
                riskDistance = stop.Distance;
            }
    
            var ladder =
                BuildTargetLadder(
                    decision,
                    entryPrice,
                    stop.Price,
                    atr,
                    structure,
                    configuration);
    
            if (ladder == null ||
                ladder.Levels.Count == 0 ||
                !ladder.ValidateForDirection(decision.Direction, entryPrice))
                return InvalidPlan(
                    decision,
                    entry,
                    mtf,
                    runtime,
                    "TARGET_LADDER_INVALID");
    
            if (configuration.Get("RequireHtfTargets", false) &&
                !HasHtfTarget(ladder))
                return InvalidPlan(
                    decision,
                    entry,
                    mtf,
                    runtime,
                    "HTF_TARGET_REQUIRED");
    
            double tp1 =
                ladder.Find(CFIPClean89TargetStage.TP1).Level.Price;
            CFIPClean89TargetLevel finalTarget =
                ladder.Levels[ladder.Levels.Count - 1];
    
            double rr1 =
                riskDistance > 0
                    ? Math.Abs(tp1 - entryPrice) / riskDistance
                    : 0;
            double rrFinal =
                riskDistance > 0
                    ? Math.Abs(finalTarget.Level.Price - entryPrice) /
                      riskDistance
                    : 0;
    
            int levelQuality =
                Math.Max(
                    0,
                    Math.Min(
                        100,
                        (stop.Quality + finalTarget.Quality) / 2));
    
            var identity =
                new CFIPClean89TradeIdentity(
                    configuration.StrategyId,
                    runtime.Symbol,
                    BuildPlanId(
                        mtf.ReferenceUtc,
                        decision.Direction,
                        entryPrice,
                        stop.Price),
                    BuildSignalId(
                        mtf.ReferenceUtc,
                        decision.Direction));
    
            return new CFIPClean89TradePlan(
                identity,
                decision.Direction,
                entry.Mode,
                entry.Model,
                new CFIPClean89PriceLevel(
                    entryPrice,
                    "EXECUTION_ANCHOR",
                    CFIPClean89Provenance.Direct(
                        "PHASE8",
                        "PLAN_EXECUTION_ANCHOR")),
                new CFIPClean89PriceLevel(
                    stop.Price,
                    structuralStop
                        ? "StructuralStop"
                        : "FallbackStop",
                    stop.Provenance),
                ladder,
                rr1,
                rrFinal,
                levelQuality,
                rr1 > 0,
                runtime.ServerUtc,
                mtf.M5 != null ? mtf.M5.ClosedIndex : -1,
                CFIPClean89Provenance.Direct(
                    "PHASE8",
                    structuralStop
                        ? "STRUCTURAL_RISK_TARGET_PLAN"
                        : "EXPLICIT_STOP_FALLBACK_TARGET_PLAN"));
        }
    
        private double ResolvePlanEntryPrice(
            CFIPClean89EntrySnapshot entry)
        {
            if (entry == null || entry.Model == null)
                return 0;
    
            if (entry.Mode == CFIPClean89EntryMode.ContinuationStop &&
                entry.Model.Trigger != null)
                return entry.Model.Trigger.Price;
    
            if (entry.Mode == CFIPClean89EntryMode.ReversalLimit)
                return entry.Model.IdealEntry.Price;
    
            if (entry.RequestedEntry != null)
                return entry.RequestedEntry.Price;
    
            return entry.Model.IdealEntry.Price;
        }
    
        private StopCandidate FindStructuralStop(
            CFIPClean89Direction direction,
            double entryPrice,
            double atr,
            CFIPClean89EntrySnapshot entry,
            CFIPClean89StructureSnapshot structure,
            double minimumRiskAtr,
            double maximumRiskAtr,
            double stopBufferAtr,
            bool useHtfStop)
        {
            var candidates = new List<StopCandidate>();
    
            if (entry.Model != null &&
                entry.Model.Invalidation != null)
            {
                double p = entry.Model.Invalidation.Price;
                if (IsProtectiveStop(direction, entryPrice, p))
                    candidates.Add(
                        new StopCandidate
                        {
                            Price = p,
                            Quality = 92,
                            Distance = Math.Abs(entryPrice - p),
                            Structural = true,
                            Provenance =
                                CFIPClean89Provenance.Direct(
                                    "ENTRY",
                                    "ENTRY_INVALIDATION")
                        });
            }
    
            for (int i = 0; i < structure.Zones.Count; i++)
            {
                CFIPClean89ZoneRecord z = structure.Zones[i];
    
                if (z.Direction != direction ||
                    z.Invalidated ||
                    z.Consumed ||
                    z.Lifecycle == CFIPClean89ZoneLifecycle.Expired ||
                    !string.Equals(
                        z.Timeframe,
                        "M5",
                        StringComparison.OrdinalIgnoreCase))
                    continue;
    
                double p =
                    direction == CFIPClean89Direction.Buy
                        ? z.CurrentLower - stopBufferAtr * atr
                        : z.CurrentUpper + stopBufferAtr * atr;
    
                if (!IsProtectiveStop(direction, entryPrice, p))
                    continue;
    
                candidates.Add(
                    new StopCandidate
                    {
                        Price = p,
                        Quality = Math.Max(60, z.Quality),
                        Distance = Math.Abs(entryPrice - p),
                        Structural = true,
                        Provenance =
                            CFIPClean89Provenance.Direct(
                                "STRUCTURE",
                                z.Kind == CFIPClean89ZoneKind.FairValueGap
                                    ? "M5_FVG_STOP"
                                    : "M5_OB_STOP")
                    });
            }
    
            for (int i = 0; i < structure.Events.Count; i++)
            {
                CFIPClean89StructureEventRecord e = structure.Events[i];
    
                bool swing =
                    direction == CFIPClean89Direction.Buy
                        ? e.Kind == CFIPClean89StructureEventKind.SwingLow
                        : e.Kind == CFIPClean89StructureEventKind.SwingHigh;
    
                bool htf =
                    e.Timeframe == "H1" ||
                    e.Timeframe == "H4" ||
                    e.Timeframe == "D1";
    
                if (!swing ||
                    !IsProtectiveStop(direction, entryPrice, e.Price) ||
                    (!htf && e.Timeframe != "M5") ||
                    (htf && !useHtfStop))
                    continue;
    
                int quality =
                    Math.Max(
                        50,
                        Math.Min(
                            100,
                            e.Quality +
                            (htf ? 8 : 0)));
    
                candidates.Add(
                    new StopCandidate
                    {
                        Price = e.Price -
                            (direction == CFIPClean89Direction.Buy
                                ? stopBufferAtr * atr
                                : -stopBufferAtr * atr),
                        Quality = quality,
                        Distance = Math.Abs(
                            entryPrice -
                            (e.Price -
                                (direction == CFIPClean89Direction.Buy
                                    ? stopBufferAtr * atr
                                    : -stopBufferAtr * atr))),
                        Structural = true,
                        Provenance =
                            CFIPClean89Provenance.Direct(
                                e.Timeframe,
                                htf
                                    ? "HTF_SWING_STOP"
                                    : "M5_SWING_STOP")
                    });
            }
    
            StopCandidate best = null;
            double maximumDistance = maximumRiskAtr * atr;
    
            for (int i = 0; i < candidates.Count; i++)
            {
                StopCandidate c = candidates[i];
    
                if (c.Distance > maximumDistance)
                    continue;
    
                if (best == null ||
                    c.Distance > best.Distance ||
                    (Math.Abs(c.Distance - best.Distance) < 0.0000001 &&
                     c.Quality > best.Quality))
                    best = c;
            }
    
            if (best != null)
                return best;
    
            // Return the closest valid structural candidate so the caller can
            // explicitly decide whether it must be rejected or replaced by a
            // configured fallback.
            for (int i = 0; i < candidates.Count; i++)
            {
                StopCandidate c = candidates[i];
                if (!IsProtectiveStop(direction, entryPrice, c.Price))
                    continue;
    
                if (best == null ||
                    c.Quality > best.Quality)
                    best = c;
            }
    
            return best;
        }
    
        private CFIPClean89TargetLadder BuildTargetLadder(
            CFIPClean89DecisionSnapshot decision,
            double entryPrice,
            double stopPrice,
            double atr,
            CFIPClean89StructureSnapshot structure,
            CFIPClean89ConfigSnapshot configuration)
        {
            double risk = Math.Abs(entryPrice - stopPrice);
            if (risk <= 0 || atr <= 0)
                return new CFIPClean89TargetLadder(
                    new List<CFIPClean89TargetLevel>());
    
            double[] rr =
            {
                configuration.Get("Tp1MinimumRR", 2.00),
                configuration.Get("Tp2MinimumRR", 3.20),
                configuration.Get("Tp3MinimumRR", 4.80),
                configuration.Get("Tp4MinimumRR", 6.50)
            };
    
            if (configuration.Get("AdaptiveStructuralRR", true))
            {
                double qualityFactor =
                    1.0 +
                    Math.Max(
                        0,
                        decision.Quality - 80) /
                    200.0;
    
                for (int i = 0; i < rr.Length; i++)
                    rr[i] *= qualityFactor;
            }
    
            double spacing =
                Math.Max(
                    0.05,
                    configuration.Get(
                        "MinimumTpSpacingAtr",
                        0.40)) * atr;
            double clearance =
                Math.Max(
                    0.02,
                    configuration.Get(
                        "TargetClearanceAtr",
                        0.10)) * atr;
            double maxExtension =
                Math.Max(
                    1.0,
                    configuration.Get(
                        "MaximumTargetExtensionAtr",
                        4.0)) * atr;
            bool rejectObstacle =
                configuration.Get(
                    "RejectTargetObstacle",
                    true);
            bool allowSynthetic =
                configuration.Get(
                    "AllowSyntheticTargetFallback",
                    true);
    
            var levels = new List<CFIPClean89TargetLevel>();
            double previous = entryPrice;
            bool htfTargetUsed = false;
    
            for (int stageIndex = 0; stageIndex < rr.Length; stageIndex++)
            {
                double minimumDistance = rr[stageIndex] * risk;
                if (minimumDistance > maxExtension)
                    break;
    
                TargetCandidate candidate =
                    FindLiquidityTarget(
                        decision.Direction,
                        entryPrice,
                        previous,
                        minimumDistance,
                        maxExtension,
                        spacing,
                        clearance,
                        structure,
                        rejectObstacle);
    
                if (candidate == null && allowSynthetic)
                {
                    double syntheticDistance =
                        Math.Max(
                            minimumDistance,
                            Math.Abs(previous - entryPrice) + spacing);
    
                    if (syntheticDistance <= maxExtension)
                    {
                        double p =
                            decision.Direction == CFIPClean89Direction.Buy
                                ? entryPrice + syntheticDistance
                                : entryPrice - syntheticDistance;
    
                        if (!rejectObstacle ||
                            !HasObstacle(
                                decision.Direction,
                                entryPrice,
                                p,
                                clearance,
                                structure))
                        {
                            candidate =
                                new TargetCandidate
                                {
                                    Price = p,
                                    Quality = 55,
                                    Htf = false,
                                    Provenance =
                                        CFIPClean89Provenance.FallbackFrom(
                                            "TARGET",
                                            "SYNTHETIC_RR_TARGET",
                                            CFIPClean89FallbackKind.Synthetic,
                                            "No suitable structural/liquidity target met the configured constraints.")
                                };
                        }
                    }
                }
    
                if (candidate == null)
                    break;
    
                CFIPClean89TargetStage stage =
                    (CFIPClean89TargetStage)(stageIndex + 1);
    
                levels.Add(
                    new CFIPClean89TargetLevel(
                        stage,
                        new CFIPClean89PriceLevel(
                            candidate.Price,
                            "TP" + (stageIndex + 1),
                            candidate.Provenance),
                        candidate.Quality,
                        CFIPClean89TargetState.Proposed));
    
                htfTargetUsed =
                    htfTargetUsed || candidate.Htf;
                previous = candidate.Price;
            }
    
            // TP1 is mandatory. Higher stages are only published when their
            // configured RR/extension/obstacle constraints can be satisfied.
            if (levels.Count == 0)
                return new CFIPClean89TargetLadder(
                    new List<CFIPClean89TargetLevel>());
    
            return new CFIPClean89TargetLadder(levels);
        }
    
        private TargetCandidate FindLiquidityTarget(
            CFIPClean89Direction direction,
            double entryPrice,
            double previousTarget,
            double minimumDistance,
            double maximumDistance,
            double spacing,
            double clearance,
            CFIPClean89StructureSnapshot structure,
            bool rejectObstacle)
        {
            TargetCandidate best = null;
    
            for (int i = 0; i < structure.Liquidity.Count; i++)
            {
                CFIPClean89LiquidityRecord x = structure.Liquidity[i];
    
                bool directional =
                    direction == CFIPClean89Direction.Buy
                        ? x.Price > entryPrice
                        : x.Price < entryPrice;
    
                if (!directional || x.Swept)
                    continue;
    
                double distance = Math.Abs(x.Price - entryPrice);
                if (distance < minimumDistance ||
                    distance > maximumDistance)
                    continue;
    
                bool progressesBeyondPrevious =
                    direction == CFIPClean89Direction.Buy
                        ? x.Price > previousTarget + spacing
                        : x.Price < previousTarget - spacing;
    
                if (!progressesBeyondPrevious)
                    continue;
    
                if (Math.Abs(x.Price - entryPrice) < clearance)
                    continue;
    
                if (rejectObstacle &&
                    HasObstacle(
                        direction,
                        entryPrice,
                        x.Price,
                        clearance,
                        structure))
                    continue;
    
                bool htf =
                    x.Timeframe == "H1" ||
                    x.Timeframe == "H4" ||
                    x.Timeframe == "D1" ||
                    x.Timeframe == "W1";
    
                int quality =
                    Math.Max(
                        40,
                        Math.Min(
                            100,
                            x.Quality +
                            (htf ? 6 : 0)));
    
                if (best == null ||
                    quality > best.Quality ||
                    (quality == best.Quality &&
                     distance < Math.Abs(best.Price - entryPrice)))
                {
                    best =
                        new TargetCandidate
                        {
                            Price = x.Price,
                            Quality = quality,
                            Htf = htf,
                            Provenance =
                                CFIPClean89Provenance.Direct(
                                    x.Timeframe,
                                    "LIQUIDITY_TARGET_" +
                                    x.Kind.ToString().ToUpperInvariant())
                        };
                }
            }
    
            return best;
        }
    
        private bool HasObstacle(
            CFIPClean89Direction direction,
            double entryPrice,
            double targetPrice,
            double clearance,
            CFIPClean89StructureSnapshot structure)
        {
            double lower =
                Math.Min(entryPrice, targetPrice) + clearance;
            double upper =
                Math.Max(entryPrice, targetPrice) - clearance;
    
            if (upper <= lower)
                return false;
    
            CFIPClean89Direction opposing =
                direction == CFIPClean89Direction.Buy
                    ? CFIPClean89Direction.Sell
                    : CFIPClean89Direction.Buy;
    
            for (int i = 0; i < structure.Zones.Count; i++)
            {
                CFIPClean89ZoneRecord z = structure.Zones[i];
    
                if (z.Direction != opposing ||
                    z.Invalidated ||
                    z.Consumed ||
                    z.Lifecycle == CFIPClean89ZoneLifecycle.Expired)
                    continue;
    
                if (z.CurrentUpper >= lower &&
                    z.CurrentLower <= upper)
                    return true;
            }
    
            return false;
        }
    
        private bool HasHtfTarget(
            CFIPClean89TargetLadder ladder)
        {
            for (int i = 0; i < ladder.Levels.Count; i++)
            {
                CFIPClean89Provenance p =
                    ladder.Levels[i].Level.Provenance;
    
                if (p != null &&
                    (p.Source == "H1" ||
                     p.Source == "H4" ||
                     p.Source == "D1" ||
                     p.Source == "W1"))
                    return true;
            }
    
            return false;
        }
    
        private bool IsProtectiveStop(
            CFIPClean89Direction direction,
            double entryPrice,
            double stopPrice)
        {
            return
                direction == CFIPClean89Direction.Buy
                    ? stopPrice < entryPrice
                    : direction == CFIPClean89Direction.Sell &&
                      stopPrice > entryPrice;
        }
    
        private string BuildPlanId(
            DateTime referenceUtc,
            CFIPClean89Direction direction,
            double entryPrice,
            double stopPrice)
        {
            return
                "CFIP89|PLAN|" +
                referenceUtc.Ticks.ToString() +
                "|" +
                direction.ToString() +
                "|" +
                entryPrice.ToString("R") +
                "|" +
                stopPrice.ToString("R");
        }
    
        private string BuildSignalId(
            DateTime referenceUtc,
            CFIPClean89Direction direction)
        {
            return
                "CFIP89|SIGNAL|" +
                referenceUtc.Ticks.ToString() +
                "|" +
                direction.ToString();
        }
    
        private CFIPClean89TradePlan InvalidPlan(
            CFIPClean89DecisionSnapshot decision,
            CFIPClean89EntrySnapshot entry,
            CFIPClean89MtfSnapshot mtf,
            CFIPClean89RuntimeSnapshot runtime,
            string reason)
        {
            DateTime now =
                runtime != null
                    ? runtime.ServerUtc
                    : DateTime.MinValue;
    
            DateTime reference =
                mtf != null
                    ? mtf.ReferenceUtc
                    : DateTime.MinValue;
    
            CFIPClean89Direction direction =
                decision != null
                    ? decision.Direction
                    : CFIPClean89Direction.Wait;
    
            CFIPClean89EntryModel entryModel =
                entry != null && entry.Model != null
                    ? entry.Model
                    : new CFIPClean89EntryModel(
                        direction == CFIPClean89Direction.Wait
                            ? CFIPClean89Direction.Buy
                            : direction,
                        new CFIPClean89PriceLevel(
                            runtime != null && runtime.Bid > 0
                                ? runtime.Bid
                                : 1,
                            "InvalidPlanEntry",
                            CFIPClean89Provenance.Direct(
                                "PHASE8",
                                "INVALID_PLAN")),
                        new CFIPClean89PriceZone(
                            runtime != null && runtime.Bid > 0
                                ? runtime.Bid
                                : 1,
                            runtime != null && runtime.Bid > 0
                                ? runtime.Bid
                                : 1,
                            "InvalidPlanZone",
                            CFIPClean89Provenance.Direct(
                                "PHASE8",
                                "INVALID_PLAN")),
                        null,
                        new CFIPClean89PriceLevel(
                            runtime != null && runtime.Bid > 0
                                ? runtime.Bid
                                : 1,
                            "InvalidPlanInvalidation",
                            CFIPClean89Provenance.Direct(
                                "PHASE8",
                                "INVALID_PLAN")));
    
            double safeEntry =
                entryModel.IdealEntry.Price;
            var identity =
                new CFIPClean89TradeIdentity(
                    "CFIP-PRO-v89",
                    runtime != null ? runtime.Symbol : string.Empty,
                    "INVALID|" + reference.Ticks.ToString(),
                    "INVALID|" + reason);
    
            var emptyLadder =
                new CFIPClean89TargetLadder(
                    new List<CFIPClean89TargetLevel>());
    
            return new CFIPClean89TradePlan(
                identity,
                direction,
                entry != null
                    ? entry.Mode
                    : CFIPClean89EntryMode.None,
                entryModel,
                new CFIPClean89PriceLevel(
                    safeEntry,
                    "InvalidPlanExecutionAnchor",
                    CFIPClean89Provenance.Direct(
                        "PHASE8",
                        "INVALID_PLAN")),
                new CFIPClean89PriceLevel(
                    safeEntry,
                    "InvalidPlanStop",
                    CFIPClean89Provenance.FallbackFrom(
                        "PHASE8",
                        reason,
                        CFIPClean89FallbackKind.None,
                        "Plan is explicitly invalid and must not reach execution.")),
                emptyLadder,
                0,
                0,
                0,
                false,
                now,
                mtf != null && mtf.M5 != null
                    ? mtf.M5.ClosedIndex
                    : -1,
                CFIPClean89Provenance.Direct(
                    "PHASE8",
                    reason));
        }
    }
}
