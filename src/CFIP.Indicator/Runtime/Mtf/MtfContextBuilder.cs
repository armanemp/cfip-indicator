// ============================================================================
// CFIP Indicator — MtfContextBuilder.cs
// ============================================================================

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
        private MtfClosedContext BuildMtfClosedContext(
                                    DateTime reference)
                                {
                                    DateTime normalizedReference =
                                        CanonicalTimeRule.EnsureUtc(reference);

                                    if (normalizedReference == DateTime.MinValue)
                                        return null;

                                    if (_m5Bars == null)
                                        return new MtfClosedContext(
                                            normalizedReference,
                                            -1,
                                            -1,
                                            -1,
                                            -1,
                                            -1,
                                            -1,
                                            -1,
                                            -1);

                                    if (_mtfClosedContextCache.TryGetStableContext(
                                            _m1Bars,
                                            _m5Bars,
                                            _m15Bars,
                                            _m30Bars,
                                            _h1Bars,
                                            _h4Bars,
                                            _d1Bars,
                                            _w1Bars,
                                            normalizedReference,
                                            out MtfClosedContext cached))
                                        return cached;

                                    MtfClosedContext context =
                                        new MtfClosedContext(
                                            normalizedReference,
                                            ClosedIndex(_m5Bars, normalizedReference),
                                            ClosedIndex(_m1Bars, normalizedReference),
                                            ClosedIndex(_m15Bars, normalizedReference),
                                            ClosedIndex(_m30Bars, normalizedReference),
                                            ClosedIndex(_h1Bars, normalizedReference),
                                            ClosedIndex(_h4Bars, normalizedReference),
                                            ClosedIndex(_d1Bars, normalizedReference),
                                            ClosedIndex(_w1Bars, normalizedReference));

                                    _mtfClosedContextCache.StoreStableContext(
                                        _m1Bars,
                                        _m5Bars,
                                        _m15Bars,
                                        _m30Bars,
                                        _h1Bars,
                                        _h4Bars,
                                        _d1Bars,
                                        _w1Bars,
                                        context);

                                    return context;
                                }
        
        private bool HasEnoughData()
                                {
                                    // Startup readiness should require enough closed
                                    // history for the canonical analyzers, not the much
                                    // deeper warm-up used by optional historical studies.
                                    // This lets the decision engine start while the
                                    // terminal continues to supply additional history.
                                    return _m5Bars != null &&
                                           _m15Bars != null &&
                                           _m30Bars != null &&
                                           _h1Bars != null &&
                                           _h4Bars != null &&
                                           _m5Bars.Count >= 60 &&
                                           _m15Bars.Count >= 50 &&
                                           _m30Bars.Count >= 45 &&
                                           _h1Bars.Count >= 40 &&
                                           _h4Bars.Count >= 36;
                                }
    }
}
