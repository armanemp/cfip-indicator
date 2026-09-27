// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public static class CFIPClean89MtfSnapshotBuilder
        {
            public static DateTime ResolveM5Reference(
                Bars m5Bars,
                DateTime serverUtc)
            {
                if (m5Bars == null ||
                    m5Bars.Count < 2 ||
                    serverUtc == DateTime.MinValue)
                    return DateTime.MinValue;
    
                int last =
                    m5Bars.Count - 1;
    
                DateTime reference =
                    m5Bars.OpenTimes[last];
    
                if (reference > serverUtc)
                    return DateTime.MinValue;
    
                return reference;
            }
    
            public static CFIPClean89MtfSnapshot Build(
                DateTime serverUtc,
                DateTime userLocalTime,
                Bars chartBars,
                string chartTimeframe,
                Bars m1Bars,
                Bars m5Bars,
                Bars m15Bars,
                Bars m30Bars,
                Bars h1Bars,
                Bars h4Bars,
                Bars d1Bars,
                Bars w1Bars,
                int minimumHistory)
            {
                DateTime reference =
                    ResolveM5Reference(
                        m5Bars,
                        serverUtc);
    
                if (reference == DateTime.MinValue)
                {
                    return new CFIPClean89MtfSnapshot(
                        serverUtc,
                        DateTime.MinValue,
                        userLocalTime,
                        TimeSpan.Zero,
                        chartTimeframe,
                        CFIPClean89MtfBarSnapshot.Missing(
                            "CHART"),
                        CFIPClean89MtfBarSnapshot.Missing("M1"),
                        CFIPClean89MtfBarSnapshot.Missing("M5"),
                        CFIPClean89MtfBarSnapshot.Missing("M15"),
                        CFIPClean89MtfBarSnapshot.Missing("M30"),
                        CFIPClean89MtfBarSnapshot.Missing("H1"),
                        CFIPClean89MtfBarSnapshot.Missing("H4"),
                        CFIPClean89MtfBarSnapshot.Missing("D1"),
                        CFIPClean89MtfBarSnapshot.Missing("W1"),
                        CFIPClean89MtfDataStatus.InvalidReference);
                }
    
                TimeSpan age =
                    serverUtc >= reference
                        ? serverUtc - reference
                        : TimeSpan.Zero;
    
                CFIPClean89MtfBarSnapshot chart =
                    ResolveClosedBar(
                        chartBars,
                        reference,
                        chartTimeframe,
                        minimumHistory);
    
                CFIPClean89MtfBarSnapshot m1 =
                    ResolveClosedBar(
                        m1Bars,
                        reference,
                        "M1",
                        minimumHistory);
    
                CFIPClean89MtfBarSnapshot m5 =
                    ResolveClosedBar(
                        m5Bars,
                        reference,
                        "M5",
                        minimumHistory);
    
                CFIPClean89MtfBarSnapshot m15 =
                    ResolveClosedBar(
                        m15Bars,
                        reference,
                        "M15",
                        minimumHistory);
    
                CFIPClean89MtfBarSnapshot m30 =
                    ResolveClosedBar(
                        m30Bars,
                        reference,
                        "M30",
                        minimumHistory);
    
                CFIPClean89MtfBarSnapshot h1 =
                    ResolveClosedBar(
                        h1Bars,
                        reference,
                        "H1",
                        minimumHistory);
    
                CFIPClean89MtfBarSnapshot h4 =
                    ResolveClosedBar(
                        h4Bars,
                        reference,
                        "H4",
                        minimumHistory);
    
                CFIPClean89MtfBarSnapshot d1 =
                    ResolveClosedBar(
                        d1Bars,
                        reference,
                        "D1",
                        minimumHistory);
    
                CFIPClean89MtfBarSnapshot w1 =
                    ResolveClosedBar(
                        w1Bars,
                        reference,
                        "W1",
                        minimumHistory);
    
                bool primaryHistory =
                    HasPrimaryHistory(
                        m5,
                        m15,
                        m30,
                        h1,
                        h4,
                        minimumHistory);
    
                bool allTimeframesAvailable =
                    m1.IsAvailable &&
                    m5.IsAvailable &&
                    m15.IsAvailable &&
                    m30.IsAvailable &&
                    h1.IsAvailable &&
                    h4.IsAvailable &&
                    d1.IsAvailable &&
                    w1.IsAvailable;
    
                CFIPClean89MtfDataStatus status;
    
                if (age > TimeSpan.FromMinutes(10))
                    status =
                        CFIPClean89MtfDataStatus.StaleReference;
                else if (!primaryHistory)
                    status =
                        CFIPClean89MtfDataStatus.PrimaryHistoryInsufficient;
                else if (!m1.IsAvailable ||
                         !m5.IsAvailable ||
                         !m15.IsAvailable ||
                         !m30.IsAvailable ||
                         !h1.IsAvailable ||
                         !h4.IsAvailable)
                    status =
                        CFIPClean89MtfDataStatus.MissingTimeframeData;
                else
                    status =
                        CFIPClean89MtfDataStatus.Ready;
    
                return new CFIPClean89MtfSnapshot(
                    serverUtc,
                    reference,
                    userLocalTime,
                    age,
                    chartTimeframe,
                    chart,
                    m1,
                    m5,
                    m15,
                    m30,
                    h1,
                    h4,
                    d1,
                    w1,
                    status);
            }
    
            public static bool HasPrimaryHistory(
                CFIPClean89MtfBarSnapshot m5,
                CFIPClean89MtfBarSnapshot m15,
                CFIPClean89MtfBarSnapshot m30,
                CFIPClean89MtfBarSnapshot h1,
                CFIPClean89MtfBarSnapshot h4,
                int minimumHistory)
            {
                int minimum =
                    Math.Max(
                        2,
                        minimumHistory);
    
                return
                    m5 != null &&
                    m15 != null &&
                    m30 != null &&
                    h1 != null &&
                    h4 != null &&
                    m5.ClosedIndex >= minimum &&
                    m15.ClosedIndex >= minimum &&
                    m30.ClosedIndex >= minimum &&
                    h1.ClosedIndex >= minimum &&
                    h4.ClosedIndex >= minimum;
            }
    
            public static CFIPClean89MtfBarSnapshot ResolveClosedBar(
                Bars bars,
                DateTime reference,
                string timeframe,
                int minimumHistory)
            {
                if (bars == null ||
                    bars.Count < 2 ||
                    reference == DateTime.MinValue ||
                    reference < bars.OpenTimes[0])
                    return
                        CFIPClean89MtfBarSnapshot.Missing(
                            timeframe);
    
                int probe =
                    bars.OpenTimes.GetIndexByTime(
                        reference);
    
                if (probe < 0)
                    probe = bars.Count - 1;
    
                probe =
                    Math.Max(
                        0,
                        Math.Min(
                            probe,
                            bars.Count - 1));
    
                // The final series item is treated as potentially forming.
                // Never return it as a closed analysis bar.
                if (probe == bars.Count - 1)
                    probe--;
    
                for (int i = probe; i >= 0; i--)
                {
                    DateTime open =
                        bars.OpenTimes[i];
    
                    if (open >= reference)
                        continue;
    
                    if (i + 1 >= bars.Count)
                        continue;
    
                    DateTime nextOpen =
                        bars.OpenTimes[i + 1];
    
                    if (nextOpen > reference)
                        continue;
    
                    bool minimum =
                        i >= Math.Max(
                            2,
                            minimumHistory);
    
                    return
                        new CFIPClean89MtfBarSnapshot(
                            timeframe,
                            i,
                            open,
                            nextOpen,
                            true,
                            true,
                            minimum);
                }
    
                return
                    CFIPClean89MtfBarSnapshot.Missing(
                        timeframe);
            }
    
            public static bool IsCoherent(
                CFIPClean89MtfSnapshot snapshot)
            {
                if (snapshot == null ||
                    !snapshot.IsReferenceValid ||
                    !snapshot.IsReferenceFresh)
                    return false;
    
                CFIPClean89MtfBarSnapshot[] items =
                {
                    snapshot.M1,
                    snapshot.M5,
                    snapshot.M15,
                    snapshot.M30,
                    snapshot.H1,
                    snapshot.H4,
                    snapshot.D1,
                    snapshot.W1
                };
    
                if (!snapshot.IsPrimaryDecisionReady)
                    return false;
    
                for (int i = 0; i < items.Length; i++)
                {
                    CFIPClean89MtfBarSnapshot item =
                        items[i];
    
                    // D1/W1 may legitimately be unavailable when history is
                    // insufficient. Missing optional data is not temporal leakage.
                    if (item == null ||
                        !item.IsAvailable)
                        continue;
    
                    if (!item.IsFullyClosedAtReference ||
                        item.ClosedIndex < 0 ||
                        item.BarOpenUtc >= snapshot.ReferenceUtc ||
                        item.NextBarOpenUtc >
                        snapshot.ReferenceUtc)
                        return false;
                }
    
                return true;
            }
        }
}
