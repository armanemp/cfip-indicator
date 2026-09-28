// CFIP Indicator — StructuralStopPlanner.cs
// Single-responsibility planning module.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private double BuildStructuralStop(
                            int closedM5,
                            int direction,
                            double entry,
                            double atr,
                            out string source,
                            out int quality)
                        {
                            source = "NONE";
                            quality = 0;

                            if (_m5Bars == null ||
                                closedM5 < 20 ||
                                atr <= 0 ||
                                !IsFinitePositive(entry))
                                return 0;

                            List<Level> candidates =
                                CollectStructuralStopCandidates(
                                    closedM5,
                                    direction,
                                    entry,
                                    atr);

                            if (candidates.Count == 0)
                                return 0;

                            return SelectStructuralStopCandidate(
                                candidates,
                                closedM5,
                                direction,
                                entry,
                                atr,
                                out source,
                                out quality);
                        }
    }
}
