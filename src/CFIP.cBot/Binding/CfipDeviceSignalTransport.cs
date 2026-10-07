using System;
using cAlgo.API;
using CFIP.Contracts;

namespace CFIP.cBot.Binding
{
    internal static class CfipDeviceSignalTransport
    {
        private static bool TryReadHeartbeat(
            Robot robot,
            string indicatorInstanceId,
            out DateTime heartbeatUtc)
        {
            heartbeatUtc = DateTime.MinValue;

            if (robot == null ||
                string.IsNullOrWhiteSpace(indicatorInstanceId))
                return false;

            try
            {
                string payload =
                    robot.LocalStorage.GetString(
                        SignalBusKey.ForHeartbeat(indicatorInstanceId),
                        LocalStorageScope.Device);

                if (string.IsNullOrWhiteSpace(payload))
                    return false;

                string[] parts = payload.Split('|');
                if (parts.Length != 2)
                    return false;

                if (!long.TryParse(
                        parts[1],
                        System.Globalization.NumberStyles.Integer,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out long ticks) ||
                    ticks <= 0)
                    return false;

                heartbeatUtc =
                    new DateTime(
                        ticks,
                        DateTimeKind.Utc);

                return heartbeatUtc != DateTime.MinValue;
            }
            catch
            {
                return false;
            }
        }

        private static SignalEnvelope RefreshEnvelopeHeartbeat(
            SignalEnvelope envelope,
            DateTime heartbeatUtc)
        {
            if (envelope == null ||
                heartbeatUtc == DateTime.MinValue ||
                heartbeatUtc < envelope.ObservedUtc)
                return envelope;

            return envelope with
            {
                ObservedUtc = heartbeatUtc
            };
        }

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

                if (TryReadHeartbeat(
                        robot,
                        indicatorInstanceId,
                        out DateTime heartbeatUtc))
                    envelope =
                        RefreshEnvelopeHeartbeat(
                            envelope,
                            heartbeatUtc);

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

                if (TryReadHeartbeat(
                        robot,
                        indicatorInstanceId,
                        out DateTime heartbeatUtc) &&
                    batch.Scenarios != null)
                {
                    SignalEnvelope[] refreshed =
                        new SignalEnvelope[batch.Scenarios.Length];

                    for (int i = 0;
                         i < batch.Scenarios.Length;
                         i++)
                    {
                        refreshed[i] =
                            RefreshEnvelopeHeartbeat(
                                batch.Scenarios[i],
                                heartbeatUtc);
                    }

                    batch =
                        batch with
                        {
                            ObservedUtc = heartbeatUtc,
                            Scenarios = refreshed
                        };
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