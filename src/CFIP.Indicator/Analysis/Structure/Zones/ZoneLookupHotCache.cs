using System;
using System.Collections.Generic;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private Bars _zoneLookupCacheBars;
        private int _zoneLookupCacheIndex = -1;

        private readonly Dictionary<string, Zone[]> _cachedFvgCandidates =
            new Dictionary<string, Zone[]>(
                StringComparer.Ordinal);

        private readonly Dictionary<string, Zone[]> _cachedObCandidates =
            new Dictionary<string, Zone[]>(
                StringComparer.Ordinal);

        private void ResetZoneLookupCacheIfNeeded(
            Bars bars,
            int index)
        {
            if (ReferenceEquals(
                    _zoneLookupCacheBars,
                    bars) &&
                _zoneLookupCacheIndex ==
                    index)
                return;

            _zoneLookupCacheBars =
                bars;
            _zoneLookupCacheIndex =
                index;

            _cachedFvgCandidates.Clear();
            _cachedObCandidates.Clear();
        }

        private bool TryGetCachedFvgCandidates(
            string key,
            out Zone[] zones)
        {
            return _cachedFvgCandidates.TryGetValue(
                key,
                out zones);
        }

        private void StoreCachedFvgCandidates(
            string key,
            List<Zone> zones)
        {
            _cachedFvgCandidates[key] =
                zones == null
                    ? new Zone[0]
                    : zones.ToArray();
        }

        private bool TryGetCachedObCandidates(
            string key,
            out Zone[] zones)
        {
            return _cachedObCandidates.TryGetValue(
                key,
                out zones);
        }

        private void StoreCachedObCandidates(
            string key,
            List<Zone> zones)
        {
            _cachedObCandidates[key] =
                zones == null
                    ? new Zone[0]
                    : zones.ToArray();
        }
    }
}
