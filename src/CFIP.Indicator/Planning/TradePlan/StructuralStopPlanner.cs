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
        private double ResolveStructuralStopMinimumRiskAtr(
            string timeframe,
            int closedM5,
            int direction,
            double atr)
        {
            if (!IsFinitePositive(atr))
                return 0;

            bool microTimeframe =
                string.Equals(
                    timeframe,
                    "M1",
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    timeframe,
                    "M5",
                    StringComparison.OrdinalIgnoreCase);

            bool allowMicroRelaxation =
                microTimeframe &&
                _m5Frame != null &&
                _m15Frame != null &&
                _m5Frame.Direction == direction &&
                _m15Frame.Direction == direction &&
                _m5Frame.Quality >= SmartStopQuality;

            double microAtr = 0;

            if (allowMicroRelaxation &&
                _m1Bars != null &&
                closedM5 >= 0)
            {
                DateTime reference =
                    ClosedBarBoundaryReference(
                        _m5Bars,
                        closedM5,
                        Server.TimeInUtc);

                int m1Index =
                    ClosedIndex(
                        _m1Bars,
                        reference);

                if (m1Index >= 10)
                    microAtr =
                        Atr(
                            _m1Bars,
                            m1Index);
            }

            return StructuralStopRiskRule.ResolveEffectiveMinimumStopRiskAtr(
                MinimumSlAtr,
                atr,
                microAtr,
                Math.Max(
                    0,
                    Symbol.Ask - Symbol.Bid),
                Symbol.PipSize,
                MaximumSpreadToStopRiskRatio,
                allowMicroRelaxation);
        }

        private double ResolveSelectedStructuralStopMinimumRiskAtr(
            string source,
            int closedM5,
            int direction,
            double atr)
        {
            bool micro =
                !string.IsNullOrWhiteSpace(source) &&
                source.EndsWith(
                    "@M1",
                    StringComparison.OrdinalIgnoreCase);

            return ResolveStructuralStopMinimumRiskAtr(
                micro ? "M1" : "M5",
                closedM5,
                direction,
                atr);
        }

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
