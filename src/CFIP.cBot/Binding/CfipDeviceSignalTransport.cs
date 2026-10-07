using System;
using cAlgo.API;
using CFIP.Contracts;

namespace CFIP.cBot.Binding
{
    internal static class CfipDeviceSignalTransport
    {
        public static bool TryRead(
            Robot robot,
            string indicatorInstanceId,
            out SignalEnvelope envelope,
            out string reason)
        {
            envelope = null;
            reason = "OK";

            if (robot == null ||
                string.IsNullOrWhiteSpace(indicatorInstanceId))
            {
                reason = "CFIP SIGNAL TRANSPORT ID UNAVAILABLE";
                return false;
            }

            try
            {
                string key =
                    SignalBusKey.ForInstance(
                        indicatorInstanceId);

                string payload =
                    robot.LocalStorage.GetString(
                        key,
                        LocalStorageScope.Device);

                if (!SignalEnvelopeCodec.TryDeserialize(
                        payload,
                        out envelope))
                {
                    reason = "CFIP SIGNAL ENVELOPE UNAVAILABLE";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                envelope = null;
                reason =
                    "CFIP SIGNAL TRANSPORT READ FAILED • " +
                    ex.Message;
                return false;
            }
        }

        public static bool TryReadScenarioBatch(
            Robot robot,
            string indicatorInstanceId,
            out SignalScenarioBatch batch,
            out string reason)
        {
            batch = null;
            reason = "OK";

            if (robot == null ||
                string.IsNullOrWhiteSpace(indicatorInstanceId))
            {
                reason = "CFIP SCENARIO BATCH TRANSPORT ID UNAVAILABLE";
                return false;
            }

            try
            {
                string key =
                    SignalBusKey.ForScenarioBatch(
                        indicatorInstanceId);

                string payload =
                    robot.LocalStorage.GetString(
                        key,
                        LocalStorageScope.Device);

                if (!SignalScenarioBatchCodec.TryDeserialize(
                        payload,
                        out batch))
                {
                    reason = "CFIP SCENARIO BATCH UNAVAILABLE";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                batch = null;
                reason =
                    "CFIP SCENARIO BATCH READ FAILED • " +
                    ex.Message;
                return false;
            }
        }

        public static void Reload(Robot robot)
        {
            if (robot == null)
                return;

            robot.LocalStorage.Reload(
                LocalStorageScope.Device);
        }
    }
}