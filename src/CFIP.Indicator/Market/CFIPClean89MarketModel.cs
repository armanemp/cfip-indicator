// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public sealed class CFIPClean89MarketModel
        {
            private readonly ReadOnlyCollection<CFIPClean89MarketFrame> _frames;
    
            public DateTime ReferenceUtc { get; private set; }
            public bool IsCoherent { get; private set; }
            public bool IsPrimaryReady { get; private set; }
            public string DataStatus { get; private set; }
    
            public IReadOnlyList<CFIPClean89MarketFrame> Frames
            {
                get { return _frames; }
            }
    
            public CFIPClean89MarketFrame M5
            {
                get { return FindFrame("M5"); }
            }
    
            public CFIPClean89MarketFrame M15
            {
                get { return FindFrame("M15"); }
            }
    
            public CFIPClean89MarketFrame FindFrame(string timeframe)
            {
                string key = timeframe ?? string.Empty;
    
                for (int i = 0; i < _frames.Count; i++)
                {
                    if (string.Equals(
                            _frames[i].Timeframe,
                            key,
                            StringComparison.OrdinalIgnoreCase))
                        return _frames[i];
                }
    
                return null;
            }
    
            public CFIPClean89MarketModel(
                DateTime referenceUtc,
                bool isCoherent,
                bool isPrimaryReady,
                string dataStatus,
                IList<CFIPClean89MarketFrame> frames)
            {
                ReferenceUtc = referenceUtc;
                IsCoherent = isCoherent;
                IsPrimaryReady = isPrimaryReady;
                DataStatus = dataStatus ?? string.Empty;
                _frames =
                    new ReadOnlyCollection<CFIPClean89MarketFrame>(
                        new List<CFIPClean89MarketFrame>(
                            frames ??
                            new List<CFIPClean89MarketFrame>()));
            }
        }
}
