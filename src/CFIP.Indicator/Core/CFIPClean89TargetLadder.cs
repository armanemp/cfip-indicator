// Migrated from CFIP-PRO v89; now isolated from the cTrader host namespace.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CFIP.Indicator.Core
{
        public sealed class CFIPClean89TargetLadder
        {
            private readonly ReadOnlyCollection<CFIPClean89TargetLevel> _levels;
    
            public IReadOnlyList<CFIPClean89TargetLevel> Levels
            {
                get { return _levels; }
            }
    
            public CFIPClean89TargetLadder(
                IList<CFIPClean89TargetLevel> levels)
            {
                if (levels == null)
                    throw new ArgumentNullException("levels");
    
                var copy =
                    new List<CFIPClean89TargetLevel>(
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
                        CFIPClean89TargetLevel left,
                        CFIPClean89TargetLevel right)
                    {
                        return left.Stage.CompareTo(right.Stage);
                    });
    
                _levels =
                    new ReadOnlyCollection<CFIPClean89TargetLevel>(
                        copy);
            }
    
            public bool ValidateForDirection(
                CFIPClean89Direction direction)
            {
                return ValidateForDirection(direction, double.NaN);
            }
    
            public bool ValidateForDirection(
                CFIPClean89Direction direction,
                double referencePrice)
            {
                if (!CFIPClean89DirectionRules.IsDirectional(direction) ||
                    _levels.Count == 0)
                    return false;
    
                for (int i = 0; i < _levels.Count; i++)
                {
                    CFIPClean89TargetLevel item = _levels[i];
    
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
    
                        if (direction == CFIPClean89Direction.Buy &&
                            current <= previous)
                            return false;
    
                        if (direction == CFIPClean89Direction.Sell &&
                            current >= previous)
                            return false;
                    }
    
                    if (!double.IsNaN(referencePrice) &&
                        referencePrice > 0)
                    {
                        if (direction == CFIPClean89Direction.Buy &&
                            current <= referencePrice)
                            return false;
    
                        if (direction == CFIPClean89Direction.Sell &&
                            current >= referencePrice)
                            return false;
                    }
                }
    
                return true;
            }
    
            public CFIPClean89TargetLevel Find(
                CFIPClean89TargetStage stage)
            {
                for (int i = 0; i < _levels.Count; i++)
                {
                    if (_levels[i].Stage == stage)
                        return _levels[i];
                }
    
                return null;
            }
        }
}
