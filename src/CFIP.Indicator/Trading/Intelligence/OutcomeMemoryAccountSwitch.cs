using System;

namespace cAlgo
{
    public partial class CFIPIndicator
    {
        private void OnOutcomeMemoryAccountSwitched(
            cAlgo.API.AccountSwitchedEventArgs args)
        {
            try
            {
                _outcomeHistory.Clear();
                RebuildOutcomeAggregates();

                _archiveCalibrationSamples.Clear();
                _archiveCalibrationWins.Clear();
                _archiveLearningOutcomeCount = 0;
                _outcomeArchiveImportQueued = false;
                _outcomeArchiveImported = false;

                _outcomeArchivePrefixCache = null;
                _outcomeArchivePrefixIdentityCache = null;
                _runtimeLogPrefixCache = null;
                _runtimeLogPrefixIdentityCache = null;
                _signalTraceArchivePrefixCache = null;
                _signalTraceArchivePrefixIdentityCache = null;

                bool restored =
                    RestoreOutcomeHistory();

                if (restored)
                    PersistPortableMemorySnapshot();

                Print(
                    "CFIP outcome memory account scope switched: {0}#{1} {2} {3} | restored={4} count={5}",
                    Account.BrokerName,
                    Account.Number,
                    Account.AccountType,
                    Account.IsLive ? "LIVE" : "DEMO",
                    restored,
                    _outcomeHistory.Count);
            }
            catch (Exception ex)
            {
                _outcomeHistory.Clear();
                RebuildOutcomeAggregates();

                Print(
                    "CFIP outcome memory account switch failed: {0}",
                    ex.ToString());
            }
        }
    }
}
