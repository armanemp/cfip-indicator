using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CFIP.Indicator
{
        public sealed class DecisionSnapshot
        {
            private readonly ReadOnlyCollection<BlockReason> _blockReasons;
    
            public Direction Direction { get; private set; }
            public int Confidence { get; private set; }
            public int Quality { get; private set; }
            public int Edge { get; private set; }
            public int MtfAgreement { get; private set; }
            public int IndependentEvidence { get; private set; }
            public int StructuralConfirmations { get; private set; }
            public string Regime { get; private set; }
            public int RegimeQuality { get; private set; }
    
            // Decision-level eligibility. Entry/Trigger eligibility belongs to .
            public bool DecisionEligible { get; private set; }
    
            public DecisionPolicyMode PolicyMode { get; private set; }
            public IReadOnlyList<BlockReason> BlockReasons
            {
                get { return _blockReasons; }
            }
            public Provenance Provenance { get; private set; }
    
            public DecisionSnapshot(
                Direction direction,
                int confidence,
                int quality,
                int edge,
                int mtfAgreement,
                int independentEvidence,
                int structuralConfirmations,
                string regime,
                int regimeQuality,
                bool decisionEligible,
                DecisionPolicyMode policyMode,
                IList<BlockReason> blockReasons,
                Provenance provenance)
            {
                Direction = direction;
                Confidence = Math.Max(0, Math.Min(100, confidence));
                Quality = Math.Max(0, Math.Min(100, quality));
                Edge = Math.Max(-100, Math.Min(100, edge));
                MtfAgreement = Math.Max(0, Math.Min(100, mtfAgreement));
                IndependentEvidence = Math.Max(0, independentEvidence);
                StructuralConfirmations = Math.Max(0, structuralConfirmations);
                Regime = regime ?? string.Empty;
                RegimeQuality = Math.Max(0, Math.Min(100, regimeQuality));
                DecisionEligible = decisionEligible;
                PolicyMode = policyMode;
    
                var normalizedBlocks =
                    new List<BlockReason>(
                        blockReasons ??
                        new List<BlockReason>());
    
                if (DecisionEligible &&
                    (Direction == Direction.Wait ||
                     normalizedBlocks.Count > 0))
                    throw new ArgumentException(
                        "Eligible decision cannot be WAIT or blocked.",
                        "decisionEligible");
    
                if (Direction == Direction.Wait &&
                    !normalizedBlocks.Contains(BlockReason.NoDirection))
                    throw new ArgumentException(
                        "WAIT decision requires a NoDirection block.",
                        "decision");
    
                if (Direction != Direction.Wait &&
                    !DecisionEligible &&
                    normalizedBlocks.Count == 0)
                    throw new ArgumentException(
                        "Blocked directional decision requires at least one block.",
                        "decisionEligible");
    
                if (!DecisionEligible &&
                    (PolicyMode == DecisionPolicyMode.Confirmed ||
                     PolicyMode == DecisionPolicyMode.Aggressive))
                    throw new ArgumentException(
                        "Confirmed/Aggressive policy requires decision eligibility.",
                        "policyMode");
    
                if (DecisionEligible &&
                    PolicyMode != DecisionPolicyMode.Confirmed &&
                    PolicyMode != DecisionPolicyMode.Aggressive)
                    throw new ArgumentException(
                        "Eligible decision requires Confirmed or Aggressive policy.",
                        "policyMode");
    
                if (PolicyMode == DecisionPolicyMode.Pending &&
                    (Direction == Direction.Wait ||
                     normalizedBlocks.Count == 0))
                    throw new ArgumentException(
                        "Pending policy requires a directional blocked setup.",
                        "policyMode");
    
                _blockReasons =
                    new ReadOnlyCollection<BlockReason>(
                        normalizedBlocks);
                Provenance =
                    provenance ??
                    Provenance.Direct(
                        "UNKNOWN",
                        "UNSPECIFIED");
            }
        }
    
        public sealed class DecisionEngine : IDecisionEngine
        {
            private sealed class FeatureAggregate
            {
                public bool Seen;
                public double BullScore;
                public double BearScore;
            }
    
            private sealed class ZoneAggregate
            {
                public int Quality;
                public bool Retested;
                public bool LiquidityConfluence;
                public bool FvgConfluence;
            }
    
            private sealed class LiquidityAggregate
            {
                public int Quality;
            }
    
            private sealed class Evidence
            {
                public double Bull;
                public double Bear;
                public int Independent;
                public int Structural;
                public int BullStructuralConfirmations;
                public int BearStructuralConfirmations;
                public int BullStructure;
                public int BearStructure;
                public int BullZone;
                public int BearZone;
                public int BullLiquidity;
                public int BearLiquidity;
                public int BullConfluence;
                public int BearConfluence;
                public int BullRetest;
                public int BearRetest;
            }
    
            public DecisionSnapshot Evaluate(
                RuntimeSnapshot runtime,
                MtfSnapshot mtf,
                MarketModel market,
                StructureSnapshot structure,
                ConfigSnapshot configuration)
            {
                var blocks = new List<BlockReason>();
    
                if (runtime == null || mtf == null || market == null ||
                    structure == null || configuration == null)
                    return Blocked(
                        BlockReason.DataIncomplete,
                        "MISSING_INPUT");
    
                if (!mtf.IsPrimaryDecisionReady ||
                    !MtfSnapshotBuilder.IsCoherent(mtf) ||
                    !market.IsCoherent || !market.IsPrimaryReady ||
                    !structure.IsCoherent || !structure.IsPrimaryReady)
                    return Blocked(
                        BlockReason.DataIncomplete,
                        "SNAPSHOT_NOT_READY");
    
                MarketFrame m5 = market.FindFrame("M5");
                string regime =
                    m5 == null
                        ? "UNKNOWN"
                        : m5.Regime.ToString();
    
                int adaptiveQualityThreshold;
                int adaptiveShareThreshold;
                int adaptiveEdgeThreshold;
    
                GetAdaptiveSmartThresholds(
                    regime,
                    configuration,
                    out adaptiveQualityThreshold,
                    out adaptiveShareThreshold,
                    out adaptiveEdgeThreshold);
    
                Evidence e = CollectEvidence(
                    market,
                    structure,
                    configuration);
    
                int bullShare;
                int bearShare;
    
                CalculateDirectionalShares(
                    e.Bull,
                    e.Bear,
                    configuration.Get("SmartScoreTemperature", 12.0),
                    out bullShare,
                    out bearShare);
    
                int strongestShare = Math.Max(bullShare, bearShare);
    
                Direction direction =
                    e.Bull <= 0 &&
                    e.Bear <= 0
                        ? Direction.Wait
                        : strongestShare < adaptiveShareThreshold
                            ? Direction.Wait
                            : bullShare > bearShare
                                ? Direction.Buy
                                : bearShare > bullShare
                                    ? Direction.Sell
                                    : Direction.Wait;
    
                int edge = Math.Abs(bullShare - bearShare);
    
                int mtfAgreement =
                    CalculateMtfAgreement(
                        direction,
                        market,
                        configuration);
    
                int marketQuality = m5 == null ? 0 : m5.MarketQuality;
                int regimeQuality = m5 == null ? 0 : m5.RegimeQuality;
    
                // Structural confirmations belong to the selected direction only.
                // Opposing confirmations must never satisfy the selected direction's gate.
                e.Structural =
                    direction == Direction.Buy
                        ? e.BullStructuralConfirmations
                        : direction == Direction.Sell
                            ? e.BearStructuralConfirmations
                            : 0;
    
                int structureQuality =
                    direction == Direction.Buy
                        ? e.BullStructure
                        : direction == Direction.Sell
                            ? e.BearStructure : 0;
    
                int zoneQuality =
                    direction == Direction.Buy
                        ? e.BullZone
                        : direction == Direction.Sell
                            ? e.BearZone : 0;
    
                int liquidityQuality =
                    direction == Direction.Buy
                        ? e.BullLiquidity
                        : direction == Direction.Sell
                            ? e.BearLiquidity : 0;
    
                int confluenceQuality =
                    configuration.Get("UseAdvancedConfluence", true)
                        ? direction == Direction.Buy
                            ? e.BullConfluence
                            : direction == Direction.Sell
                                ? e.BearConfluence : 0
                        : 0;
    
                int retestQuality =
                    direction == Direction.Buy
                        ? e.BullRetest
                        : direction == Direction.Sell
                            ? e.BearRetest : 0;
    
                // Weighted domains intentionally sum to 1.00. Structure, zone,
                // liquidity and retest are not allowed to independently recreate
                // the same directional indicator score.
                int quality = Clamp((int)Math.Round(
                    marketQuality * 0.28 +
                    mtfAgreement * 0.18 +
                    structureQuality * 0.18 +
                    zoneQuality * 0.12 +
                    liquidityQuality * 0.07 +
                    confluenceQuality * 0.05 +
                    retestQuality * 0.06 +
                    regimeQuality * 0.06), 0, 100);
    
                int confidence = Clamp(
                    (int)Math.Round(quality * 0.70 + (50 + edge) * 0.30),
                    0, 100);
    
                confidence = ApplyHigherTimeframePenalty(
                    confidence,
                    direction,
                    market,
                    configuration);
    
                if (direction == Direction.Wait)
                {
                    // A neutral directional consensus is the root decision block.
                    // Direction-dependent gates are not meaningful until a direction
                    // exists, so they must not pollute the exact block-reason ledger.
                    blocks.Add(BlockReason.NoDirection);
                }
                else
                {
                    if (confidence < configuration.Get("MinimumConfidence", 72))
                        blocks.Add(BlockReason.ConfidenceTooLow);
        
                    if (edge < adaptiveEdgeThreshold)
                        blocks.Add(BlockReason.EvidenceInsufficient);
        
                    if (quality < adaptiveQualityThreshold)
                        blocks.Add(BlockReason.PolicyBlocked);
        
                    if (mtfAgreement < configuration.Get("MinimumTimeframeAgreement", 72) &&
                        configuration.Get("RequireHigherTfAgreement", true))
                        blocks.Add(BlockReason.MtfDisagreement);
        
                    int requiredEvidence = Math.Max(
                        configuration.Get("MinimumIndependentEvidence", 4),
                        configuration.Get("EnableSmartDecisionEngine", true)
                            ? configuration.Get("SmartMinimumIndependentEvidence", 4) : 0);
        
                    if (e.Independent < requiredEvidence)
                        blocks.Add(BlockReason.EvidenceInsufficient);
        
                    if (configuration.Get("RequireStructuralConfirmation", true) &&
                        e.Structural < configuration.Get("MinimumStructuralConfirmations", 4))
                        blocks.Add(BlockReason.StructureInvalid);
        
                    if (configuration.Get("RequireCoreAgreement", true) &&
                        !CoreAgreement(direction, market, configuration))
                        blocks.Add(BlockReason.MtfDisagreement);
        
                    if (configuration.Get("EnableSmartDecisionEngine", true))
                    {
                        if (configuration.Get("RequireSmartConsensus", true) &&
                            strongestShare <
                            Math.Max(
                                configuration.Get("SmartConsensusThreshold", 57),
                                adaptiveShareThreshold))
                        {
                            bool soft =
                                configuration.Get("AllowSmartSoftGate", true) &&
                                quality >= configuration.Get("SmartStrongSetupQuality", 82) &&
                                edge >= configuration.Get("SmartStrongSetupEdge", 10) &&
                                e.Independent >= requiredEvidence + 1;
        
                            if (!soft)
                                blocks.Add(BlockReason.PolicyBlocked);
                        }
        
                        if (mtfAgreement <
                            configuration.Get("SmartMinimumTimeframeAgreement", 72))
                            blocks.Add(BlockReason.MtfDisagreement);
                    }
        
                    // Legacy name retained for preset parity. In  this is
                    // only a Decision-quality policy floor; actual Entry/Trigger eligibility
                    // remains exclusively owned by .
                    if (configuration.Get("UseSmartEntryQualityFilter", true) &&
                        quality < Math.Max(
                            configuration.Get("SmartQualityThreshold", 70),
                            Math.Max(
                                adaptiveQualityThreshold,
                                configuration.Get("EnableSmartDecisionEngine", true)
                                    ? SmartMinimumConsensusFloor(configuration)
                                    : 0)))
                        blocks.Add(BlockReason.PolicyBlocked);
        
                    if (configuration.Get("UseRegimeNoTradeGuard", true) &&
                        RegimeBlocked(regime, regimeQuality, configuration))
                        blocks.Add(BlockReason.VolatilityBlocked);
        
                    if (configuration.Get("UseHistoricalChoppinessGuard", true) &&
                        m5 != null && m5.Choppy &&
                        market.M15 != null && market.M15.Choppy &&
                        quality < Math.Max(
                            configuration.Get("SmartRegimeQualityFloor", 55) + 5,
                            configuration.Get("NoTradeMinimumSmartQuality", 55) + 5))
                        blocks.Add(BlockReason.VolatilityBlocked);
        
        
                }
    
                blocks = Distinct(blocks);
    
                bool eligible =
                    direction != Direction.Wait &&
                    blocks.Count == 0;
    
                DecisionPolicyMode policy =
                    ResolvePolicy(
                        eligible,
                        direction,
                        confidence,
                        quality,
                        e.Independent,
                        blocks,
                        configuration);
    
                return new DecisionSnapshot(
                    direction, confidence, quality, edge, mtfAgreement,
                    e.Independent, e.Structural, regime, regimeQuality,
                    eligible, policy, blocks,
                    Provenance.Direct(
                        "DECISION_ENGINE",
                        eligible ? "DECISION_ELIGIBLE" : "DECISION_BLOCKED"));
            }
    
            private Evidence CollectEvidence(
                MarketModel market,
                StructureSnapshot structure,
                ConfigSnapshot configuration)
            {
                var e = new Evidence();
    
                // Directional market evidence is deduplicated by feature family
                // across timeframes. MTF agreement is a separate semantic domain.
                AddMarketEvidence(
                    e,
                    market.FindFrame("M5"),
                    market.FindFrame("M15"),
                    market.FindFrame("M30"),
                    market.FindFrame("H1"),
                    market.FindFrame("H4"),
                    market.FindFrame("D1"),
                    market.FindFrame("W1"));
    
                bool bullM5Break = false;
                bool bullM5Displacement = false;
                bool bullM15Break = false;
                bool bullH1Break = false;
                bool bullH4Break = false;
    
                bool bearM5Break = false;
                bool bearM5Displacement = false;
                bool bearM15Break = false;
                bool bearH1Break = false;
                bool bearH4Break = false;
    
                int bullDisplacement = 0;
                int bearDisplacement = 0;
    
                for (int i = 0; i < structure.Events.Count; i++)
                {
                    StructureEventRecord x = structure.Events[i];
                    if (x == null || x.Direction == Direction.Wait)
                        continue;
    
                    bool isStructureBreak =
                        x.Kind == StructureEventKind.BreakOfStructure ||
                        x.Kind == StructureEventKind.MarketStructureShift ||
                        x.Kind == StructureEventKind.ChangeOfCharacter;
    
                    bool isM5 = string.Equals(
                        x.Timeframe, "M5", StringComparison.OrdinalIgnoreCase);
                    bool isM15 = string.Equals(
                        x.Timeframe, "M15", StringComparison.OrdinalIgnoreCase);
                    bool isH1 = string.Equals(
                        x.Timeframe, "H1", StringComparison.OrdinalIgnoreCase);
                    bool isH4 = string.Equals(
                        x.Timeframe, "H4", StringComparison.OrdinalIgnoreCase);
    
                    if (x.Direction == Direction.Buy)
                    {
                        if (isStructureBreak)
                        {
                            e.BullStructure = Math.Max(e.BullStructure, x.Quality);
    
                            if (isM5) bullM5Break = true;
                            else if (isM15) bullM15Break = true;
                            else if (isH1) bullH1Break = true;
                            else if (isH4) bullH4Break = true;
                        }
                        else if (x.Kind == StructureEventKind.Displacement)
                        {
                            bullDisplacement = Math.Max(bullDisplacement, x.Quality);
                            if (isM5) bullM5Displacement = true;
                        }
                    }
                    else
                    {
                        if (isStructureBreak)
                        {
                            e.BearStructure = Math.Max(e.BearStructure, x.Quality);
    
                            if (isM5) bearM5Break = true;
                            else if (isM15) bearM15Break = true;
                            else if (isH1) bearH1Break = true;
                            else if (isH4) bearH4Break = true;
                        }
                        else if (x.Kind == StructureEventKind.Displacement)
                        {
                            bearDisplacement = Math.Max(bearDisplacement, x.Quality);
                            if (isM5) bearM5Displacement = true;
                        }
                    }
                }
    
                // Preserve the  confirmation concept without treating BOS/MSS/CHOCH
                // as three independent confirmations. A structural-break family counts
                // once per timeframe; M5 displacement remains a distinct confirmation.
                e.BullStructuralConfirmations =
                    (bullM5Break ? 1 : 0) +
                    (bullM5Displacement ? 1 : 0) +
                    (bullM15Break ? 1 : 0) +
                    (bullH1Break ? 1 : 0) +
                    (bullH4Break ? 1 : 0);
    
                e.BearStructuralConfirmations =
                    (bearM5Break ? 1 : 0) +
                    (bearM5Displacement ? 1 : 0) +
                    (bearM15Break ? 1 : 0) +
                    (bearH1Break ? 1 : 0) +
                    (bearH4Break ? 1 : 0);
    
                e.Bull += e.BullStructuralConfirmations > 0
                    ? e.BullStructure * 0.10
                    : 0;
    
                e.Bear += e.BearStructuralConfirmations > 0
                    ? e.BearStructure * 0.10
                    : 0;
    
                if (bullDisplacement > 0)
                    e.Bull += bullDisplacement * 0.10;
    
                if (bearDisplacement > 0)
                    e.Bear += bearDisplacement * 0.10;
    
                // Zones are deduplicated by zone family (FVG/OB) per direction.
                // Multiple timeframes or repeated instances of the same family cannot
                // manufacture directional confidence through record count.
                var bullZones =
                    new Dictionary<ZoneKind, ZoneAggregate>();
                var bearZones =
                    new Dictionary<ZoneKind, ZoneAggregate>();
    
                for (int i = 0; i < structure.Zones.Count; i++)
                {
                    ZoneRecord z = structure.Zones[i];
                    if (z == null || z.Direction == Direction.Wait ||
                        z.Invalidated || z.Consumed || !z.ExecutionEligible)
                        continue;
    
                    Dictionary<ZoneKind, ZoneAggregate> target =
                        z.Direction == Direction.Buy
                            ? bullZones
                            : bearZones;
    
                    ZoneAggregate aggregate;
                    if (!target.TryGetValue(z.Kind, out aggregate))
                    {
                        aggregate = new ZoneAggregate();
                        target.Add(z.Kind, aggregate);
                    }
    
                    if (z.Quality > aggregate.Quality)
                    {
                        aggregate.Quality = z.Quality;
                        aggregate.Retested = z.Retested;
                        aggregate.LiquidityConfluence = z.LiquidityConfluence;
                        aggregate.FvgConfluence = z.FvgConfluence;
                    }
                    else if (z.Quality == aggregate.Quality)
                    {
                        aggregate.Retested =
                            aggregate.Retested || z.Retested;
                        aggregate.LiquidityConfluence =
                            aggregate.LiquidityConfluence ||
                            z.LiquidityConfluence;
                        aggregate.FvgConfluence =
                            aggregate.FvgConfluence ||
                            z.FvgConfluence;
                    }
                }
    
                foreach (KeyValuePair<ZoneKind, ZoneAggregate> pair in bullZones)
                {
                    ZoneAggregate aggregate = pair.Value;
                    if (aggregate == null || aggregate.Quality <= 0)
                        continue;
    
                    e.Bull += aggregate.Quality * 0.08;
                    e.BullZone = Math.Max(e.BullZone, aggregate.Quality);
                    if (aggregate.Retested)
                        e.BullRetest = Math.Max(e.BullRetest, aggregate.Quality);
                    if (aggregate.LiquidityConfluence || aggregate.FvgConfluence)
                        e.BullConfluence = Math.Max(
                            e.BullConfluence,
                            aggregate.Quality);
                }
    
                foreach (KeyValuePair<ZoneKind, ZoneAggregate> pair in bearZones)
                {
                    ZoneAggregate aggregate = pair.Value;
                    if (aggregate == null || aggregate.Quality <= 0)
                        continue;
    
                    e.Bear += aggregate.Quality * 0.08;
                    e.BearZone = Math.Max(e.BearZone, aggregate.Quality);
                    if (aggregate.Retested)
                        e.BearRetest = Math.Max(e.BearRetest, aggregate.Quality);
                    if (aggregate.LiquidityConfluence || aggregate.FvgConfluence)
                        e.BearConfluence = Math.Max(
                            e.BearConfluence,
                            aggregate.Quality);
                }
    
                // Liquidity sweeps are deduplicated by liquidity-pool family. This
                // preserves distinct pools (PDH, PDL, equal highs, swings, etc.) while
                // preventing repeated timeframe records from dominating the score.
                var bullLiquidity =
                    new Dictionary<LiquidityKind, LiquidityAggregate>();
                var bearLiquidity =
                    new Dictionary<LiquidityKind, LiquidityAggregate>();
    
                for (int i = 0; i < structure.Liquidity.Count; i++)
                {
                    LiquidityRecord l = structure.Liquidity[i];
                    if (l == null || !l.Swept ||
                        l.SweepDirection == Direction.Wait)
                        continue;
    
                    Dictionary<LiquidityKind, LiquidityAggregate> target =
                        l.SweepDirection == Direction.Buy
                            ? bullLiquidity
                            : bearLiquidity;
    
                    LiquidityAggregate aggregate;
                    if (!target.TryGetValue(l.Kind, out aggregate))
                    {
                        aggregate = new LiquidityAggregate();
                        target.Add(l.Kind, aggregate);
                    }
    
                    aggregate.Quality =
                        Math.Max(
                            aggregate.Quality,
                            l.Quality);
                }
    
                foreach (KeyValuePair<LiquidityKind, LiquidityAggregate> pair in bullLiquidity)
                {
                    LiquidityAggregate aggregate = pair.Value;
                    if (aggregate == null || aggregate.Quality <= 0)
                        continue;
    
                    e.Bull += aggregate.Quality * 0.06;
                    e.BullLiquidity = Math.Max(
                        e.BullLiquidity,
                        aggregate.Quality);
                }
    
                foreach (KeyValuePair<LiquidityKind, LiquidityAggregate> pair in bearLiquidity)
                {
                    LiquidityAggregate aggregate = pair.Value;
                    if (aggregate == null || aggregate.Quality <= 0)
                        continue;
    
                    e.Bear += aggregate.Quality * 0.06;
                    e.BearLiquidity = Math.Max(
                        e.BearLiquidity,
                        aggregate.Quality);
                }
    
                if (configuration.Get("UsePremiumDiscount", true) &&
                    structure.PremiumDiscount != null &&
                    structure.PremiumDiscount.Available)
                {
                    // Preserve the  location bias: discount supports BUY,
                    // premium supports SELL. This affects directional score only;
                    // it is not an independent-evidence count.
                    if (structure.PremiumDiscount.IsDiscount)
                        e.Bull += 6.0;
                    else if (structure.PremiumDiscount.IsPremium)
                        e.Bear += 6.0;
                }
    
                // Confluence is a quality modifier, not another directional evidence
                // source. It is consumed only by the weighted quality domain.
                return e;
            }
    
            private void AddMarketEvidence(
                Evidence e,
                params MarketFrame[] frames)
            {
                if (e == null || frames == null)
                    return;
    
                var families =
                    new Dictionary<MarketFeature, FeatureAggregate>();
    
                for (int i = 0; i < frames.Length; i++)
                {
                    MarketFrame frame = frames[i];
                    if (frame == null || !frame.DataValid)
                        continue;
    
                    for (int j = 0; j < frame.Features.Count; j++)
                    {
                        FeatureEvidence feature = frame.Features[j];
                        if (feature == null ||
                            !feature.Triggered ||
                            !feature.CountsAsEvidence ||
                            feature.Direction == Direction.Wait)
                            continue;
    
                        FeatureAggregate aggregate;
                        if (!families.TryGetValue(feature.Feature, out aggregate))
                        {
                            aggregate = new FeatureAggregate();
                            families.Add(feature.Feature, aggregate);
                        }
    
                        aggregate.Seen = true;
    
                        // Feature.Value is normalized to [0,1], while Weight is
                        // the market-model semantic weight. Keep that weighting
                        // and independently retain the strongest BUY and SELL
                        // observation for the feature family across timeframes.
                        double score = feature.Value * Math.Max(0, feature.Weight);
    
                        if (feature.Direction == Direction.Buy)
                            aggregate.BullScore = Math.Max(
                                aggregate.BullScore,
                                score);
                        else if (feature.Direction == Direction.Sell)
                            aggregate.BearScore = Math.Max(
                                aggregate.BearScore,
                                score);
                    }
                }
    
                foreach (KeyValuePair<MarketFeature, FeatureAggregate> pair in families)
                {
                    FeatureAggregate aggregate = pair.Value;
                    if (aggregate == null || !aggregate.Seen)
                        continue;
    
                    // A market feature family contributes once. Keep the stronger
                    // directional observation; an exact tie is neutral rather than
                    // inheriting the answer from timeframe iteration order.
                    if (aggregate.BullScore > aggregate.BearScore &&
                        aggregate.BullScore > 0)
                    {
                        e.Bull += aggregate.BullScore;
                        e.Independent++;
                    }
                    else if (aggregate.BearScore > aggregate.BullScore &&
                             aggregate.BearScore > 0)
                    {
                        e.Bear += aggregate.BearScore;
                        e.Independent++;
                    }
                }
            }
    
            private int ApplyHigherTimeframePenalty(
                int confidence,
                Direction direction,
                MarketModel market,
                ConfigSnapshot cfg)
            {
                if (direction == Direction.Wait ||
                    market == null)
                    return confidence;
    
                int basePenalty = Math.Max(
                    0,
                    cfg.Get("HigherTfPenalty", 7));
    
                if (basePenalty == 0)
                    return confidence;
    
                bool h1Against = IsAgainst(
                    market.FindFrame("H1"),
                    direction);
    
                bool h4Against = IsAgainst(
                    market.FindFrame("H4"),
                    direction);
    
                bool d1Against = IsAgainst(
                    market.FindFrame("D1"),
                    direction);
    
                if (!h1Against && !h4Against && !d1Against)
                    return confidence;
    
                int penalty =
                    basePenalty +
                    (d1Against
                        ? basePenalty / 2
                        : 0);
    
                return Clamp(
                    confidence - penalty,
                    0,
                    100);
            }
    
            private bool IsAgainst(
                MarketFrame frame,
                Direction direction)
            {
                return frame != null &&
                       frame.DataValid &&
                       frame.BiasDirection != Direction.Wait &&
                       frame.BiasDirection != direction;
            }
    
            private void CalculateDirectionalShares(
                double bull,
                double bear,
                double temperature,
                out int bullShare,
                out int bearShare)
            {
                double safeTemperature = Math.Max(1.0, temperature);
                double centered = (bull - bear) / safeTemperature;
    
                double expBull = Math.Exp(
                    Clamp(
                        centered,
                        -12,
                        12));
    
                double expBear = Math.Exp(
                    Clamp(
                        -centered,
                        -12,
                        12));
    
                double total =
                    Math.Max(
                        1e-9,
                        expBull + expBear);
    
                bullShare = Clamp(
                    (int)Math.Round(
                        100.0 * expBull / total),
                    0,
                    100);
    
                bearShare = 100 - bullShare;
            }
    
            private void GetAdaptiveSmartThresholds(
                string regime,
                ConfigSnapshot cfg,
                out int qualityThreshold,
                out int shareThreshold,
                out int edgeThreshold)
            {
                qualityThreshold = Math.Max(
                    40,
                    Math.Min(
                        95,
                        cfg.Get("MinimumSmartQuality", 70)));
    
                shareThreshold = Math.Max(
                    50,
                    Math.Min(
                        90,
                        cfg.Get("MinimumSmartDirectionShare", 57)));
    
                edgeThreshold = Math.Max(
                    4,
                    Math.Min(
                        30,
                        cfg.Get("MinimumEdge", 15)));
    
                if (!cfg.Get("AdaptiveSmartThresholds", true))
                    return;
    
                int buffer = Math.Max(
                    0,
                    cfg.Get("SmartRegimeBuffer", 6));
    
                switch (regime ?? "UNKNOWN")
                {
                    case "TREND":
                    case "Trend":
                    case "EXPANSION":
                    case "Expansion":
                        qualityThreshold -= buffer;
                        shareThreshold -= Math.Max(1, buffer / 3);
                        edgeThreshold -= Math.Max(1, buffer / 3);
                        break;
    
                    case "REVERSAL":
                    case "Reversal":
                        qualityThreshold -= Math.Max(1, buffer / 2);
                        break;
    
                    case "RANGE":
                    case "Range":
                        qualityThreshold += Math.Max(1, buffer / 2);
                        shareThreshold += Math.Max(1, buffer / 3);
                        edgeThreshold += Math.Max(1, buffer / 3);
                        break;
    
                    case "TRANSITION":
                    case "Transition":
                        qualityThreshold += Math.Max(1, buffer / 2);
                        shareThreshold += Math.Max(1, buffer / 3);
                        edgeThreshold += Math.Max(1, buffer / 3);
                        break;
    
                    case "COMPRESSION":
                    case "Compression":
                        qualityThreshold += buffer;
                        shareThreshold += Math.Max(1, buffer / 2);
                        edgeThreshold += Math.Max(1, buffer / 2);
                        break;
                }
    
                qualityThreshold = Math.Max(
                    40,
                    Math.Min(
                        95,
                        qualityThreshold));
    
                shareThreshold = Math.Max(
                    50,
                    Math.Min(
                        90,
                        shareThreshold));
    
                edgeThreshold = Math.Max(
                    4,
                    Math.Min(
                        30,
                        edgeThreshold));
            }
    
            private int SmartMinimumConsensusFloor(
                ConfigSnapshot cfg)
            {
                return Math.Max(
                    40,
                    cfg.Get("SmartConsensusThreshold", 57) - 12);
            }
    
            private int CalculateMtfAgreement(
                Direction direction,
                MarketModel market,
                ConfigSnapshot cfg)
            {
                if (direction == Direction.Wait) return 0;
                double total = 0, aligned = 0;
    
                AddMtf(market.FindFrame("M5"), cfg.Get("M5Weight", 7), direction, ref total, ref aligned);
                AddMtf(market.FindFrame("M15"), cfg.Get("M15Weight", 8), direction, ref total, ref aligned);
                AddMtf(market.FindFrame("M30"), cfg.Get("M30Weight", 5), direction, ref total, ref aligned);
                AddMtf(market.FindFrame("H1"), cfg.Get("H1Weight", 3), direction, ref total, ref aligned);
                AddMtf(market.FindFrame("H4"), cfg.Get("H4Weight", 2), direction, ref total, ref aligned);
                AddMtf(market.FindFrame("D1"), cfg.Get("D1Weight", 2), direction, ref total, ref aligned);
                if (cfg.Get("SmartWeeklyContext", true))
                    AddMtf(market.FindFrame("W1"), cfg.Get("W1Weight", 3), direction, ref total, ref aligned);
    
                return total <= 0 ? 0 : Clamp((int)Math.Round(100.0 * aligned / total), 0, 100);
            }
    
            private void AddMtf(
                MarketFrame frame, int weight,
                Direction direction,
                ref double total, ref double aligned)
            {
                if (frame == null || !frame.DataValid ||
                    frame.BiasDirection == Direction.Wait || weight <= 0)
                    return;
    
                total += weight;
                if (frame.BiasDirection == direction) aligned += weight;
            }
    
            private bool CoreAgreement(
                Direction direction,
                MarketModel market,
                ConfigSnapshot cfg)
            {
                if (direction == Direction.Wait) return false;
                MarketFrame m5 = market.FindFrame("M5");
                MarketFrame m15 = market.FindFrame("M15");
                if (m5 == null || m15 == null || m5.BiasDirection != direction)
                    return false;
    
                return m15.BiasDirection == direction ||
                    (cfg.Get("AllowM15NeutralPullback", true) &&
                     m15.BiasDirection == Direction.Wait);
            }
    
            private bool RegimeBlocked(
                string regime, int quality, ConfigSnapshot cfg)
            {
                if (string.Equals(regime, "Compression", StringComparison.OrdinalIgnoreCase) &&
                    cfg.Get("BlockCompressionRegime", true))
                    return true;
    
                bool weak =
                    (string.Equals(regime, "Range", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(regime, "Transition", StringComparison.OrdinalIgnoreCase)) &&
                    quality < cfg.Get("SmartRegimeQualityFloor", 55);
    
                return weak && cfg.Get("BlockWeakRangeTransition", true);
            }
    
            private DecisionPolicyMode ResolvePolicy(
                bool eligible,
                Direction direction,
                int confidence,
                int quality,
                int evidence,
                IList<BlockReason> blocks,
                ConfigSnapshot cfg)
            {
                if (eligible &&
                    cfg.Get("EnableAggressiveAutoEntry", false) &&
                    confidence >= cfg.Get("AggressiveMinimumConfidence", 88) &&
                    evidence >= cfg.Get("AggressiveMinimumEvidence", 4) &&
                    quality >= cfg.Get("AggressiveMinimumSmartQuality", 78))
                    return DecisionPolicyMode.Aggressive;
    
                if (eligible)
                    return DecisionPolicyMode.Confirmed;
    
                //  owns only policy state. Trigger/retest execution remains
                // . Pending therefore means "directional setup exists but
                // one or more policy gates are not yet satisfied".
                if (direction != Direction.Wait &&
                    cfg.Get("EnableSmartDecisionEngine", true) &&
                    blocks != null &&
                    blocks.Count > 0 &&
                    quality >= cfg.Get("NoTradeMinimumSmartQuality", 55))
                    return DecisionPolicyMode.Pending;
    
                return DecisionPolicyMode.Soft;
            }
    
            private static List<BlockReason> Distinct(
                IList<BlockReason> source)
            {
                var result = new List<BlockReason>();
                if (source == null) return result;
    
                for (int i = 0; i < source.Count; i++)
                {
                    if (source[i] == BlockReason.None) continue;
                    bool found = false;
                    for (int j = 0; j < result.Count; j++)
                        if (result[j] == source[i]) { found = true; break; }
                    if (!found) result.Add(source[i]);
                }
                return result;
            }
    
            private static DecisionSnapshot Blocked(
                BlockReason reason, string rule)
            {
                var blocks = new List<BlockReason>();
                blocks.Add(reason);
    
                return new DecisionSnapshot(
                    Direction.Wait,
                    0, 0, 0, 0, 0, 0, "UNKNOWN", 0,
                    false,
                    DecisionPolicyMode.Soft,
                    blocks,
                    Provenance.Direct("DECISION_ENGINE", rule));
            }
    
            private static int Clamp(int value, int min, int max)
            {
                return Math.Max(min, Math.Min(max, value));
            }
        }
    
        public interface IDecisionEngine
        {
            DecisionSnapshot Evaluate(
                RuntimeSnapshot runtime,
                MtfSnapshot mtf,
                MarketModel market,
                StructureSnapshot structure,
                ConfigSnapshot configuration);
        }
    
    
}
