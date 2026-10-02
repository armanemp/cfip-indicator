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
                envelope.Identity == null ||
                envelope.Identity.Revision <= 0 ||
                envelope.Identity.Revision ==
                    _lastPublishedSignalBusRevision)
                return;

            try
            {
                string key =
                    SignalBusKey.ForInstance(InstanceId);

                string payload =
                    SignalEnvelopeCodec.Serialize(envelope);

                LocalStorage.SetString(
                    key,
                    payload,
                    LocalStorageScope.Device);

                LocalStorage.Flush(
                    LocalStorageScope.Device);

                _lastPublishedSignalBusRevision =
                    envelope.Identity.Revision;
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP SIGNAL BUS PUBLISH FAILED | revision={0} | {1}",
                    envelope.Identity.Revision,
                    ex.Message);
            }
        }
    }
}