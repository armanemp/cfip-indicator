using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CFIP.Indicator
{
        public enum Direction
        {
            Sell = -1,
            Wait = 0,
            Buy = 1
        }
    
        public static class DirectionRules
        {
            public static bool IsDirectional(Direction direction)
            {
                return direction != Direction.Wait;
            }
    
            public static Direction Opposite(Direction direction)
            {
                if (direction == Direction.Buy)
                    return Direction.Sell;
    
                if (direction == Direction.Sell)
                    return Direction.Buy;
    
                return Direction.Wait;
            }
    
            public static bool IsProtectiveMove(
                Direction direction,
                double currentStop,
                double candidateStop)
            {
                if (!IsDirectional(direction) ||
                    currentStop <= 0 ||
                    candidateStop <= 0)
                    return false;
    
                return direction == Direction.Buy
                    ? candidateStop > currentStop
                    : candidateStop < currentStop;
            }
        }
    
        public enum DecisionPolicyMode
        {
            Confirmed = 0,
            Soft = 1,
            Aggressive = 2,
            Pending = 3
        }
    
        public enum ExecutionKind
        {
            None = 0,
            Market = 1,
            Stop = 2,
            Limit = 3
        }
    
        public enum EntryMode
        {
            None = 0,
            RetestMarket = 1,
            BreakoutMarket = 2,
            ContinuationStop = 3,
            ReversalLimit = 4
        }
    
        public enum LifecycleState
        {
            Flat = 0,
            SignalDetected = 1,
            PlanReady = 2,
            ExecutionReady = 3,
            PendingOrder = 4,
            LivePosition = 5,
            ExitRequested = 6,
            RecoveryRequired = 7,
            Closed = 8,
            Rejected = 9,
            Error = 10
        }
    
        public enum BlockReason
        {
            None = 0,
            DataIncomplete = 1,
            NoDirection = 2,
            ConfidenceTooLow = 3,
            EvidenceInsufficient = 4,
            MtfDisagreement = 5,
            StructureInvalid = 6,
            EntryInvalid = 7,
            RiskInvalid = 8,
            TargetInvalid = 9,
            SpreadBlocked = 10,
            SessionBlocked = 11,
            NewsBlocked = 12,
            VolatilityBlocked = 13,
            DailyLossBlocked = 14,
            ExistingExposureBlocked = 15,
            DuplicateExecutionBlocked = 16,
            BrokerUnavailable = 17,
            BrokerConstraintsBlocked = 18,
            CooldownBlocked = 19,
            PolicyBlocked = 20,
            Unknown = 99
        }
    
        public enum FallbackKind
        {
            None = 0,
            Structural = 1,
            HigherTimeframe = 2,
            ExecutionFrame = 3,
            Atr = 4,
            Synthetic = 5
        }
    
        public enum TargetStage
        {
            TP1 = 1,
            TP2 = 2,
            TP3 = 3,
            TP4 = 4
        }
    
        public enum ProtectionState
        {
            Unknown = 0,
            Unprotected = 1,
            PartiallyProtected = 2,
            FullyProtected = 3,
            RecoveryRequired = 4
        }
    
        public enum BrokerObjectKind
        {
            None = 0,
            Position = 1,
            PendingOrder = 2
        }
    
        public enum TargetState
        {
            Proposed = 0,
            Active = 1,
            Hit = 2,
            Invalidated = 3,
            Consumed = 4
        }
    
        public sealed class Provenance
        {
            public string Source { get; private set; }
            public string Rule { get; private set; }
            public FallbackKind Fallback { get; private set; }
            public string Detail { get; private set; }
    
            public Provenance(
                string source,
                string rule,
                FallbackKind fallback,
                string detail)
            {
                Source = source ?? string.Empty;
                Rule = rule ?? string.Empty;
                Fallback = fallback;
                Detail = detail ?? string.Empty;
            }
    
            public static Provenance Direct(
                string source,
                string rule)
            {
                return new Provenance(
                    source,
                    rule,
                    FallbackKind.None,
                    string.Empty);
            }
    
            public static Provenance FallbackFrom(
                string source,
                string rule,
                FallbackKind fallback,
                string detail)
            {
                return new Provenance(
                    source,
                    rule,
                    fallback,
                    detail);
            }
        }
    
        public sealed class PriceLevel
        {
            public double Price { get; private set; }
            public string Name { get; private set; }
            public Provenance Provenance { get; private set; }
    
            public PriceLevel(
                double price,
                string name,
                Provenance provenance)
            {
                if (price <= 0)
                    throw new ArgumentOutOfRangeException("price");
    
                Price = price;
                Name = name ?? string.Empty;
                Provenance = provenance ??
                             Provenance.Direct(
                                 "UNKNOWN",
                                 "UNSPECIFIED");
            }
        }
    
        public sealed class PriceZone
        {
            public double Lower { get; private set; }
            public double Upper { get; private set; }
            public string Name { get; private set; }
            public Provenance Provenance { get; private set; }
    
            public PriceZone(
                double lower,
                double upper,
                string name,
                Provenance provenance)
            {
                if (lower <= 0 || upper <= 0 || upper < lower)
                    throw new ArgumentException("Invalid price zone.");
    
                Lower = lower;
                Upper = upper;
                Name = name ?? string.Empty;
                Provenance = provenance ??
                             Provenance.Direct(
                                 "UNKNOWN",
                                 "UNSPECIFIED");
            }
    
            public bool Contains(double price)
            {
                return price >= Lower && price <= Upper;
            }
    
            public double Midpoint
            {
                get { return (Lower + Upper) / 2.0; }
            }
        }
    
        public sealed class EntryModel
        {
            public Direction Direction { get; private set; }
    
            // Preferred price inside the structural execution area.
            public PriceLevel IdealEntry { get; private set; }
    
            // Allowed structural retest region.
            public PriceZone EntryZone { get; private set; }
    
            // Structural activation threshold for breakout/continuation.
            public PriceLevel Trigger { get; private set; }
    
            // Exact strategy/broker protection boundary.
            public PriceLevel Invalidation { get; private set; }
    
            public EntryModel(
                Direction direction,
                PriceLevel idealEntry,
                PriceZone entryZone,
                PriceLevel trigger,
                PriceLevel invalidation)
            {
                if (!DirectionRules.IsDirectional(direction))
                    throw new ArgumentException("A directional entry model is required.");
    
                if (idealEntry == null)
                    throw new ArgumentNullException("idealEntry");
    
                if (entryZone == null)
                    throw new ArgumentNullException("entryZone");
    
                if (invalidation == null)
                    throw new ArgumentNullException("invalidation");
    
                // Retest entries do not require an activation threshold.
                // Breakout/continuation entries must provide Trigger.
                Direction = direction;
                IdealEntry = idealEntry;
                EntryZone = entryZone;
                Trigger = trigger;
                Invalidation = invalidation;
            }
        }
    
        public sealed class TargetLevel
        {
            public TargetStage Stage { get; private set; }
            public PriceLevel Level { get; private set; }
            public TargetState State { get; private set; }
            public int Quality { get; private set; }
    
            public TargetLevel(
                TargetStage stage,
                PriceLevel level,
                int quality,
                TargetState state)
            {
                if ((int)stage < (int)TargetStage.TP1 ||
                    (int)stage > (int)TargetStage.TP4)
                    throw new ArgumentOutOfRangeException("stage");
    
                if (level == null)
                    throw new ArgumentNullException("level");
    
                Stage = stage;
                Level = level;
                Quality = Math.Max(0, Math.Min(100, quality));
                State = state;
            }
        }
    
        public sealed class TargetLadder
        {
            private readonly ReadOnlyCollection<TargetLevel> _levels;
    
            public IReadOnlyList<TargetLevel> Levels
            {
                get { return _levels; }
            }
    
            public TargetLadder(
                IList<TargetLevel> levels)
            {
                if (levels == null)
                    throw new ArgumentNullException("levels");
    
                var copy =
                    new List<TargetLevel>(
                        levels);
    
                for (int i = 0; i < copy.Count; i++)
                {
                    if (copy[i] == null ||
                        copy[i].Level == null)
                        throw new ArgumentException(
                            "Target ladder cannot contain null levels.",
                            "levels");
                }
    
                copy.Sort(
                    delegate (
                        TargetLevel left,
                        TargetLevel right)
                    {
                        return left.Stage.CompareTo(right.Stage);
                    });
    
                _levels =
                    new ReadOnlyCollection<TargetLevel>(
                        copy);
            }
    
            public bool ValidateForDirection(
                Direction direction)
            {
                return ValidateForDirection(direction, double.NaN);
            }
    
            public bool ValidateForDirection(
                Direction direction,
                double referencePrice)
            {
                if (!DirectionRules.IsDirectional(direction) ||
                    _levels.Count == 0)
                    return false;
    
                for (int i = 0; i < _levels.Count; i++)
                {
                    TargetLevel item = _levels[i];
    
                    if (item == null ||
                        item.Level == null)
                        return false;
    
                    if ((int)item.Stage != i + 1)
                        return false;
    
                    double current = item.Level.Price;
    
                    if (double.IsNaN(current) ||
                        double.IsInfinity(current) ||
                        current <= 0)
                        return false;
    
                    if (i > 0)
                    {
                        double previous = _levels[i - 1].Level.Price;
    
                        if (direction == Direction.Buy &&
                            current <= previous)
                            return false;
    
                        if (direction == Direction.Sell &&
                            current >= previous)
                            return false;
                    }
    
                    if (!double.IsNaN(referencePrice) &&
                        referencePrice > 0)
                    {
                        if (direction == Direction.Buy &&
                            current <= referencePrice)
                            return false;
    
                        if (direction == Direction.Sell &&
                            current >= referencePrice)
                            return false;
                    }
                }
    
                return true;
            }
    
            public TargetLevel Find(
                TargetStage stage)
            {
                for (int i = 0; i < _levels.Count; i++)
                {
                    if (_levels[i].Stage == stage)
                        return _levels[i];
                }
    
                return null;
            }
        }
    
        public sealed class BrokerConstraints
        {
            public double MinVolumeInUnits { get; private set; }
            public double VolumeStepInUnits { get; private set; }
            public double MinStopDistancePips { get; private set; }
            public double MinTakeProfitDistancePips { get; private set; }
    
            public BrokerConstraints(
                double minVolumeInUnits,
                double volumeStepInUnits,
                double minStopDistancePips,
                double minTakeProfitDistancePips)
            {
                MinVolumeInUnits = Math.Max(0, minVolumeInUnits);
                VolumeStepInUnits = Math.Max(0, volumeStepInUnits);
                MinStopDistancePips = Math.Max(0, minStopDistancePips);
                MinTakeProfitDistancePips =
                    Math.Max(0, minTakeProfitDistancePips);
            }
        }
    
    
}
