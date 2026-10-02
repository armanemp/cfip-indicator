using System;
using cAlgo.API;
using CFIP.Contracts;

namespace cAlgo
{
    public partial class CFIPIndicator
    {
        private long _lastPublishedScenarioBatchRevision = -1;

        private void PublishDeviceSignalScenarioBatch(
            SignalScenarioBatch batch)
        {
            if (batch == null ||
                batch.ContractVersion <= 0 ||
                string.IsNullOrWhiteSpace(batch.IndicatorInstanceId) ||
                batch.Revision <= 0 ||
                batch.Revision ==
                    _lastPublishedScenarioBatchRevision)
                return;

            try
            {
                string key =
                    SignalBusKey.ForScenarioBatch(
                        InstanceId);

                string payload =
                    SignalScenarioBatchCodec.Serialize(
                        batch);

                LocalStorage.SetString(
                    key,
                    payload,
                    LocalStorageScope.Device);

                LocalStorage.Flush(
                    LocalStorageScope.Device);

                _lastPublishedScenarioBatchRevision =
                    batch.Revision;
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP SCENARIO BATCH PUBLISH FAILED | revision={0} | {1}",
                    batch.Revision,
                    ex.Message);
            }
        }
    }
}
