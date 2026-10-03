using System;
using System.Collections.Generic;
using CFIP.Contracts;

namespace cAlgo
{
    internal sealed class AlertDeliveryQueue
    {
        private readonly int _capacity;
        private readonly Queue<AlertDelivery> _critical =
            new Queue<AlertDelivery>();
        private readonly Queue<AlertDelivery> _normal =
            new Queue<AlertDelivery>();
        private readonly HashSet<string> _pendingAlertIds =
            new HashSet<string>(StringComparer.Ordinal);

        public AlertDeliveryQueue(int capacity)
        {
            _capacity = Math.Max(1, capacity);
        }

        public int Count
        {
            get { return _critical.Count + _normal.Count; }
        }

        public bool Enqueue(AlertDelivery delivery)
        {
            if (string.IsNullOrWhiteSpace(delivery.Message))
                return false;

            string alertId =
                GetAlertId(delivery);

            // Transport idempotency must be independent of alert-envelope revision
            // and generated AlertId text. Recalculation/reconnect may create a new
            // envelope instance for the same causal event; the canonical identity
            // below keeps that event pending only once.
            // Idempotency at the transport boundary prevents the same canonical
            // event from being queued twice by independent calculation/timer
            // callers while still allowing the same key to be re-alerted later
            // after it has actually been delivered and its normal cooldown expires.
            if (!string.IsNullOrWhiteSpace(alertId) &&
                _pendingAlertIds.Contains(alertId))
                return false;

            if (Count >= _capacity)
            {
                if (delivery.Critical)
                {
                    if (_normal.Count > 0)
                    {
                        AlertDelivery evicted =
                            _normal.Dequeue();
                        RemovePendingAlertId(evicted);
                    }
                    else if (_critical.Count > 0)
                    {
                        AlertDelivery evicted =
                            _critical.Dequeue();
                        RemovePendingAlertId(evicted);
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            if (delivery.Critical)
                _critical.Enqueue(delivery);
            else
                _normal.Enqueue(delivery);

            if (!string.IsNullOrWhiteSpace(alertId))
                _pendingAlertIds.Add(alertId);

            return true;
        }

        public bool TryPeek(out AlertDelivery delivery)
        {
            if (_critical.Count > 0)
            {
                delivery = _critical.Peek();
                return true;
            }

            if (_normal.Count > 0)
            {
                delivery = _normal.Peek();
                return true;
            }

            delivery = default(AlertDelivery);
            return false;
        }

        public bool TryDequeue(out AlertDelivery delivery)
        {
            if (_critical.Count > 0)
            {
                delivery = _critical.Dequeue();
                RemovePendingAlertId(delivery);
                return true;
            }

            if (_normal.Count > 0)
            {
                delivery = _normal.Dequeue();
                RemovePendingAlertId(delivery);
                return true;
            }

            delivery = default(AlertDelivery);
            return false;
        }

        public void ClearPendingAlerts()
        {
            _critical.Clear();
            _normal.Clear();
            _pendingAlertIds.Clear();
        }

        private static string GetAlertId(
            AlertDelivery delivery)
        {
            if (delivery.Envelope == null ||
                delivery.Envelope.Identity == null)
                return "";

            ContractIdentity identity =
                delivery.Envelope.Identity;

            return
                (identity.Symbol ?? string.Empty) +
                "|" +
                (identity.SignalId ?? string.Empty) +
                "|" +
                (identity.ScenarioId ?? string.Empty) +
                "|" +
                (identity.PlanId ?? string.Empty) +
                "|" +
                identity.CreatedClosedM5.ToString(
                    System.Globalization.CultureInfo.InvariantCulture) +
                "|" +
                identity.Direction.ToString() +
                "|" +
                (delivery.Envelope.AlertKey ?? string.Empty);
        }

        private void RemovePendingAlertId(
            AlertDelivery delivery)
        {
            string alertId =
                GetAlertId(delivery);

            if (!string.IsNullOrWhiteSpace(alertId))
                _pendingAlertIds.Remove(alertId);
        }
    }
}