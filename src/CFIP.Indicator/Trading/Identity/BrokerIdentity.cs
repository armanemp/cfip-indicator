// ============================================================================
// CFIP Indicator — BrokerIdentity.cs
// ============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        // ============================================================
                
                        private bool HasTradingPermission()
                        {
                            try
                            {
                                return Permissions.TradingPermission.IsAllowed;
                            }
                            catch
                            {
                                return false;
                            }
                        }
        
        private bool EnsureTradingPermission()
                        {
                            if (HasTradingPermission())
                            {
                                _lastTradingPermissionRequestUtc =
                                    DateTime.MinValue;
                                return true;
                            }
                
                            DateTime nowUtc =
                                TimeInUtc;
                
                            if ((nowUtc -
                                 _lastTradingPermissionRequestUtc).TotalSeconds < 3)
                                return false;
                
                            _lastTradingPermissionRequestUtc =
                                nowUtc;
                
                            try
                            {
                                bool granted =
                                    Permissions.TradingPermission.Request();
                
                                return
                                    granted &&
                                    HasTradingPermission();
                            }
                            catch (Exception ex)
                            {
                                Print(
                                    "CFIP TradingPermission request failed: {0}",
                                    ex.Message);
                                return false;
                            }
                        }
        
        private bool ValidateTradeIdentityConfiguration()
        {
            string managed;
            bool valid =
                ManagedIdentityRule.TryBuildLabel(
                    CbotIdentity.ManagedLabel,
                    InstanceId,
                    out managed);

            if (!valid)
            {
                Print(
                    "CFIP managed identity unavailable: InstanceId is empty.");
            }

            return valid;
        }

        private bool IsManagedPosition(Position position)
        {
            if (position == null ||
                position.SymbolName != SymbolName)
                return false;

            string managedLabel =
                ManagedExecutionLabel();

            return
                !string.IsNullOrWhiteSpace(managedLabel) &&
                string.Equals(
                    position.Label,
                    managedLabel,
                    StringComparison.Ordinal);
        }

        private bool IsManagedPendingOrder(PendingOrder order)
                        {
                            return
                                order != null &&
                                order.SymbolName == SymbolName &&
                                string.Equals(
                                    order.Label,
                                    PendingOrderLabel(),
                                    StringComparison.Ordinal);
                        }
        
        private string PendingOrderLabel()
                        {
                            string managedLabel =
                                ManagedExecutionLabel();

                            return string.IsNullOrWhiteSpace(managedLabel)
                                ? string.Empty
                                : managedLabel + "-PENDING";
                        }
        
        private int ManagedPendingOrderCount()
                        {
                            int count=0;
                
                            foreach (PendingOrder order in PendingOrders)
                            {
                                if (IsManagedPendingOrder(order))
                                    count++;
                            }
                
                            return count;
                        }
        
        private Position GetManagedPosition()
                        {
                            foreach (Position position in Positions)
                            {
                                if (IsManagedPosition(position) &&
                                    position.SymbolName == SymbolName)
                                    return position;
                            }
                
                            return null;
                        }
        
        private PendingOrder GetManagedPendingOrder()
                        {
                            foreach (PendingOrder order in PendingOrders)
                            {
                                if (IsManagedPendingOrder(order) &&
                                    order.SymbolName == SymbolName)
                                    return order;
                            }
                
                            return null;
                        }
    }
}
