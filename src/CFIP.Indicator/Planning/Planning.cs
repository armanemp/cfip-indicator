using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CFIP.Indicator
{
        public enum EntryTriggerState
        {
            Blocked = 0,
            WaitingRetest = 1,
            WaitingBreakout = 2,
            TriggerReached = 3,
            Ready = 4,
            Invalidated = 5,
            Expired = 6
        }
    
        public sealed class EntrySnapshot
        {
            private readonly ReadOnlyCollection<BlockReason> _blockReasons;
    
            // This is the exact  snapshot received by Entry.
            public DecisionSnapshot Decision { get; private set; }
    
            public Direction Direction { get; private set; }
            public EntryMode Mode { get; private set; }
            public EntryTriggerState State { get; private set; }
            public EntryModel Model { get; private set; }
            public PriceLevel RequestedEntry { get; private set; }
    
            public bool TriggerReached { get; private set; }
            public bool Eligible { get; private set; }
    
            public int EntryQuality { get; private set; }
            public DateTime ReferenceUtc { get; private set; }
            public DateTime CreatedUtc { get; private set; }
            public DateTime? ExpiresUtc { get; private set; }
    
            public IReadOnlyList<BlockReason> BlockReasons
            {
                get { return _blockReasons; }
            }
    
            public Provenance Provenance { get; private set; }
    
            public EntrySnapshot(
                DecisionSnapshot decision,
                Direction direction,
                EntryMode mode,
                EntryTriggerState state,
                EntryModel model,
                PriceLevel requestedEntry,
                bool triggerReached,
                bool eligible,
                int entryQuality,
                DateTime referenceUtc,
                DateTime createdUtc,
                DateTime? expiresUtc,
                IList<BlockReason> blockReasons,
                Provenance provenance)
            {
                Decision =
                    decision ??
                    throw new ArgumentNullException("decision");
    
                if (direction != decision.Direction)
                    throw new ArgumentException(
                        "Entry direction must equal the authoritative Decision direction.",
                        "direction");
    
                if (eligible &&
                    (!decision.DecisionEligible ||
                     state != EntryTriggerState.Ready ||
                     model == null ||
                     requestedEntry == null ||
                     direction == Direction.Wait))
                    throw new ArgumentException(
                        "Eligible Entry requires a directional ready Decision-backed model.",
                        "eligible");
    
                if (eligible &&
                    blockReasons != null &&
                    blockReasons.Count > 0)
                    throw new ArgumentException(
                        "Eligible Entry cannot contain block reasons.",
                        "blockReasons");
    
                if (!eligible &&
                    (blockReasons == null || blockReasons.Count == 0))
                    throw new ArgumentException(
                        "Blocked Entry requires at least one block reason.",
                        "blockReasons");
    
                if (triggerReached &&
                    model == null)
                    throw new ArgumentException(
                        "A reached Trigger requires an EntryModel.",
                        "triggerReached");
    
                Direction = direction;
                Mode = mode;
                State = state;
                Model = model;
                RequestedEntry = requestedEntry;
                TriggerReached = triggerReached;
                Eligible = eligible;
                EntryQuality = Math.Max(0, Math.Min(100, entryQuality));
                ReferenceUtc = referenceUtc;
                CreatedUtc = createdUtc;
                ExpiresUtc = expiresUtc;
    
                _blockReasons =
                    new ReadOnlyCollection<BlockReason>(
                        new List<BlockReason>(
                            blockReasons ??
                            new List<BlockReason>()));
    
                Provenance =
                    provenance ??
                    Provenance.Direct(
                        "UNKNOWN",
                        "UNSPECIFIED");
            }
        }
    
        public sealed class EntryTriggerEngine :
            IEntryTriggerEngine
        {
            private sealed class ZoneCandidate
            {
                public ZoneRecord Zone;
                public int Quality;
                public double Distance;
            }
    
            public EntrySnapshot Evaluate(
                DecisionSnapshot decision,
                RuntimeSnapshot runtime,
                MtfSnapshot mtf,
                MarketModel market,
                StructureSnapshot structure,
                ConfigSnapshot configuration)
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
                        BlockReason.DataIncomplete,
                        EntryTriggerState.Blocked,
                        "MISSING_ENTRY_INPUT");
    
                // Final direction is NEVER recomputed here.
                Direction direction = decision.Direction;
    
                if (!decision.DecisionEligible ||
                    direction == Direction.Wait)
                    return Blocked(
                        decision,
                        FirstDecisionBlock(
                            decision.BlockReasons,
                            BlockReason.PolicyBlocked),
                        EntryTriggerState.Blocked,
                        "DECISION_NOT_ENTRY_ELIGIBLE");
    
                MarketFrame m5 = market.FindFrame("M5");
                MarketFrame m1 = market.FindFrame("M1");
    
                if (m5 == null ||
                    !m5.DataValid ||
                    m5.Atr <= 0)
                    return Blocked(
                        decision,
                        BlockReason.DataIncomplete,
                        EntryTriggerState.Blocked,
                        "M5_ENTRY_FRAME_UNAVAILABLE");
    
                double executablePrice =
                    direction == Direction.Buy
                        ? runtime.Ask
                        : runtime.Bid;
    
                if (executablePrice <= 0)
                    return Blocked(
                        decision,
                        BlockReason.DataIncomplete,
                        EntryTriggerState.Blocked,
                        "EXECUTABLE_PRICE_INVALID");
    
                var blocks = new List<BlockReason>();
    
                if (configuration.Get("UseSpreadFilter", true) &&
                    Math.Abs(runtime.Ask - runtime.Bid) >
                    m5.Atr *
                    Math.Max(
                        0,
                        configuration.Get(
                            "MaximumSpreadAtr",
                            0.20)))
                    blocks.Add(BlockReason.SpreadBlocked);
    
                if (configuration.Get("UseM5Confirmation", true) &&
                    m5.BiasDirection != direction)
                    blocks.Add(BlockReason.MtfDisagreement);
    
                if (configuration.Get("UseM1Trigger", false) &&
                    (m1 == null ||
                     !m1.DataValid ||
                     m1.BiasDirection != direction))
                    blocks.Add(BlockReason.MtfDisagreement);
    
                if (blocks.Count > 0)
                    return Blocked(
                        decision,
                        FirstBlock(
                            blocks,
                            BlockReason.EntryInvalid),
                        EntryTriggerState.Blocked,
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
                            BlockReason.EntryInvalid,
                            EntryTriggerState.Expired,
                            "RETEST_SETUP_EXPIRED");
    
                    double idealPrice =
                        ZoneMidpoint(retest.Zone);
    
                    double invalidation =
                        direction == Direction.Buy
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
                        new PriceLevel(
                            idealPrice,
                            "IDEAL_ENTRY",
                            Provenance.Direct(
                                "ENTRY_ENGINE",
                                "RETEST_ZONE_MIDPOINT"));
    
                    var zone =
                        new PriceZone(
                            retest.Zone.CurrentLower,
                            retest.Zone.CurrentUpper,
                            retest.Zone.Kind.ToString(),
                            Provenance.Direct(
                                "ENTRY_ENGINE",
                                "RETEST_ZONE"));
    
                    var invalidationLevel =
                        new PriceLevel(
                            invalidation,
                            "INVALIDATION",
                            Provenance.Direct(
                                "ENTRY_ENGINE",
                                "RETEST_INVALIDATION"));
    
                    if (configuration.Get(
                            "EnableSetupInvalidation",
                            true) &&
                        IsInvalidated(
                            direction,
                            executablePrice,
                            invalidation))
                        return new EntrySnapshot(
                            decision,
                            direction,
                            EntryMode.RetestMarket,
                            EntryTriggerState.Invalidated,
                            new EntryModel(
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
                            new List<BlockReason>
                            {
                                BlockReason.EntryInvalid
                            },
                            Provenance.Direct(
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
                            BlockReason.EntryInvalid,
                            EntryTriggerState.WaitingRetest,
                            "RETEST_PRECISION_QUALITY");
    
                    if (!WithinMaximumEntryDistance(
                        executablePrice,
                        idealPrice,
                        m5.Atr,
                        configuration))
                        return Blocked(
                            decision,
                            BlockReason.EntryInvalid,
                            EntryTriggerState.WaitingRetest,
                            "RETEST_ENTRY_DISTANCE");
    
                    var model =
                        new EntryModel(
                            direction,
                            idealEntry,
                            zone,
                            null,
                            invalidationLevel);
    
                    var requestedEntry =
                        new PriceLevel(
                            executablePrice,
                            "REQUESTED_ENTRY",
                            Provenance.Direct(
                                "ENTRY_ENGINE",
                                "RETEST_MARKET"));
    
                    return new EntrySnapshot(
                        decision,
                        direction,
                        EntryMode.RetestMarket,
                        EntryTriggerState.Ready,
                        model,
                        requestedEntry,
                        false,
                        true,
                        retest.Quality,
                        mtf.ReferenceUtc,
                        runtime.ServerUtc,
                        retestExpiry,
                        new List<BlockReason>(),
                        Provenance.Direct(
                            "ENTRY_ENGINE",
                            "RETEST_READY"));
                }
    
                if (!configuration.Get(
                        "AllowPrecisionBreakoutEntry",
                        true))
                    return Blocked(
                        decision,
                        BlockReason.EntryInvalid,
                        EntryTriggerState.WaitingRetest,
                        "RETEST_REQUIRED");
    
                ZoneCandidate breakoutZone =
                    FindBestExecutionZone(
                        direction,
                        executablePrice,
                        structure);
    
                StructureEventRecord triggerEvent;
    
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
                        BlockReason.EntryInvalid,
                        EntryTriggerState.WaitingBreakout,
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
                    direction == Direction.Buy
                        ? Math.Max(
                            triggerEvent.Price,
                            breakoutZone.Zone.CurrentUpper)
                        : Math.Min(
                            triggerEvent.Price,
                            breakoutZone.Zone.CurrentLower);
    
                double triggerPrice =
                    direction == Direction.Buy
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
                    direction == Direction.Buy
                        ? executablePrice >= triggerPrice - triggerTolerance
                        : executablePrice <= triggerPrice + triggerTolerance;
    
                double invalidation =
                    direction == Direction.Buy
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
                    new EntryModel(
                        direction,
                        new PriceLevel(
                            ZoneMidpoint(breakoutZone.Zone),
                            "IDEAL_ENTRY",
                            Provenance.Direct(
                                "ENTRY_ENGINE",
                                "BREAKOUT_ZONE_MIDPOINT")),
                        new PriceZone(
                            breakoutZone.Zone.CurrentLower,
                            breakoutZone.Zone.CurrentUpper,
                            breakoutZone.Zone.Kind.ToString(),
                            Provenance.Direct(
                                "ENTRY_ENGINE",
                                "BREAKOUT_ZONE")),
                        new PriceLevel(
                            triggerPrice,
                            "TRIGGER",
                            Provenance.Direct(
                                "ENTRY_ENGINE",
                                "BREAKOUT_STRUCTURAL_EVENT")),
                        new PriceLevel(
                            invalidation,
                            "INVALIDATION",
                            Provenance.Direct(
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
                    return new EntrySnapshot(
                        decision,
                        direction,
                        EntryMode.BreakoutMarket,
                        EntryTriggerState.WaitingBreakout,
                        breakoutModel,
                        null,
                        false,
                        false,
                        breakoutZone.Quality,
                        mtf.ReferenceUtc,
                        runtime.ServerUtc,
                        expiry,
                        new List<BlockReason>
                        {
                            BlockReason.EntryInvalid
                        },
                        Provenance.Direct(
                            "ENTRY_ENGINE",
                            "BREAKOUT_PRECISION_QUALITY"));
    
                if (configuration.Get("AvoidLateEntry", true) &&
                    runtime.ServerUtc > expiry)
                    return new EntrySnapshot(
                        decision,
                        direction,
                        EntryMode.BreakoutMarket,
                        EntryTriggerState.Expired,
                        breakoutModel,
                        null,
                        false,
                        false,
                        breakoutZone.Quality,
                        mtf.ReferenceUtc,
                        runtime.ServerUtc,
                        expiry,
                        new List<BlockReason>
                        {
                            BlockReason.EntryInvalid
                        },
                        Provenance.Direct(
                            "ENTRY_ENGINE",
                            "BREAKOUT_SETUP_EXPIRED"));
    
                if (configuration.Get("EnableSetupInvalidation", true) &&
                    IsInvalidated(
                        direction,
                        executablePrice,
                        invalidation))
                    return new EntrySnapshot(
                        decision,
                        direction,
                        EntryMode.BreakoutMarket,
                        EntryTriggerState.Invalidated,
                        breakoutModel,
                        null,
                        false,
                        false,
                        breakoutZone.Quality,
                        mtf.ReferenceUtc,
                        runtime.ServerUtc,
                        expiry,
                        new List<BlockReason>
                        {
                            BlockReason.EntryInvalid
                        },
                        Provenance.Direct(
                            "ENTRY_ENGINE",
                            "BREAKOUT_INVALIDATED"));
    
                if (!triggerReached)
                {
                    EntryMode waitingMode =
                        ResolvePendingEntryMode(
                            triggerEvent.Kind,
                            configuration);
    
                    return new EntrySnapshot(
                        decision,
                        direction,
                        waitingMode,
                        EntryTriggerState.WaitingBreakout,
                        breakoutModel,
                        null,
                        false,
                        false,
                        breakoutZone.Quality,
                        mtf.ReferenceUtc,
                        runtime.ServerUtc,
                        expiry,
                        new List<BlockReason>
                        {
                            BlockReason.EntryInvalid
                        },
                        Provenance.Direct(
                            "ENTRY_ENGINE",
                            "TRIGGER_WAIT"));
                }
    
                if (!WithinMaximumTriggerDistance(
                    executablePrice,
                    triggerPrice,
                    m5.Atr,
                    configuration))
                    return new EntrySnapshot(
                        decision,
                        direction,
                        EntryMode.BreakoutMarket,
                        EntryTriggerState.WaitingBreakout,
                        breakoutModel,
                        null,
                        true,
                        false,
                        breakoutZone.Quality,
                        mtf.ReferenceUtc,
                        runtime.ServerUtc,
                        expiry,
                        new List<BlockReason>
                        {
                            BlockReason.EntryInvalid
                        },
                        Provenance.Direct(
                            "ENTRY_ENGINE",
                            "LATE_BREAKOUT_ENTRY"));
    
                var requested =
                    new PriceLevel(
                        executablePrice,
                        "REQUESTED_ENTRY",
                        Provenance.Direct(
                            "ENTRY_ENGINE",
                            "BREAKOUT_MARKET_AFTER_TRIGGER"));
    
                return new EntrySnapshot(
                    decision,
                    direction,
                    EntryMode.BreakoutMarket,
                    EntryTriggerState.Ready,
                    breakoutModel,
                    requested,
                    true,
                    true,
                    breakoutZone.Quality,
                    mtf.ReferenceUtc,
                    runtime.ServerUtc,
                    expiry,
                    new List<BlockReason>(),
                    Provenance.Direct(
                        "ENTRY_ENGINE",
                        "BREAKOUT_TRIGGER_REACHED"));
            }
    
            private bool TryFindRetestZone(
                Direction direction,
                double executablePrice,
                MarketFrame m5,
                StructureSnapshot structure,
                ConfigSnapshot cfg,
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
                    ZoneRecord zone =
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
                            ZoneKind.FairValueGap &&
                        cfg.Get(
                            "RequireFvgRetest",
                            false) &&
                        !zone.Contains(executablePrice))
                        continue;
    
                    if (zone.Kind ==
                            ZoneKind.OrderBlock &&
                        cfg.Get(
                            "RequireObRetest",
                            false) &&
                        !zone.Contains(executablePrice))
                        continue;
    
                    bool touched =
                        priceInsideExpandedZone ||
                        (
                            direction == Direction.Buy
                                ? m5.Low <= upper
                                : m5.High >= lower);
    
                    if (!touched)
                        continue;
    
                    bool closeConfirmed =
                        direction == Direction.Buy
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
                        ZoneLifecycle.Retested ||
                        zone.Lifecycle ==
                        ZoneLifecycle.PartiallyMitigated)
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
                Direction direction,
                double price,
                StructureSnapshot structure)
            {
                ZoneCandidate best = null;
    
                for (int i = 0; i < structure.Zones.Count; i++)
                {
                    ZoneRecord zone =
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
                Direction direction,
                DateTime referenceUtc,
                MarketFrame m5,
                StructureSnapshot structure,
                ConfigSnapshot cfg,
                out StructureEventRecord latest)
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
                    StructureEventRecord item =
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
                            StructureEventKind.BreakOfStructure ||
                        item.Kind ==
                            StructureEventKind.MarketStructureShift ||
                        item.Kind ==
                            StructureEventKind.ChangeOfCharacter)
                        structuralBreakPresent = true;
    
                    if (item.Kind ==
                            StructureEventKind.Displacement)
                        displacementPresent = true;
    
                    if (item.Kind ==
                            StructureEventKind.LiquiditySweep)
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
                        : direction == Direction.Buy
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
                ZoneRecord zone)
            {
                if (zone == null)
                    return 0;
    
                return
                    (zone.CurrentLower + zone.CurrentUpper) /
                    2.0;
            }
    
            private bool IsTriggerEvent(
                StructureEventKind kind)
            {
                return
                    kind ==
                        StructureEventKind.BreakOfStructure ||
                    kind ==
                        StructureEventKind.MarketStructureShift ||
                    kind ==
                        StructureEventKind.ChangeOfCharacter ||
                    kind ==
                        StructureEventKind.Displacement;
            }
    
            private bool IsUsableZone(
                ZoneRecord zone,
                Direction direction)
            {
                if (zone == null ||
                    zone.Direction != direction ||
                    !zone.ExecutionEligible ||
                    zone.Consumed ||
                    zone.Invalidated ||
                    zone.Lifecycle ==
                        ZoneLifecycle.Invalidated ||
                    zone.Lifecycle ==
                        ZoneLifecycle.Expired)
                    return false;
    
                return
                    zone.CurrentLower > 0 &&
                    zone.CurrentUpper >= zone.CurrentLower;
            }
    
            private EntryMode ResolvePendingEntryMode(
                StructureEventKind kind,
                ConfigSnapshot cfg)
            {
                if (kind ==
                        StructureEventKind.MarketStructureShift ||
                    kind ==
                        StructureEventKind.ChangeOfCharacter)
                    return EntryMode.ReversalLimit;
    
                PendingOrderMode pendingMode =
                    cfg.Get(
                        "PendingOrderMode",
                        PendingOrderMode.Adaptive);
    
                if (pendingMode ==
                        PendingOrderMode.ContinuationStop ||
                    pendingMode ==
                        PendingOrderMode.Both ||
                    pendingMode ==
                        PendingOrderMode.Adaptive)
                    return EntryMode.ContinuationStop;
    
                return EntryMode.BreakoutMarket;
            }
    
            private bool WithinMaximumEntryDistance(
                double executablePrice,
                double idealPrice,
                double atr,
                ConfigSnapshot cfg)
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
                ConfigSnapshot cfg)
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
                Direction direction,
                double price,
                double invalidation)
            {
                return direction == Direction.Buy
                    ? price <= invalidation
                    : price >= invalidation;
            }
    
            private DateTime ExpiryFor(
                DateTime sourceUtc,
                MarketFrame m5,
                ConfigSnapshot cfg)
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
    
            private EntrySnapshot Blocked(
                DecisionSnapshot decision,
                BlockReason reason,
                EntryTriggerState state,
                string rule)
            {
                return Blocked(
                    decision,
                    reason,
                    state,
                    rule,
                    null);
            }
    
            private EntrySnapshot Blocked(
                DecisionSnapshot decision,
                BlockReason primaryReason,
                EntryTriggerState state,
                string rule,
                IList<BlockReason> additional)
            {
                var blocks =
                    new List<BlockReason>();
    
                if (additional != null)
                {
                    for (int i = 0; i < additional.Count; i++)
                        if (!blocks.Contains(additional[i]))
                            blocks.Add(additional[i]);
                }
    
                if (!blocks.Contains(primaryReason))
                    blocks.Add(primaryReason);
    
                return new EntrySnapshot(
                    decision,
                    decision.Direction,
                    EntryMode.None,
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
                    Provenance.Direct(
                        "ENTRY_ENGINE",
                        rule));
            }
    
            private BlockReason FirstDecisionBlock(
                IReadOnlyList<BlockReason> blocks,
                BlockReason fallback)
            {
                if (blocks != null)
                {
                    for (int i = 0; i < blocks.Count; i++)
                        if (blocks[i] !=
                            BlockReason.None)
                            return blocks[i];
                }
    
                return fallback;
            }
    
            private BlockReason FirstBlock(
                IList<BlockReason> blocks,
                BlockReason fallback)
            {
                if (blocks != null)
                {
                    for (int i = 0; i < blocks.Count; i++)
                        if (blocks[i] !=
                            BlockReason.None)
                            return blocks[i];
                }
    
                return fallback;
            }
        }
    
        public sealed class TradeIdentity
        {
            public string StrategyId { get; private set; }
            public string Symbol { get; private set; }
            public string PlanId { get; private set; }
            public string SignalId { get; private set; }
    
            public TradeIdentity(
                string strategyId,
                string symbol,
                string planId,
                string signalId)
            {
                StrategyId = strategyId ?? string.Empty;
                Symbol = symbol ?? string.Empty;
                PlanId = planId ?? string.Empty;
                SignalId = signalId ?? string.Empty;
            }
        }
    
        public sealed class ExecutionIdentity
        {
            public string IntentId { get; private set; }
            public string IdempotencyKey { get; private set; }
            public DateTime CreatedUtc { get; private set; }
    
            public ExecutionIdentity(
                string intentId,
                string idempotencyKey,
                DateTime createdUtc)
            {
                IntentId = intentId ?? string.Empty;
                IdempotencyKey = idempotencyKey ?? string.Empty;
                CreatedUtc = createdUtc;
    
                if (string.IsNullOrWhiteSpace(IdempotencyKey))
                    throw new ArgumentException(
                        "IdempotencyKey is mandatory.",
                        "idempotencyKey");
            }
        }
    
        public sealed class TradePlan
        {
            public TradeIdentity Identity { get; private set; }
            public Direction Direction { get; private set; }
            public EntryMode EntryMode { get; private set; }
            public EntryModel Entry { get; private set; }
            public PriceLevel ExecutionAnchor { get; private set; }
            public PriceLevel StructuralStop { get; private set; }
            public TargetLadder TargetLadder { get; private set; }
            public double RiskRewardToTp1 { get; private set; }
            public double RiskRewardToFinalTarget { get; private set; }
            public int LevelQuality { get; private set; }
            public bool IsValid { get; private set; }
            public DateTime CreatedUtc { get; private set; }
            public int ReferenceBarIndex { get; private set; }
            public Provenance Provenance { get; private set; }
    
            public TradePlan(
                TradeIdentity identity,
                Direction direction,
                EntryMode entryMode,
                EntryModel entry,
                PriceLevel executionAnchor,
                PriceLevel structuralStop,
                TargetLadder targetLadder,
                double riskRewardToTp1,
                double riskRewardToFinalTarget,
                int levelQuality,
                bool isValid,
                DateTime createdUtc,
                int referenceBarIndex,
                Provenance provenance)
            {
                Identity =
                    identity ??
                    throw new ArgumentNullException("identity");
                Direction = direction;
                EntryMode = entryMode;
                Entry =
                    entry ??
                    throw new ArgumentNullException("entry");
                ExecutionAnchor =
                    executionAnchor ??
                    throw new ArgumentNullException("executionAnchor");
                StructuralStop =
                    structuralStop ??
                    throw new ArgumentNullException("structuralStop");
                TargetLadder =
                    targetLadder ??
                    throw new ArgumentNullException("targetLadder");
                RiskRewardToTp1 = Math.Max(0, riskRewardToTp1);
                RiskRewardToFinalTarget = Math.Max(0, riskRewardToFinalTarget);
                LevelQuality = Math.Max(0, Math.Min(100, levelQuality));
                IsValid = isValid;
    
                if (IsValid)
                {
                    if (!DirectionRules.IsDirectional(Direction))
                        throw new ArgumentException(
                            "A valid TradePlan must be directional.",
                            "direction");
    
                    if (Entry.Direction != Direction)
                        throw new ArgumentException(
                            "TradePlan direction must match Entry direction.",
                            "entry");
    
                    if (!DirectionRules.IsProtectivePrice(
                        Direction,
                        ExecutionAnchor.Price,
                        StructuralStop.Price))
                        throw new ArgumentException(
                            "TradePlan structural stop must protect the selected direction.",
                            "structuralStop");
    
                    if (!TargetLadder.ValidateForDirection(Direction, ExecutionAnchor.Price) ||
                        TargetLadder.Find(TargetStage.TP1) == null ||
                        RiskRewardToTp1 <= 0)
                        throw new ArgumentException(
                            "A valid TradePlan requires an ordered TP ladder with TP1 and positive RR.",
                            "targetLadder");
                }
    
                CreatedUtc = createdUtc;
                ReferenceBarIndex = referenceBarIndex;
                Provenance =
                    provenance ??
                    Provenance.Direct(
                        "UNKNOWN",
                        "UNSPECIFIED");
            }
        }
    
    public sealed class TradePlanBuilder :
        ITradePlanBuilder
    {
        private sealed class StopCandidate
        {
            public double Price;
            public int Quality;
            public double Distance;
            public bool Structural;
            public Provenance Provenance;
        }
    
        private sealed class TargetCandidate
        {
            public double Price;
            public int Quality;
            public bool Htf;
            public Provenance Provenance;
        }
    
        public TradePlan Build(
            DecisionSnapshot decision,
            EntrySnapshot entry,
            MarketModel market,
            StructureSnapshot structure,
            MtfSnapshot mtf,
            RuntimeSnapshot runtime,
            ConfigSnapshot configuration)
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
                    "PLAN_INPUT_INCOMPLETE");
    
            if (!decision.DecisionEligible ||
                entry.Model == null ||
                !DirectionRules.IsDirectional(decision.Direction))
                return InvalidPlan(
                    decision,
                    entry,
                    mtf,
                    runtime,
                    "DECISION_OR_ENTRY_MODEL_INVALID");
    
            bool marketEntryReady =
                entry.Eligible &&
                entry.State == EntryTriggerState.Ready &&
                (entry.Mode == EntryMode.RetestMarket ||
                 entry.Mode == EntryMode.BreakoutMarket);
    
            bool pendingProposal =
                !entry.Eligible &&
                entry.State == EntryTriggerState.WaitingBreakout &&
                (entry.Mode == EntryMode.ContinuationStop ||
                 entry.Mode == EntryMode.ReversalLimit) &&
                (entry.Mode == EntryMode.ReversalLimit ||
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
    
            MarketFrame m5 = market.FindFrame("M5");
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
                    decision.Direction == Direction.Buy
                        ? entryPrice - fallbackAtr * atr
                        : entryPrice + fallbackAtr * atr;
    
                stop = new StopCandidate
                {
                    Price = fallbackPrice,
                    Quality = 50,
                    Distance = Math.Abs(entryPrice - fallbackPrice),
                    Structural = false,
                    Provenance =
                        Provenance.FallbackFrom(
                            "RISK",
                            "ATR_STOP",
                            FallbackKind.Atr,
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
                    decision.Direction == Direction.Buy
                        ? entryPrice - minimumDistance
                        : entryPrice + minimumDistance;
                stop.Distance = minimumDistance;
                stop.Provenance =
                    stop.Structural
                        ? Provenance.Direct(
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
                    decision.Direction == Direction.Buy
                        ? entryPrice - boundedAtr * atr
                        : entryPrice + boundedAtr * atr;
    
                stop = new StopCandidate
                {
                    Price = fallbackPrice,
                    Quality = 50,
                    Distance = Math.Abs(entryPrice - fallbackPrice),
                    Structural = false,
                    Provenance =
                        Provenance.FallbackFrom(
                            "RISK",
                            "ATR_STOP_RISK_CAP",
                            FallbackKind.Atr,
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
                ladder.Find(TargetStage.TP1).Level.Price;
            TargetLevel finalTarget =
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
                new TradeIdentity(
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
    
            return new TradePlan(
                identity,
                decision.Direction,
                entry.Mode,
                entry.Model,
                new PriceLevel(
                    entryPrice,
                    "EXECUTION_ANCHOR",
                    Provenance.Direct(
                        "TRADE_PLAN",
                        "PLAN_EXECUTION_ANCHOR")),
                new PriceLevel(
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
                Provenance.Direct(
                    "TRADE_PLAN",
                    structuralStop
                        ? "STRUCTURAL_RISK_TARGET_PLAN"
                        : "EXPLICIT_STOP_FALLBACK_TARGET_PLAN"));
        }
    
        private double ResolvePlanEntryPrice(
            EntrySnapshot entry)
        {
            if (entry == null || entry.Model == null)
                return 0;
    
            if (entry.Mode == EntryMode.ContinuationStop &&
                entry.Model.Trigger != null)
                return entry.Model.Trigger.Price;
    
            if (entry.Mode == EntryMode.ReversalLimit)
                return entry.Model.IdealEntry.Price;
    
            if (entry.RequestedEntry != null)
                return entry.RequestedEntry.Price;
    
            return entry.Model.IdealEntry.Price;
        }
    
        private StopCandidate FindStructuralStop(
            Direction direction,
            double entryPrice,
            double atr,
            EntrySnapshot entry,
            StructureSnapshot structure,
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
                                Provenance.Direct(
                                    "ENTRY",
                                    "ENTRY_INVALIDATION")
                        });
            }
    
            for (int i = 0; i < structure.Zones.Count; i++)
            {
                ZoneRecord z = structure.Zones[i];
    
                if (z.Direction != direction ||
                    z.Invalidated ||
                    z.Consumed ||
                    z.Lifecycle == ZoneLifecycle.Expired ||
                    !string.Equals(
                        z.Timeframe,
                        "M5",
                        StringComparison.OrdinalIgnoreCase))
                    continue;
    
                double p =
                    direction == Direction.Buy
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
                            Provenance.Direct(
                                "STRUCTURE",
                                z.Kind == ZoneKind.FairValueGap
                                    ? "M5_FVG_STOP"
                                    : "M5_OB_STOP")
                    });
            }
    
            for (int i = 0; i < structure.Events.Count; i++)
            {
                StructureEventRecord e = structure.Events[i];
    
                bool swing =
                    direction == Direction.Buy
                        ? e.Kind == StructureEventKind.SwingLow
                        : e.Kind == StructureEventKind.SwingHigh;
    
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
                            (direction == Direction.Buy
                                ? stopBufferAtr * atr
                                : -stopBufferAtr * atr),
                        Quality = quality,
                        Distance = Math.Abs(
                            entryPrice -
                            (e.Price -
                                (direction == Direction.Buy
                                    ? stopBufferAtr * atr
                                    : -stopBufferAtr * atr))),
                        Structural = true,
                        Provenance =
                            Provenance.Direct(
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
    
        private TargetLadder BuildTargetLadder(
            DecisionSnapshot decision,
            double entryPrice,
            double stopPrice,
            double atr,
            StructureSnapshot structure,
            ConfigSnapshot configuration)
        {
            double risk = Math.Abs(entryPrice - stopPrice);
            if (risk <= 0 || atr <= 0)
                return new TargetLadder(
                    new List<TargetLevel>());
    
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
    
            var levels = new List<TargetLevel>();
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
                            decision.Direction == Direction.Buy
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
                                        Provenance.FallbackFrom(
                                            "TARGET",
                                            "SYNTHETIC_RR_TARGET",
                                            FallbackKind.Synthetic,
                                            "No suitable structural/liquidity target met the configured constraints.")
                                };
                        }
                    }
                }
    
                if (candidate == null)
                    break;
    
                TargetStage stage =
                    (TargetStage)(stageIndex + 1);
    
                levels.Add(
                    new TargetLevel(
                        stage,
                        new PriceLevel(
                            candidate.Price,
                            "TP" + (stageIndex + 1),
                            candidate.Provenance),
                        candidate.Quality,
                        TargetState.Proposed));
    
                htfTargetUsed =
                    htfTargetUsed || candidate.Htf;
                previous = candidate.Price;
            }
    
            // TP1 is mandatory. Higher stages are only published when their
            // configured RR/extension/obstacle constraints can be satisfied.
            if (levels.Count == 0)
                return new TargetLadder(
                    new List<TargetLevel>());
    
            return new TargetLadder(levels);
        }
    
        private TargetCandidate FindLiquidityTarget(
            Direction direction,
            double entryPrice,
            double previousTarget,
            double minimumDistance,
            double maximumDistance,
            double spacing,
            double clearance,
            StructureSnapshot structure,
            bool rejectObstacle)
        {
            TargetCandidate best = null;
    
            for (int i = 0; i < structure.Liquidity.Count; i++)
            {
                LiquidityRecord x = structure.Liquidity[i];
    
                bool directional =
                    direction == Direction.Buy
                        ? x.Price > entryPrice
                        : x.Price < entryPrice;
    
                if (!directional || x.Swept)
                    continue;
    
                double distance = Math.Abs(x.Price - entryPrice);
                if (distance < minimumDistance ||
                    distance > maximumDistance)
                    continue;
    
                bool progressesBeyondPrevious =
                    direction == Direction.Buy
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
                                Provenance.Direct(
                                    x.Timeframe,
                                    "LIQUIDITY_TARGET_" +
                                    x.Kind.ToString().ToUpperInvariant())
                        };
                }
            }
    
            return best;
        }
    
        private bool HasObstacle(
            Direction direction,
            double entryPrice,
            double targetPrice,
            double clearance,
            StructureSnapshot structure)
        {
            double lower =
                Math.Min(entryPrice, targetPrice) + clearance;
            double upper =
                Math.Max(entryPrice, targetPrice) - clearance;
    
            if (upper <= lower)
                return false;
    
            Direction opposing =
                direction == Direction.Buy
                    ? Direction.Sell
                    : Direction.Buy;
    
            for (int i = 0; i < structure.Zones.Count; i++)
            {
                ZoneRecord z = structure.Zones[i];
    
                if (z.Direction != opposing ||
                    z.Invalidated ||
                    z.Consumed ||
                    z.Lifecycle == ZoneLifecycle.Expired)
                    continue;
    
                if (z.CurrentUpper >= lower &&
                    z.CurrentLower <= upper)
                    return true;
            }
    
            return false;
        }
    
        private bool HasHtfTarget(
            TargetLadder ladder)
        {
            for (int i = 0; i < ladder.Levels.Count; i++)
            {
                Provenance p =
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
            Direction direction,
            double entryPrice,
            double stopPrice)
        {
            return
                direction == Direction.Buy
                    ? stopPrice < entryPrice
                    : direction == Direction.Sell &&
                      stopPrice > entryPrice;
        }
    
        private string BuildPlanId(
            DateTime referenceUtc,
            Direction direction,
            double entryPrice,
            double stopPrice)
        {
            return
                "CFIP|PLAN|" +
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
            Direction direction)
        {
            return
                "CFIP|SIGNAL|" +
                referenceUtc.Ticks.ToString() +
                "|" +
                direction.ToString();
        }
    
        private TradePlan InvalidPlan(
            DecisionSnapshot decision,
            EntrySnapshot entry,
            MtfSnapshot mtf,
            RuntimeSnapshot runtime,
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
    
            Direction direction =
                decision != null
                    ? decision.Direction
                    : Direction.Wait;
    
            EntryModel entryModel =
                entry != null && entry.Model != null
                    ? entry.Model
                    : new EntryModel(
                        direction == Direction.Wait
                            ? Direction.Buy
                            : direction,
                        new PriceLevel(
                            runtime != null && runtime.Bid > 0
                                ? runtime.Bid
                                : 1,
                            "InvalidPlanEntry",
                            Provenance.Direct(
                                "TRADE_PLAN",
                                "INVALID_PLAN")),
                        new PriceZone(
                            runtime != null && runtime.Bid > 0
                                ? runtime.Bid
                                : 1,
                            runtime != null && runtime.Bid > 0
                                ? runtime.Bid
                                : 1,
                            "InvalidPlanZone",
                            Provenance.Direct(
                                "TRADE_PLAN",
                                "INVALID_PLAN")),
                        null,
                        new PriceLevel(
                            runtime != null && runtime.Bid > 0
                                ? runtime.Bid
                                : 1,
                            "InvalidPlanInvalidation",
                            Provenance.Direct(
                                "TRADE_PLAN",
                                "INVALID_PLAN")));
    
            double safeEntry =
                entryModel.IdealEntry.Price;
            var identity =
                new TradeIdentity(
                    "CFIP-PRO-",
                    runtime != null ? runtime.Symbol : string.Empty,
                    "INVALID|" + reference.Ticks.ToString(),
                    "INVALID|" + reason);
    
            var emptyLadder =
                new TargetLadder(
                    new List<TargetLevel>());
    
            return new TradePlan(
                identity,
                direction,
                entry != null
                    ? entry.Mode
                    : EntryMode.None,
                entryModel,
                new PriceLevel(
                    safeEntry,
                    "InvalidPlanExecutionAnchor",
                    Provenance.Direct(
                        "TRADE_PLAN",
                        "INVALID_PLAN")),
                new PriceLevel(
                    safeEntry,
                    "InvalidPlanStop",
                    Provenance.FallbackFrom(
                        "TRADE_PLAN",
                        reason,
                        FallbackKind.None,
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
                Provenance.Direct(
                    "TRADE_PLAN",
                    reason));
        }
    }
    
        public interface IEntryTriggerEngine
        {
            EntrySnapshot Evaluate(
                DecisionSnapshot decision,
                RuntimeSnapshot runtime,
                MtfSnapshot mtf,
                MarketModel market,
                StructureSnapshot structure,
                ConfigSnapshot configuration);
        }
    
        public interface ITradePlanBuilder
        {
            TradePlan Build(
                DecisionSnapshot decision,
                EntrySnapshot entry,
                MarketModel market,
                StructureSnapshot structure,
                MtfSnapshot mtf,
                RuntimeSnapshot runtime,
                ConfigSnapshot configuration);
        }
    
    
}
