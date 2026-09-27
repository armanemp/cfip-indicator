// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public sealed class CFIPClean89EntryTriggerEngine :
            ICFIPClean89EntryTriggerEngine
        {
            private sealed class ZoneCandidate
            {
                public CFIPClean89ZoneRecord Zone;
                public int Quality;
                public double Distance;
            }
    
            public CFIPClean89EntrySnapshot Evaluate(
                CFIPClean89DecisionSnapshot decision,
                CFIPClean89RuntimeSnapshot runtime,
                CFIPClean89MtfSnapshot mtf,
                CFIPClean89MarketModel market,
                CFIPClean89StructureSnapshot structure,
                CFIPClean89ConfigSnapshot configuration)
            {
                if (decision == null)
                    throw new ArgumentNullException("decision");
    
                if (runtime == null ||
                    mtf == null ||
                    market == null ||
                    structure == null ||
                    configuration == null)
                    return Blocked(
                        decision,
                        CFIPClean89BlockReason.DataIncomplete,
                        CFIPClean89EntryTriggerState.Blocked,
                        "MISSING_ENTRY_INPUT");
    
                // Final direction is NEVER recomputed here.
                CFIPClean89Direction direction = decision.Direction;
    
                if (!decision.DecisionEligible ||
                    direction == CFIPClean89Direction.Wait)
                    return Blocked(
                        decision,
                        FirstDecisionBlock(
                            decision.BlockReasons,
                            CFIPClean89BlockReason.PolicyBlocked),
                        CFIPClean89EntryTriggerState.Blocked,
                        "DECISION_NOT_ENTRY_ELIGIBLE");
    
                CFIPClean89MarketFrame m5 = market.FindFrame("M5");
                CFIPClean89MarketFrame m1 = market.FindFrame("M1");
    
                if (m5 == null ||
                    !m5.DataValid ||
                    m5.Atr <= 0)
                    return Blocked(
                        decision,
                        CFIPClean89BlockReason.DataIncomplete,
                        CFIPClean89EntryTriggerState.Blocked,
                        "M5_ENTRY_FRAME_UNAVAILABLE");
    
                double executablePrice =
                    direction == CFIPClean89Direction.Buy
                        ? runtime.Ask
                        : runtime.Bid;
    
                if (executablePrice <= 0)
                    return Blocked(
                        decision,
                        CFIPClean89BlockReason.DataIncomplete,
                        CFIPClean89EntryTriggerState.Blocked,
                        "EXECUTABLE_PRICE_INVALID");
    
                var blocks = new List<CFIPClean89BlockReason>();
    
                if (configuration.Get("UseSpreadFilter", true) &&
                    Math.Abs(runtime.Ask - runtime.Bid) >
                    m5.Atr *
                    Math.Max(
                        0,
                        configuration.Get(
                            "MaximumSpreadAtr",
                            0.20)))
                    blocks.Add(CFIPClean89BlockReason.SpreadBlocked);
    
                if (configuration.Get("UseM5Confirmation", true) &&
                    m5.BiasDirection != direction)
                    blocks.Add(CFIPClean89BlockReason.MtfDisagreement);
    
                if (configuration.Get("UseM1Trigger", false) &&
                    (m1 == null ||
                     !m1.DataValid ||
                     m1.BiasDirection != direction))
                    blocks.Add(CFIPClean89BlockReason.MtfDisagreement);
    
                if (blocks.Count > 0)
                    return Blocked(
                        decision,
                        FirstBlock(
                            blocks,
                            CFIPClean89BlockReason.EntryInvalid),
                        CFIPClean89EntryTriggerState.Blocked,
                        "ENTRY_PRECONDITIONS",
                        blocks);
    
                ZoneCandidate retest;
                DateTime? retestExpiry;
    
                if (TryFindRetestZone(
                    direction,
                    executablePrice,
                    m5,
                    structure,
                    configuration,
                    out retest,
                    out retestExpiry))
                {
                    if (configuration.Get("AvoidLateEntry", true) &&
                        retestExpiry.HasValue &&
                        runtime.ServerUtc > retestExpiry.Value)
                        return Blocked(
                            decision,
                            CFIPClean89BlockReason.EntryInvalid,
                            CFIPClean89EntryTriggerState.Expired,
                            "RETEST_SETUP_EXPIRED");
    
                    double idealPrice =
                        ZoneMidpoint(retest.Zone);
    
                    double invalidation =
                        direction == CFIPClean89Direction.Buy
                            ? retest.Zone.CurrentLower -
                              m5.Atr *
                              Math.Max(
                                  0,
                                  configuration.Get(
                                      "InvalidationZoneCloseAtr",
                                      0.10))
                            : retest.Zone.CurrentUpper +
                              m5.Atr *
                              Math.Max(
                                  0,
                                  configuration.Get(
                                      "InvalidationZoneCloseAtr",
                                      0.10));
    
                    var idealEntry =
                        new CFIPClean89PriceLevel(
                            idealPrice,
                            "IDEAL_ENTRY",
                            CFIPClean89Provenance.Direct(
                                "ENTRY_ENGINE",
                                "RETEST_ZONE_MIDPOINT"));
    
                    var zone =
                        new CFIPClean89PriceZone(
                            retest.Zone.CurrentLower,
                            retest.Zone.CurrentUpper,
                            retest.Zone.Kind.ToString(),
                            CFIPClean89Provenance.Direct(
                                "ENTRY_ENGINE",
                                "RETEST_ZONE"));
    
                    var invalidationLevel =
                        new CFIPClean89PriceLevel(
                            invalidation,
                            "INVALIDATION",
                            CFIPClean89Provenance.Direct(
                                "ENTRY_ENGINE",
                                "RETEST_INVALIDATION"));
    
                    if (configuration.Get(
                            "EnableSetupInvalidation",
                            true) &&
                        IsInvalidated(
                            direction,
                            executablePrice,
                            invalidation))
                        return new CFIPClean89EntrySnapshot(
                            decision,
                            direction,
                            CFIPClean89EntryMode.RetestMarket,
                            CFIPClean89EntryTriggerState.Invalidated,
                            new CFIPClean89EntryModel(
                                direction,
                                idealEntry,
                                zone,
                                null,
                                invalidationLevel),
                            null,
                            false,
                            false,
                            retest.Quality,
                            mtf.ReferenceUtc,
                            runtime.ServerUtc,
                            retestExpiry,
                            new List<CFIPClean89BlockReason>
                            {
                                CFIPClean89BlockReason.EntryInvalid
                            },
                            CFIPClean89Provenance.Direct(
                                "ENTRY_ENGINE",
                                "RETEST_INVALIDATED"));
    
                    int minimumEntryQuality =
                        configuration.Get(
                            "MinimumEntryQuality",
                            64);
    
                    if (configuration.Get("RequirePrecisionEntry", false) &&
                        retest.Quality < minimumEntryQuality)
                        return Blocked(
                            decision,
                            CFIPClean89BlockReason.EntryInvalid,
                            CFIPClean89EntryTriggerState.WaitingRetest,
                            "RETEST_PRECISION_QUALITY");
    
                    if (!WithinMaximumEntryDistance(
                        executablePrice,
                        idealPrice,
                        m5.Atr,
                        configuration))
                        return Blocked(
                            decision,
                            CFIPClean89BlockReason.EntryInvalid,
                            CFIPClean89EntryTriggerState.WaitingRetest,
                            "RETEST_ENTRY_DISTANCE");
    
                    var model =
                        new CFIPClean89EntryModel(
                            direction,
                            idealEntry,
                            zone,
                            null,
                            invalidationLevel);
    
                    var requestedEntry =
                        new CFIPClean89PriceLevel(
                            executablePrice,
                            "REQUESTED_ENTRY",
                            CFIPClean89Provenance.Direct(
                                "ENTRY_ENGINE",
                                "RETEST_MARKET"));
    
                    return new CFIPClean89EntrySnapshot(
                        decision,
                        direction,
                        CFIPClean89EntryMode.RetestMarket,
                        CFIPClean89EntryTriggerState.Ready,
                        model,
                        requestedEntry,
                        false,
                        true,
                        retest.Quality,
                        mtf.ReferenceUtc,
                        runtime.ServerUtc,
                        retestExpiry,
                        new List<CFIPClean89BlockReason>(),
                        CFIPClean89Provenance.Direct(
                            "ENTRY_ENGINE",
                            "RETEST_READY"));
                }
    
                if (!configuration.Get(
                        "AllowPrecisionBreakoutEntry",
                        true))
                    return Blocked(
                        decision,
                        CFIPClean89BlockReason.EntryInvalid,
                        CFIPClean89EntryTriggerState.WaitingRetest,
                        "RETEST_REQUIRED");
    
                ZoneCandidate breakoutZone =
                    FindBestExecutionZone(
                        direction,
                        executablePrice,
                        structure);
    
                CFIPClean89StructureEventRecord triggerEvent;
    
                if (breakoutZone == null ||
                    !TryFindFreshBreakoutEvent(
                        direction,
                        mtf.ReferenceUtc,
                        m5,
                        structure,
                        configuration,
                        out triggerEvent))
                    return Blocked(
                        decision,
                        CFIPClean89BlockReason.EntryInvalid,
                        CFIPClean89EntryTriggerState.WaitingBreakout,
                        "BREAKOUT_TRIGGER_UNAVAILABLE");
    
                double bufferAtr =
                    Math.Max(
                        0,
                        configuration.Get(
                            "PrecisionBreakoutBufferAtr",
                            configuration.Get(
                                "EntryBufferAtr",
                                0.05)));
    
                double breakoutAnchor =
                    direction == CFIPClean89Direction.Buy
                        ? Math.Max(
                            triggerEvent.Price,
                            breakoutZone.Zone.CurrentUpper)
                        : Math.Min(
                            triggerEvent.Price,
                            breakoutZone.Zone.CurrentLower);
    
                double triggerPrice =
                    direction == CFIPClean89Direction.Buy
                        ? breakoutAnchor + m5.Atr * bufferAtr
                        : breakoutAnchor - m5.Atr * bufferAtr;
    
                double triggerTolerance =
                    m5.Atr *
                    Math.Max(
                        0,
                        configuration.Get(
                            "EntryBufferAtr",
                            0.05));
    
                bool triggerReached =
                    direction == CFIPClean89Direction.Buy
                        ? executablePrice >= triggerPrice - triggerTolerance
                        : executablePrice <= triggerPrice + triggerTolerance;
    
                double invalidation =
                    direction == CFIPClean89Direction.Buy
                        ? Math.Min(
                            breakoutZone.Zone.CurrentLower,
                            triggerEvent.Price) -
                          m5.Atr *
                          Math.Max(
                              0,
                              configuration.Get(
                                  "InvalidationStructureAtr",
                                  0.10))
                        : Math.Max(
                            breakoutZone.Zone.CurrentUpper,
                            triggerEvent.Price) +
                          m5.Atr *
                          Math.Max(
                              0,
                              configuration.Get(
                                  "InvalidationStructureAtr",
                                  0.10));
    
                var breakoutModel =
                    new CFIPClean89EntryModel(
                        direction,
                        new CFIPClean89PriceLevel(
                            ZoneMidpoint(breakoutZone.Zone),
                            "IDEAL_ENTRY",
                            CFIPClean89Provenance.Direct(
                                "ENTRY_ENGINE",
                                "BREAKOUT_ZONE_MIDPOINT")),
                        new CFIPClean89PriceZone(
                            breakoutZone.Zone.CurrentLower,
                            breakoutZone.Zone.CurrentUpper,
                            breakoutZone.Zone.Kind.ToString(),
                            CFIPClean89Provenance.Direct(
                                "ENTRY_ENGINE",
                                "BREAKOUT_ZONE")),
                        new CFIPClean89PriceLevel(
                            triggerPrice,
                            "TRIGGER",
                            CFIPClean89Provenance.Direct(
                                "ENTRY_ENGINE",
                                "BREAKOUT_STRUCTURAL_EVENT")),
                        new CFIPClean89PriceLevel(
                            invalidation,
                            "INVALIDATION",
                            CFIPClean89Provenance.Direct(
                                "ENTRY_ENGINE",
                                "BREAKOUT_INVALIDATION")));
    
                DateTime expiry =
                    ExpiryFor(
                        triggerEvent.TimeUtc,
                        m5,
                        configuration);
    
                if (configuration.Get("RequirePrecisionEntry", false) &&
                    breakoutZone.Quality <
                    configuration.Get(
                        "MinimumEntryQuality",
                        64))
                    return new CFIPClean89EntrySnapshot(
                        decision,
                        direction,
                        CFIPClean89EntryMode.BreakoutMarket,
                        CFIPClean89EntryTriggerState.WaitingBreakout,
                        breakoutModel,
                        null,
                        false,
                        false,
                        breakoutZone.Quality,
                        mtf.ReferenceUtc,
                        runtime.ServerUtc,
                        expiry,
                        new List<CFIPClean89BlockReason>
                        {
                            CFIPClean89BlockReason.EntryInvalid
                        },
                        CFIPClean89Provenance.Direct(
                            "ENTRY_ENGINE",
                            "BREAKOUT_PRECISION_QUALITY"));
    
                if (configuration.Get("AvoidLateEntry", true) &&
                    runtime.ServerUtc > expiry)
                    return new CFIPClean89EntrySnapshot(
                        decision,
                        direction,
                        CFIPClean89EntryMode.BreakoutMarket,
                        CFIPClean89EntryTriggerState.Expired,
                        breakoutModel,
                        null,
                        false,
                        false,
                        breakoutZone.Quality,
                        mtf.ReferenceUtc,
                        runtime.ServerUtc,
                        expiry,
                        new List<CFIPClean89BlockReason>
                        {
                            CFIPClean89BlockReason.EntryInvalid
                        },
                        CFIPClean89Provenance.Direct(
                            "ENTRY_ENGINE",
                            "BREAKOUT_SETUP_EXPIRED"));
    
                if (configuration.Get("EnableSetupInvalidation", true) &&
                    IsInvalidated(
                        direction,
                        executablePrice,
                        invalidation))
                    return new CFIPClean89EntrySnapshot(
                        decision,
                        direction,
                        CFIPClean89EntryMode.BreakoutMarket,
                        CFIPClean89EntryTriggerState.Invalidated,
                        breakoutModel,
                        null,
                        false,
                        false,
                        breakoutZone.Quality,
                        mtf.ReferenceUtc,
                        runtime.ServerUtc,
                        expiry,
                        new List<CFIPClean89BlockReason>
                        {
                            CFIPClean89BlockReason.EntryInvalid
                        },
                        CFIPClean89Provenance.Direct(
                            "ENTRY_ENGINE",
                            "BREAKOUT_INVALIDATED"));
    
                if (!triggerReached)
                {
                    CFIPClean89EntryMode waitingMode =
                        ResolvePendingEntryMode(
                            triggerEvent.Kind,
                            configuration);
    
                    return new CFIPClean89EntrySnapshot(
                        decision,
                        direction,
                        waitingMode,
                        CFIPClean89EntryTriggerState.WaitingBreakout,
                        breakoutModel,
                        null,
                        false,
                        false,
                        breakoutZone.Quality,
                        mtf.ReferenceUtc,
                        runtime.ServerUtc,
                        expiry,
                        new List<CFIPClean89BlockReason>
                        {
                            CFIPClean89BlockReason.EntryInvalid
                        },
                        CFIPClean89Provenance.Direct(
                            "ENTRY_ENGINE",
                            "TRIGGER_WAIT"));
                }
    
                if (!WithinMaximumTriggerDistance(
                    executablePrice,
                    triggerPrice,
                    m5.Atr,
                    configuration))
                    return new CFIPClean89EntrySnapshot(
                        decision,
                        direction,
                        CFIPClean89EntryMode.BreakoutMarket,
                        CFIPClean89EntryTriggerState.WaitingBreakout,
                        breakoutModel,
                        null,
                        true,
                        false,
                        breakoutZone.Quality,
                        mtf.ReferenceUtc,
                        runtime.ServerUtc,
                        expiry,
                        new List<CFIPClean89BlockReason>
                        {
                            CFIPClean89BlockReason.EntryInvalid
                        },
                        CFIPClean89Provenance.Direct(
                            "ENTRY_ENGINE",
                            "LATE_BREAKOUT_ENTRY"));
    
                var requested =
                    new CFIPClean89PriceLevel(
                        executablePrice,
                        "REQUESTED_ENTRY",
                        CFIPClean89Provenance.Direct(
                            "ENTRY_ENGINE",
                            "BREAKOUT_MARKET_AFTER_TRIGGER"));
    
                return new CFIPClean89EntrySnapshot(
                    decision,
                    direction,
                    CFIPClean89EntryMode.BreakoutMarket,
                    CFIPClean89EntryTriggerState.Ready,
                    breakoutModel,
                    requested,
                    true,
                    true,
                    breakoutZone.Quality,
                    mtf.ReferenceUtc,
                    runtime.ServerUtc,
                    expiry,
                    new List<CFIPClean89BlockReason>(),
                    CFIPClean89Provenance.Direct(
                        "ENTRY_ENGINE",
                        "BREAKOUT_TRIGGER_REACHED"));
            }
    
            private bool TryFindRetestZone(
                CFIPClean89Direction direction,
                double executablePrice,
                CFIPClean89MarketFrame m5,
                CFIPClean89StructureSnapshot structure,
                CFIPClean89ConfigSnapshot cfg,
                out ZoneCandidate candidate,
                out DateTime? expiresUtc)
            {
                candidate = null;
                expiresUtc = null;
    
                double tolerance =
                    m5.Atr *
                    Math.Max(
                        0,
                        cfg.Get(
                            "RetestZoneToleranceAtr",
                            0.12));
    
                int minimumQuality =
                    cfg.Get(
                        "MinimumRetestQuality",
                        cfg.Get(
                            "MinimumEntryQuality",
                            64));
    
                for (int i = 0; i < structure.Zones.Count; i++)
                {
                    CFIPClean89ZoneRecord zone =
                        structure.Zones[i];
    
                    if (!IsUsableZone(zone, direction))
                        continue;
    
                    if (m5.ClosedBarTimeUtc < zone.CreatedUtc)
                        continue;
    
                    if (zone.AgeBars >
                        Math.Max(
                            1,
                            cfg.Get(
                                "RetestMaxBarsAfterDisplacement",
                                12)))
                        continue;
    
                    double lower =
                        zone.CurrentLower - tolerance;
                    double upper =
                        zone.CurrentUpper + tolerance;
    
                    bool priceInsideExpandedZone =
                        executablePrice >= lower &&
                        executablePrice <= upper;
    
                    if (zone.Kind ==
                            CFIPClean89ZoneKind.FairValueGap &&
                        cfg.Get(
                            "RequireFvgRetest",
                            false) &&
                        !zone.Contains(executablePrice))
                        continue;
    
                    if (zone.Kind ==
                            CFIPClean89ZoneKind.OrderBlock &&
                        cfg.Get(
                            "RequireObRetest",
                            false) &&
                        !zone.Contains(executablePrice))
                        continue;
    
                    bool touched =
                        priceInsideExpandedZone ||
                        (
                            direction == CFIPClean89Direction.Buy
                                ? m5.Low <= upper
                                : m5.High >= lower);
    
                    if (!touched)
                        continue;
    
                    bool closeConfirmed =
                        direction == CFIPClean89Direction.Buy
                            ? m5.Close >= ZoneMidpoint(zone)
                            : m5.Close <= ZoneMidpoint(zone);
    
                    if (cfg.Get(
                            "RequireRetestCloseConfirmation",
                            true) &&
                        (!touched || !closeConfirmed))
                        continue;
    
                    double bodyAtr =
                        Math.Abs(m5.Close - m5.Open) /
                        m5.Atr;
    
                    if (bodyAtr <
                        Math.Max(
                            0,
                            cfg.Get(
                                "RetestRejectionBodyAtr",
                                0.15)))
                        continue;
    
                    int quality = zone.Quality;
    
                    if (zone.LiquidityConfluence)
                        quality += 6;
    
                    if (zone.FvgConfluence)
                        quality += 6;
    
                    if (zone.Retested ||
                        zone.Lifecycle ==
                        CFIPClean89ZoneLifecycle.Retested ||
                        zone.Lifecycle ==
                        CFIPClean89ZoneLifecycle.PartiallyMitigated)
                        quality += 6;
    
                    quality =
                        Math.Max(
                            0,
                            Math.Min(
                                100,
                                quality));
    
                    if (cfg.Get(
                            "RequireRetestQuality",
                            true) &&
                        quality < minimumQuality)
                        continue;
    
                    double distance =
                        Math.Abs(
                            executablePrice -
                            ZoneMidpoint(zone));
    
                    if (candidate == null ||
                        quality > candidate.Quality ||
                        (quality == candidate.Quality &&
                         distance < candidate.Distance))
                    {
                        candidate =
                            new ZoneCandidate
                            {
                                Zone = zone,
                                Quality = quality,
                                Distance = distance
                            };
    
                        expiresUtc =
                            zone.CreatedUtc +
                            TimeSpan.FromMinutes(
                                5 *
                                Math.Max(
                                    1,
                                    cfg.Get(
                                        "RetestMaxBarsAfterDisplacement",
                                        12)));
                    }
                }
    
                return candidate != null;
            }
    
            private ZoneCandidate FindBestExecutionZone(
                CFIPClean89Direction direction,
                double price,
                CFIPClean89StructureSnapshot structure)
            {
                ZoneCandidate best = null;
    
                for (int i = 0; i < structure.Zones.Count; i++)
                {
                    CFIPClean89ZoneRecord zone =
                        structure.Zones[i];
    
                    if (!IsUsableZone(zone, direction))
                        continue;
    
                    double distance =
                        price <= zone.CurrentLower
                            ? zone.CurrentLower - price
                            : price >= zone.CurrentUpper
                                ? price - zone.CurrentUpper
                                : 0;
    
                    var candidate =
                        new ZoneCandidate
                        {
                            Zone = zone,
                            Quality = zone.Quality,
                            Distance = distance
                        };
    
                    if (best == null ||
                        candidate.Quality > best.Quality ||
                        (candidate.Quality == best.Quality &&
                         candidate.Distance < best.Distance))
                        best = candidate;
                }
    
                return best;
            }
    
            private bool TryFindFreshBreakoutEvent(
                CFIPClean89Direction direction,
                DateTime referenceUtc,
                CFIPClean89MarketFrame m5,
                CFIPClean89StructureSnapshot structure,
                CFIPClean89ConfigSnapshot cfg,
                out CFIPClean89StructureEventRecord latest)
            {
                latest = null;
    
                DateTime cutoff =
                    referenceUtc -
                    TimeSpan.FromMinutes(
                        5 *
                        Math.Max(
                            1,
                            cfg.Get(
                                "RetestLookbackBars",
                                8)));
    
                bool structuralBreakPresent = false;
                bool displacementPresent = false;
                bool liquiditySweepPresent = false;
    
                for (int i = 0; i < structure.Events.Count; i++)
                {
                    CFIPClean89StructureEventRecord item =
                        structure.Events[i];
    
                    if (item.Direction != direction ||
                        item.TimeUtc < cutoff ||
                        item.TimeUtc > referenceUtc ||
                        !string.Equals(
                            item.Timeframe,
                            "M5",
                            StringComparison.OrdinalIgnoreCase))
                        continue;
    
                    if (IsTriggerEvent(item.Kind))
                    {
                        if (latest == null ||
                            item.TimeUtc > latest.TimeUtc)
                            latest = item;
                    }
    
                    if (item.Kind ==
                            CFIPClean89StructureEventKind.BreakOfStructure ||
                        item.Kind ==
                            CFIPClean89StructureEventKind.MarketStructureShift ||
                        item.Kind ==
                            CFIPClean89StructureEventKind.ChangeOfCharacter)
                        structuralBreakPresent = true;
    
                    if (item.Kind ==
                            CFIPClean89StructureEventKind.Displacement)
                        displacementPresent = true;
    
                    if (item.Kind ==
                            CFIPClean89StructureEventKind.LiquiditySweep)
                        liquiditySweepPresent = true;
                }
    
                if (latest == null)
                    return false;
    
                if (!cfg.Get("RequireFreshM5Trigger", true))
                    return true;
    
                int triggerEvidence = 0;
    
                // BOS/MSS/CHOCH are one structural-break family.
                if (structuralBreakPresent)
                    triggerEvidence += 2;
    
                if (displacementPresent)
                    triggerEvidence += 1;
    
                if (liquiditySweepPresent)
                    triggerEvidence += 1;
    
                double candleRange =
                    Math.Max(0, m5.High - m5.Low);
    
                double candleBody =
                    Math.Abs(m5.Close - m5.Open);
    
                double bodyAtr =
                    candleRange <= 0
                        ? 0
                        : candleBody / Math.Max(m5.Atr, 1e-12);
    
                double rangeAtr =
                    candleRange <= 0
                        ? 0
                        : candleRange / Math.Max(m5.Atr, 1e-12);
    
                double closeLocation =
                    candleRange <= 0
                        ? 0.5
                        : direction == CFIPClean89Direction.Buy
                            ? (m5.Close - m5.Low) / candleRange
                            : (m5.High - m5.Close) / candleRange;
    
                if (bodyAtr >=
                    Math.Max(
                        0,
                        cfg.Get(
                            "MinimumTriggerBodyAtr",
                            0.12)))
                    triggerEvidence += 1;
    
                if (closeLocation >=
                    Math.Max(
                        0.50,
                        Math.Min(
                            0.95,
                            cfg.Get(
                                "MinimumCloseLocation",
                                0.65))))
                    triggerEvidence += 1;
    
                double maximumRange =
                    Math.Max(
                        0,
                        cfg.Get(
                            "MaximumTriggerRangeAtr",
                            2.5));
    
                if (maximumRange > 0 &&
                    rangeAtr > maximumRange)
                    return false;
    
                int required =
                    Math.Max(
                        1,
                        cfg.Get(
                            "MinimumFreshTriggerEvidence",
                            3));
    
                return triggerEvidence >= required;
            }
    
            private double ZoneMidpoint(
                CFIPClean89ZoneRecord zone)
            {
                if (zone == null)
                    return 0;
    
                return
                    (zone.CurrentLower + zone.CurrentUpper) /
                    2.0;
            }
    
            private bool IsTriggerEvent(
                CFIPClean89StructureEventKind kind)
            {
                return
                    kind ==
                        CFIPClean89StructureEventKind.BreakOfStructure ||
                    kind ==
                        CFIPClean89StructureEventKind.MarketStructureShift ||
                    kind ==
                        CFIPClean89StructureEventKind.ChangeOfCharacter ||
                    kind ==
                        CFIPClean89StructureEventKind.Displacement;
            }
    
            private bool IsUsableZone(
                CFIPClean89ZoneRecord zone,
                CFIPClean89Direction direction)
            {
                if (zone == null ||
                    zone.Direction != direction ||
                    !zone.ExecutionEligible ||
                    zone.Consumed ||
                    zone.Invalidated ||
                    zone.Lifecycle ==
                        CFIPClean89ZoneLifecycle.Invalidated ||
                    zone.Lifecycle ==
                        CFIPClean89ZoneLifecycle.Expired)
                    return false;
    
                return
                    zone.CurrentLower > 0 &&
                    zone.CurrentUpper >= zone.CurrentLower;
            }
    
            private CFIPClean89EntryMode ResolvePendingEntryMode(
                CFIPClean89StructureEventKind kind,
                CFIPClean89ConfigSnapshot cfg)
            {
                if (kind ==
                        CFIPClean89StructureEventKind.MarketStructureShift ||
                    kind ==
                        CFIPClean89StructureEventKind.ChangeOfCharacter)
                    return CFIPClean89EntryMode.ReversalLimit;
    
                CFIPClean89PendingOrderMode pendingMode =
                    cfg.Get(
                        "PendingOrderMode",
                        CFIPClean89PendingOrderMode.Adaptive);
    
                if (pendingMode ==
                        CFIPClean89PendingOrderMode.ContinuationStop ||
                    pendingMode ==
                        CFIPClean89PendingOrderMode.Both ||
                    pendingMode ==
                        CFIPClean89PendingOrderMode.Adaptive)
                    return CFIPClean89EntryMode.ContinuationStop;
    
                return CFIPClean89EntryMode.BreakoutMarket;
            }
    
            private bool WithinMaximumEntryDistance(
                double executablePrice,
                double idealPrice,
                double atr,
                CFIPClean89ConfigSnapshot cfg)
            {
                if (atr <= 0)
                    return false;
    
                double max =
                    Math.Max(
                        0,
                        cfg.Get(
                            "MaximumEntryDistanceAtr",
                            0.60));
    
                if (max <= 0)
                    return true;
    
                return
                    Math.Abs(executablePrice - idealPrice) /
                    atr <= max;
            }
    
            private bool WithinMaximumTriggerDistance(
                double executablePrice,
                double triggerPrice,
                double atr,
                CFIPClean89ConfigSnapshot cfg)
            {
                if (atr <= 0)
                    return false;
    
                double max =
                    Math.Max(
                        0,
                        Math.Min(
                            cfg.Get(
                                "MaximumEntryExtensionAtr",
                                0.60),
                            cfg.Get(
                                "MaximumEntryDistanceAtr",
                                0.60)));
    
                return
                    max <= 0 ||
                    Math.Abs(executablePrice - triggerPrice) /
                    atr <= max;
            }
    
            private bool IsInvalidated(
                CFIPClean89Direction direction,
                double price,
                double invalidation)
            {
                return direction == CFIPClean89Direction.Buy
                    ? price <= invalidation
                    : price >= invalidation;
            }
    
            private DateTime ExpiryFor(
                DateTime sourceUtc,
                CFIPClean89MarketFrame m5,
                CFIPClean89ConfigSnapshot cfg)
            {
                return
                    sourceUtc +
                    TimeSpan.FromMinutes(
                        5 *
                        Math.Max(
                            1,
                            cfg.Get(
                                "RetestLookbackBars",
                                8)));
            }
    
            private CFIPClean89EntrySnapshot Blocked(
                CFIPClean89DecisionSnapshot decision,
                CFIPClean89BlockReason reason,
                CFIPClean89EntryTriggerState state,
                string rule)
            {
                return Blocked(
                    decision,
                    reason,
                    state,
                    rule,
                    null);
            }
    
            private CFIPClean89EntrySnapshot Blocked(
                CFIPClean89DecisionSnapshot decision,
                CFIPClean89BlockReason primaryReason,
                CFIPClean89EntryTriggerState state,
                string rule,
                IList<CFIPClean89BlockReason> additional)
            {
                var blocks =
                    new List<CFIPClean89BlockReason>();
    
                if (additional != null)
                {
                    for (int i = 0; i < additional.Count; i++)
                        if (!blocks.Contains(additional[i]))
                            blocks.Add(additional[i]);
                }
    
                if (!blocks.Contains(primaryReason))
                    blocks.Add(primaryReason);
    
                return new CFIPClean89EntrySnapshot(
                    decision,
                    decision.Direction,
                    CFIPClean89EntryMode.None,
                    state,
                    null,
                    null,
                    false,
                    false,
                    0,
                    DateTime.MinValue,
                    DateTime.MinValue,
                    null,
                    blocks,
                    CFIPClean89Provenance.Direct(
                        "ENTRY_ENGINE",
                        rule));
            }
    
            private CFIPClean89BlockReason FirstDecisionBlock(
                IReadOnlyList<CFIPClean89BlockReason> blocks,
                CFIPClean89BlockReason fallback)
            {
                if (blocks != null)
                {
                    for (int i = 0; i < blocks.Count; i++)
                        if (blocks[i] !=
                            CFIPClean89BlockReason.None)
                            return blocks[i];
                }
    
                return fallback;
            }
    
            private CFIPClean89BlockReason FirstBlock(
                IList<CFIPClean89BlockReason> blocks,
                CFIPClean89BlockReason fallback)
            {
                if (blocks != null)
                {
                    for (int i = 0; i < blocks.Count; i++)
                        if (blocks[i] !=
                            CFIPClean89BlockReason.None)
                            return blocks[i];
                }
    
                return fallback;
            }
        }
}
