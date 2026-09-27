// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public sealed class CFIPClean89StructureSnapshot
        {
            private readonly ReadOnlyCollection<CFIPClean89StructureEventRecord> _events;
            private readonly ReadOnlyCollection<CFIPClean89ZoneRecord> _zones;
            private readonly ReadOnlyCollection<CFIPClean89LiquidityRecord> _liquidity;
    
            public DateTime ReferenceUtc { get; private set; }
            public bool IsCoherent { get; private set; }
            public bool IsPrimaryReady { get; private set; }
            public CFIPClean89Direction CurrentStructureDirection { get; private set; }
            public int StructureQuality { get; private set; }
            public CFIPClean89PremiumDiscountState PremiumDiscount { get; private set; }
            public IReadOnlyList<CFIPClean89StructureEventRecord> Events { get { return _events; } }
            public IReadOnlyList<CFIPClean89ZoneRecord> Zones { get { return _zones; } }
            public IReadOnlyList<CFIPClean89LiquidityRecord> Liquidity { get { return _liquidity; } }
    
            public CFIPClean89StructureSnapshot(
                DateTime referenceUtc, bool isCoherent, bool isPrimaryReady,
                CFIPClean89Direction direction, int quality,
                IList<CFIPClean89StructureEventRecord> events,
                IList<CFIPClean89ZoneRecord> zones,
                IList<CFIPClean89LiquidityRecord> liquidity,
                CFIPClean89PremiumDiscountState premiumDiscount)
            {
                ReferenceUtc = referenceUtc; IsCoherent = isCoherent; IsPrimaryReady = isPrimaryReady;
                CurrentStructureDirection = direction;
                StructureQuality = Math.Max(0, Math.Min(100, quality));
                _events = new ReadOnlyCollection<CFIPClean89StructureEventRecord>(
                    new List<CFIPClean89StructureEventRecord>(events ?? new List<CFIPClean89StructureEventRecord>()));
                _zones = new ReadOnlyCollection<CFIPClean89ZoneRecord>(
                    new List<CFIPClean89ZoneRecord>(zones ?? new List<CFIPClean89ZoneRecord>()));
                _liquidity = new ReadOnlyCollection<CFIPClean89LiquidityRecord>(
                    new List<CFIPClean89LiquidityRecord>(liquidity ?? new List<CFIPClean89LiquidityRecord>()));
                PremiumDiscount = premiumDiscount ?? new CFIPClean89PremiumDiscountState(false, 0, 0, 0);
            }
    
            public bool HasEvent(CFIPClean89StructureEventKind kind, CFIPClean89Direction direction)
            {
                for (int i = 0; i < _events.Count; i++)
                    if (_events[i].Kind == kind && _events[i].Direction == direction) return true;
                return false;
            }
    
            public CFIPClean89ZoneRecord FindNearestZone(
                CFIPClean89Direction direction, CFIPClean89ZoneKind kind,
                double price, bool executionEligibleOnly)
            {
                CFIPClean89ZoneRecord best = null; double bestDistance = double.MaxValue;
                for (int i = 0; i < _zones.Count; i++)
                {
                    var z = _zones[i];
                    if (z.Direction != direction || z.Kind != kind ||
                        z.Consumed || z.Invalidated || z.Lifecycle == CFIPClean89ZoneLifecycle.Expired)
                        continue;
                    if (executionEligibleOnly && !z.ExecutionEligible) continue;
    
                    double distance =
                        price <= z.CurrentLower ? z.CurrentLower - price :
                        price >= z.CurrentUpper ? price - z.CurrentUpper : 0;
    
                    if (distance < bestDistance) { bestDistance = distance; best = z; }
                }
                return best;
            }
    
            public CFIPClean89LiquidityRecord FindNearestLiquidity(
                CFIPClean89LiquiditySide side, double price, bool unsweptOnly)
            {
                CFIPClean89LiquidityRecord best = null; double bestDistance = double.MaxValue;
                for (int i = 0; i < _liquidity.Count; i++)
                {
                    var x = _liquidity[i];
                    if (x.Side != side || (unsweptOnly && x.Swept)) continue;
                    double distance = Math.Abs(x.Price - price);
                    if (distance < bestDistance) { bestDistance = distance; best = x; }
                }
                return best;
            }
        }
}
