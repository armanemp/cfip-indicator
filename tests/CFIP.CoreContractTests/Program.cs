using System;
using System.Collections.Generic;
using CFIP.Indicator.Core;

internal static class Program
{
    private static void Main()
    {
        Assert(CFIPClean89DirectionRules.Opposite(CFIPClean89Direction.Buy) == CFIPClean89Direction.Sell, "BUY opposite");
        Assert(CFIPClean89DirectionRules.Opposite(CFIPClean89Direction.Sell) == CFIPClean89Direction.Buy, "SELL opposite");
        Assert(!CFIPClean89DirectionRules.IsDirectional(CFIPClean89Direction.Wait), "WAIT is non-directional");

        Assert(CFIPClean89DirectionRules.IsProtectiveMove(
            CFIPClean89Direction.Buy, 100, 101), "BUY protective move");
        Assert(CFIPClean89DirectionRules.IsProtectiveMove(
            CFIPClean89Direction.Sell, 100, 99), "SELL protective move");
        Assert(!CFIPClean89DirectionRules.IsProtectiveMove(
            CFIPClean89Direction.Buy, 100, 99), "BUY backward stop rejected");

        var provenance = CFIPClean89Provenance.Direct("STRUCTURE", "TEST");
        var p1 = new CFIPClean89PriceLevel(100, "TP1", provenance);
        var p2 = new CFIPClean89PriceLevel(101, "TP2", provenance);
        var p3 = new CFIPClean89PriceLevel(102, "TP3", provenance);
        var t1 = new CFIPClean89TargetLevel(CFIPClean89TargetStage.TP1, p1, 80, CFIPClean89TargetState.Proposed);
        var t2 = new CFIPClean89TargetLevel(CFIPClean89TargetStage.TP2, p2, 80, CFIPClean89TargetState.Proposed);
        var t3 = new CFIPClean89TargetLevel(CFIPClean89TargetStage.TP3, p3, 80, CFIPClean89TargetState.Proposed);
        var ladder = new CFIPClean89TargetLadder(new List<CFIPClean89TargetLevel> { t3, t1, t2 });

        Assert(ladder.Find(CFIPClean89TargetStage.TP1) == t1, "TP1 lookup");
        Assert(ladder.ValidateForDirection(CFIPClean89Direction.Buy, 99), "BUY target ladder");
        Assert(!ladder.ValidateForDirection(CFIPClean89Direction.Sell, 99), "SELL target ladder rejects BUY ordering");

        var zone = new CFIPClean89PriceZone(99, 101, "ENTRY", provenance);
        Assert(zone.Contains(100), "zone contains midpoint");
        Assert(!zone.Contains(102), "zone excludes outside price");

        var entry = new CFIPClean89EntryModel(
            CFIPClean89Direction.Buy,
            new CFIPClean89PriceLevel(100, "Ideal", provenance),
            zone,
            null,
            new CFIPClean89PriceLevel(97, "Invalidation", provenance));

        Assert(entry.Direction == CFIPClean89Direction.Buy, "entry direction");
        Assert(entry.Trigger == null, "retest trigger may be absent");

        Console.WriteLine("Core contract tests passed.");
    }

    private static void Assert(bool condition, string name)
    {
        if (!condition)
            throw new InvalidOperationException("FAILED: " + name);
    }
}
