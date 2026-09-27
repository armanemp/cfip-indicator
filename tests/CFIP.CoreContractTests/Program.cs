using System;
using System.Collections.Generic;
using CFIP.Indicator;

internal static class Program
{
    private static void Main()
    {
        Assert(DirectionRules.Opposite(Direction.Buy) == Direction.Sell, "BUY opposite");
        Assert(DirectionRules.Opposite(Direction.Sell) == Direction.Buy, "SELL opposite");
        Assert(!DirectionRules.IsDirectional(Direction.Wait), "WAIT non-directional");
        Assert(DirectionRules.IsProtectiveMove(Direction.Buy, 100, 101), "BUY protective");
        Assert(DirectionRules.IsProtectiveMove(Direction.Sell, 100, 99), "SELL protective");
        Assert(!DirectionRules.IsProtectiveMove(Direction.Buy, 100, 99), "BUY backward stop");

        var provenance = Provenance.Direct("STRUCTURE", "TEST");
        var ladder = new TargetLadder(new List<TargetLevel>
        {
            new TargetLevel(TargetStage.TP3, new PriceLevel(102, "TP3", provenance), 80, TargetState.Proposed),
            new TargetLevel(TargetStage.TP1, new PriceLevel(100, "TP1", provenance), 80, TargetState.Proposed),
            new TargetLevel(TargetStage.TP2, new PriceLevel(101, "TP2", provenance), 80, TargetState.Proposed)
        });

        Assert(ladder.ValidateForDirection(Direction.Buy, 99), "BUY ladder");
        Assert(!ladder.ValidateForDirection(Direction.Sell, 99), "SELL rejects BUY ordering");
        Console.WriteLine("Core contract tests passed.");
    }

    private static void Assert(bool condition, string name)
    {
        if (!condition) throw new InvalidOperationException("FAILED: " + name);
    }
}
