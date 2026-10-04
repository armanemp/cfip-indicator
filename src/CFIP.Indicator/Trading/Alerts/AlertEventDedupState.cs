using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator
    {
        private const string AlertEventDedupStorageKey =
            "CFIP Alert Event Dedup";

        private const int AlertEventDedupStorageReloadSeconds =
            5;

        private DateTime _lastAlertEventDedupStorageReloadUtc =
            DateTime.MinValue;

        private bool _alertEventDedupPersistenceDirty;

        private void RefreshSharedAlertEventDedupState(
            DateTime nowUtc)
        {
            DateTime now =
                CanonicalTimeRule.EnsureUtc(
                    nowUtc);

            if (_lastAlertEventDedupStorageReloadUtc !=
                    DateTime.MinValue &&
                (now -
                 _lastAlertEventDedupStorageReloadUtc)
                .TotalSeconds <
                AlertEventDedupStorageReloadSeconds)
            {
                return;
            }

            try
            {
                LocalStorage.Reload(
                    LocalStorageScope.Type);

                string payload =
                    LocalStorage.GetString(
                        AlertEventDedupStorageKey,
                        LocalStorageScope.Type);

                AlertEventDedupCoordinator.Import(
                    payload,
                    now);

                _lastAlertEventDedupStorageReloadUtc =
                    now;
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP alert dedup shared-state reload failed: {0}",
                    ex.Message);
            }
        }

        private bool TryClaimSharedAlertEvent(
            string eventKey,
            DateTime nowUtc)
        {
            if (!AlertEventDedupCoordinator.TryClaim(
                    eventKey,
                    nowUtc))
            {
                return false;
            }

            PersistSharedAlertEventDedupState(
                nowUtc);

            return true;
        }

        private void ReleaseSharedAlertEvent(
            string eventKey,
            DateTime nowUtc)
        {
            AlertEventDedupCoordinator.Release(
                eventKey);

            PersistSharedAlertEventDedupState(
                nowUtc);
        }

        private void PersistSharedAlertEventDedupState(
            DateTime nowUtc)
        {
            try
            {
                LocalStorage.SetString(
                    AlertEventDedupStorageKey,
                    AlertEventDedupCoordinator.Serialize(
                        nowUtc),
                    LocalStorageScope.Type);

                _alertEventDedupPersistenceDirty =
                    true;

                _lastAlertEventDedupStorageReloadUtc =
                    CanonicalTimeRule.EnsureUtc(
                        nowUtc);
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP alert dedup shared-state persist queue failed: {0}",
                    ex.Message);
            }
        }

        private bool ConsumeAlertEventDedupPersistenceDirty()
        {
            if (!_alertEventDedupPersistenceDirty)
                return false;

            _alertEventDedupPersistenceDirty =
                false;

            return true;
        }
    }
}