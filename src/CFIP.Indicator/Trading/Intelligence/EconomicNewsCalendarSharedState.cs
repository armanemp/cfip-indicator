using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator
    {
        private const string EconomicNewsSharedStateSchema =
            "CFIP-NEWS,2";
        private const string EconomicNewsSharedMetadataKey =
            "CFIP EconomicNews Shared Metadata";
        private const string EconomicNewsSharedPayloadKey =
            "CFIP EconomicNews Shared Payload";
        private const int EconomicNewsSharedReloadSeconds = 15;

        private DateTime _economicNewsLastSharedStorageReloadUtc =
            DateTime.MinValue;

        private bool TryAdoptSharedEconomicNewsSnapshot(
            string uri,
            string[] relevantCurrencies,
            DateTime referenceUtc,
            out DateTime sharedLastAttemptUtc)
        {
            sharedLastAttemptUtc =
                DateTime.MinValue;

            string payload;
            DateTime successUtc;

            try
            {
                if (_economicNewsLastSharedStorageReloadUtc !=
                    DateTime.MinValue &&
                    (referenceUtc -
                     _economicNewsLastSharedStorageReloadUtc).TotalSeconds <
                    EconomicNewsSharedReloadSeconds)
                {
                    sharedLastAttemptUtc =
                        _economicNewsLastAttemptUtc;

                    if (EconomicNewsFeedCoordinator.TryReadEconomicNewsSnapshot(
                            uri,
                            _economicNewsLastSuccessUtc,
                            out payload,
                            out successUtc))
                    {
                        PublishEconomicNewsSnapshot(
                            payload,
                            successUtc,
                            relevantCurrencies);

                        return true;
                    }

                    return false;
                }

                LocalStorage.Reload(
                    LocalStorageScope.Type);

                _economicNewsLastSharedStorageReloadUtc =
                    referenceUtc;

                string metadata =
                    LocalStorage.GetString(
                        EconomicNewsSharedMetadataKey,
                        LocalStorageScope.Type);

                string[] parts =
                    string.IsNullOrWhiteSpace(metadata)
                        ? new string[0]
                        : metadata.Split('|');

                if (parts.Length >= 4 &&
                    string.Equals(
                        parts[0],
                        EconomicNewsSharedStateSchema,
                        StringComparison.Ordinal) &&
                    string.Equals(
                        parts[1],
                        uri,
                        StringComparison.OrdinalIgnoreCase))
                {
                    long attemptTicks;
                    long successTicks;

                    if (long.TryParse(
                            parts[2],
                            out attemptTicks) &&
                        attemptTicks > 0)
                    {
                        sharedLastAttemptUtc =
                            new DateTime(
                                attemptTicks,
                                DateTimeKind.Utc);
                    }

                    if (long.TryParse(
                            parts[3],
                            out successTicks) &&
                        successTicks > 0)
                    {
                        successUtc =
                            new DateTime(
                                successTicks,
                                DateTimeKind.Utc);

                        payload =
                            LocalStorage.GetString(
                                EconomicNewsSharedPayloadKey,
                                LocalStorageScope.Type);

                        if (successUtc >
                            _economicNewsLastSuccessUtc &&
                            !string.IsNullOrWhiteSpace(payload))
                        {
                            PublishEconomicNewsSnapshot(
                                payload,
                                successUtc,
                                relevantCurrencies);

                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP economic news shared-state reload failed: {0}",
                    ex.Message);
            }

            if (EconomicNewsFeedCoordinator.TryReadEconomicNewsSnapshot(
                    uri,
                    _economicNewsLastSuccessUtc,
                    out payload,
                    out successUtc))
            {
                PublishEconomicNewsSnapshot(
                    payload,
                    successUtc,
                    relevantCurrencies);

                sharedLastAttemptUtc =
                    successUtc;

                return true;
            }

            return false;
        }

        private void PublishEconomicNewsSnapshot(
            string payload,
            DateTime successUtc,
            string[] relevantCurrencies)
        {
            CfipEconomicNewsEvent[] next =
                EconomicNewsCalendarParser.Parse(
                    payload,
                    relevantCurrencies);

            lock (_economicNewsSync)
            {
                if (_economicNewsDisposed ||
                    successUtc <= _economicNewsLastSuccessUtc)
                    return;

                _economicNewsEvents =
                    next;
                _economicNewsLastSuccessUtc =
                    successUtc;
                _economicNewsLastFailureUtc =
                    DateTime.MinValue;
                _economicNewsLastError = "";
                _economicNewsFetchHealthy = true;
                _economicNewsLastAttemptUtc =
                    successUtc;
                UpdateEconomicNewsStatusUnsafe(
                    successUtc);
            }
        }

        private void PersistSharedEconomicNewsState(
            string uri,
            DateTime attemptUtc,
            DateTime successUtc,
            string payload)
        {
            try
            {
                string metadata =
                    EconomicNewsSharedStateSchema +
                    "|" +
                    uri +
                    "|" +
                    (attemptUtc == DateTime.MinValue
                        ? 0
                        : attemptUtc.Ticks).ToString() +
                    "|" +
                    (successUtc == DateTime.MinValue
                        ? 0
                        : successUtc.Ticks).ToString();

                LocalStorage.SetString(
                    EconomicNewsSharedMetadataKey,
                    metadata,
                    LocalStorageScope.Type);

                if (payload != null)
                {
                    LocalStorage.SetString(
                        EconomicNewsSharedPayloadKey,
                        payload,
                        LocalStorageScope.Type);
                }

                LocalStorage.Flush(
                    LocalStorageScope.Type);

                _economicNewsLastSharedStorageReloadUtc =
                    attemptUtc;
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP economic news shared-state persist failed: {0}",
                    ex.Message);
            }
        }
    }
}
