using System;
using System.Collections.Generic;
using cAlgo.API;

namespace cAlgo
{
    internal sealed class PlanLevelVisual
    {
        public string Key { get; }
        public string LabelName { get; }
        public double Price { get; }
        public Color Color { get; }
        public bool Visible { get; }
        public bool IncludeDistance { get; }

        public PlanLevelVisual(
            string key,
            string labelName,
            double price,
            Color color,
            bool visible,
            bool includeDistance)
        {
            Key = key;
            LabelName = labelName;
            Price = price;
            Color = color;
            Visible = visible;
            IncludeDistance = includeDistance;
        }
    }

    internal sealed class PlanLevelVisualState
    {
        public double Entry { get; }
        public double IdealEntry { get; }
        public double Trigger { get; }
        public double Stop { get; }
        public double Tp1 { get; }
        public double Tp2 { get; }
        public double Tp3 { get; }
        public double Tp4 { get; }
        public double ActiveTarget { get; }

        public IReadOnlyList<PlanLevelVisual> Levels { get; }

        public PlanLevelVisualState(
            double entry,
            double idealEntry,
            double trigger,
            double stop,
            double tp1,
            double tp2,
            double tp3,
            double tp4,
            double activeTarget,
            IReadOnlyList<PlanLevelVisual> levels)
        {
            Entry = entry;
            IdealEntry = idealEntry;
            Trigger = trigger;
            Stop = stop;
            Tp1 = tp1;
            Tp2 = tp2;
            Tp3 = tp3;
            Tp4 = tp4;
            ActiveTarget = activeTarget;
            Levels = levels;
        }
    }
}
