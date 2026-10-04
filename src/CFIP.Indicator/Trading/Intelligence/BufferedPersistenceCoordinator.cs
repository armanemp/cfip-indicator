using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private const int BufferedArchiveFlushIntervalMilliseconds = 1000;
        private const int BufferedArchiveFlushBudget = 512;

        private readonly BufferedArchivePersistence _bufferedArchivePersistence =
            new BufferedArchivePersistence();

        private DateTime _lastBufferedArchiveFlushUtc =
            DateTime.MinValue;

        private bool _outcomeMemoryPersistenceDirty;
        private bool _dailyLossPersistenceDirty;
        private bool _dailyLossPersistenceForceFlush;
        private bool _alertEventDedupPersistenceDirty;

        private void MarkOutcomeMemoryPersistenceDirty()
        {
            _outcomeMemoryPersistenceDirty = true;
        }

        private void MarkDailyLossPersistenceDirty(
            bool forceFlush)
        {
            _dailyLossPersistenceDirty = true;

            if (forceFlush)
                _dailyLossPersistenceForceFlush = true;
        }

        private void FlushBufferedPersistence(
            DateTime nowUtc,
            bool force)
        {
            DateTime now =
                CanonicalTimeRule.EnsureUtc(
                    nowUtc);

            bool intervalDue =
                _lastBufferedArchiveFlushUtc ==
                    DateTime.MinValue ||
                (now -
                 _lastBufferedArchiveFlushUtc)
                .TotalMilliseconds >=
                BufferedArchiveFlushIntervalMilliseconds;

            bool archivePending =
                _bufferedArchivePersistence.PendingLineCount > 0;

            bool localStoragePending =
                _outcomeMemoryPersistenceDirty ||
                _dailyLossPersistenceDirty ||
                _alertEventDedupPersistenceDirty;

            if (!force &&
                !intervalDue &&
                !localStoragePending)
                return;

            if (archivePending &&
                (force || intervalDue))
            {
                try
                {
                    _bufferedArchivePersistence.Flush(
                        force
                            ? 4096
                            : BufferedArchiveFlushBudget);
                }
                catch (Exception ex)
                {
                    Print(
                        "CFIP buffered archive flush failed: {0}",
                        ex.Message);
                }
            }

            bool shouldFlushLocalStorage =
                force ||
                _dailyLossPersistenceForceFlush ||
                localStoragePending &&
                intervalDue;

            if (shouldFlushLocalStorage &&
                localStoragePending)
            {
                try
                {
                    LocalStorage.Flush(
                        LocalStorageScope.Type);

                    _outcomeMemoryPersistenceDirty = false;
                    _dailyLossPersistenceDirty = false;
                    _dailyLossPersistenceForceFlush = false;
                    _alertEventDedupPersistenceDirty = false;
                }
                catch (Exception ex)
                {
                    Print(
                        "CFIP buffered LocalStorage flush failed: {0}",
                        ex.Message);
                }
            }

            _lastBufferedArchiveFlushUtc =
                now;
        }

        private void FlushBufferedPersistenceOnShutdown()
        {
            try
            {
                FlushBufferedPersistence(
                    TimeInUtc,
                    true);
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP buffered persistence shutdown flush failed: {0}",
                    ex.Message);
            }
        }
    }
}
