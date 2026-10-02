using System;
using cAlgo.API;
using CFIP.Contracts;

namespace cAlgo
{
    public partial class CFIPIndicator
    {
        private long _lastPublishedSignalBusRevision = -1;

        private void PublishDeviceSignalEnvelope(
            SignalEnvelope envelope)
        {
            if (envelope == null ||
                envelope.Identity == null)
                return;

            long revision =
                envelope.Identity.Revision;

            if (revision <= 0 ||
                revision == _lastPublishedSignalBusRevision)
                return;

            try
            {
                string key =
                    SignalBusKey.ForIndicatorInstance(
                        InstanceId);

                string payload =
                    SignalEnvelopeCodec.Serialize(
                        envelope);

                LocalStorage.SetString(
                    key,
                    payload,
                    LocalStorageScope.Device);

                LocalStorage.Flush(
                    LocalStorageScope.Device);

                _lastPublishedSignalBusRevision =
                    revision;
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP SIGNAL BUS PUBLISH FAILED | revision={0} | {1}",
                    revision,
                    ex.Message);
            }
        }
    }
}
