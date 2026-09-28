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
                                    if (_m5Bars == null)
                                        return new MtfClosedContext(
                                            reference,
                                            -1,
                                            -1,
                                            -1,
                                            -1,
                                            -1,
                                            -1,
                                            -1,
                                            -1);

                                    if (_mtfClosedContextCache.TryGetCached(
                                            _m1Bars,
                                            _m5Bars,
                                            _m15Bars,
                                            _m30Bars,
                                            _h1Bars,
                                            _h4Bars,
                                            _d1Bars,
                                            _w1Bars,
                                            out MtfClosedContext cached))
                                        return cached;

                                    MtfClosedContext context =
                                        new MtfClosedContext(
                                            reference,
                                            ClosedIndex(_m5Bars, reference),
                                            ClosedIndex(_m1Bars, reference),
                                            ClosedIndex(_m15Bars, reference),
                                            ClosedIndex(_m30Bars, reference),
                                            ClosedIndex(_h1Bars, reference),
                                            ClosedIndex(_h4Bars, reference),
                                            ClosedIndex(_d1Bars, reference),
                                            ClosedIndex(_w1Bars, reference));

                                    _mtfClosedContextCache.Store(
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
                                    return _m5Bars != null &&
                                           _m15Bars != null &&
                                           _m30Bars != null &&
                                           _h1Bars != null &&
                                           _h4Bars != null &&
                                           _m5Bars.Count >= 100 &&
                                           _m15Bars.Count >= 100 &&
                                           _m30Bars.Count >= 80 &&
                                           _h1Bars.Count >= 80 &&
                                           _h4Bars.Count >= 60;
                                }
    }
}
