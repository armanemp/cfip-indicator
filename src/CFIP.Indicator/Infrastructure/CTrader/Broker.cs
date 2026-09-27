using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace CFIP.Indicator
{
        public sealed class BrokerPositionSnapshot
        {
            public string BrokerPositionId { get; private set; }
            public string Label { get; private set; }
            public string Symbol { get; private set; }
            public Direction Direction { get; private set; }
            public double EntryPrice { get; private set; }
            public double VolumeInUnits { get; private set; }
            public double? StopLoss { get; private set; }
            public double? TakeProfit { get; private set; }
            public double NetProfit { get; private set; }
            public string Comment { get; private set; }
            public bool IsOpen { get; private set; }
    
            public BrokerPositionSnapshot(
                string brokerPositionId,
                string label,
                string symbol,
                Direction direction,
                double entryPrice,
                double volumeInUnits,
                double? stopLoss,
                double? takeProfit,
                double netProfit,
                string comment,
                bool isOpen)
            {
                BrokerPositionId = brokerPositionId ?? string.Empty;
                Label = label ?? string.Empty;
                Symbol = symbol ?? string.Empty;
                Direction = direction;
                EntryPrice = entryPrice;
                VolumeInUnits = volumeInUnits;
                StopLoss = stopLoss;
                TakeProfit = takeProfit;
                NetProfit = netProfit;
                Comment = comment ?? string.Empty;
                IsOpen = isOpen;
            }
        }
    
        public sealed class BrokerPendingOrderSnapshot
        {
            public string BrokerOrderId { get; private set; }
            public string Label { get; private set; }
            public string Symbol { get; private set; }
            public Direction Direction { get; private set; }
            public ExecutionKind Kind { get; private set; }
            public double RequestedEntry { get; private set; }
            public double VolumeInUnits { get; private set; }
            public double? StopLoss { get; private set; }
            public double? TakeProfit { get; private set; }
            public bool IsActive { get; private set; }
    
            public BrokerPendingOrderSnapshot(
                string brokerOrderId,
                string label,
                string symbol,
                Direction direction,
                ExecutionKind kind,
                double requestedEntry,
                double volumeInUnits,
                double? stopLoss,
                double? takeProfit,
                bool isActive)
            {
                BrokerOrderId = brokerOrderId ?? string.Empty;
                Label = label ?? string.Empty;
                Symbol = symbol ?? string.Empty;
                Direction = direction;
                Kind = kind;
                RequestedEntry = requestedEntry;
                VolumeInUnits = volumeInUnits;
                StopLoss = stopLoss;
                TakeProfit = takeProfit;
                IsActive = isActive;
            }
        }
    
        public sealed class BrokerStateSnapshot
        {
            private readonly ReadOnlyCollection<BrokerPositionSnapshot> _positions;
            private readonly ReadOnlyCollection<BrokerPendingOrderSnapshot> _pendingOrders;
    
            public IReadOnlyList<BrokerPositionSnapshot> Positions
            {
                get { return _positions; }
            }
    
            public IReadOnlyList<BrokerPendingOrderSnapshot> PendingOrders
            {
                get { return _pendingOrders; }
            }
    
            public BrokerStateSnapshot(
                IList<BrokerPositionSnapshot> positions,
                IList<BrokerPendingOrderSnapshot> pendingOrders)
            {
                _positions =
                    new ReadOnlyCollection<BrokerPositionSnapshot>(
                        new List<BrokerPositionSnapshot>(
                            positions ??
                            new List<BrokerPositionSnapshot>()));
    
                _pendingOrders =
                    new ReadOnlyCollection<BrokerPendingOrderSnapshot>(
                        new List<BrokerPendingOrderSnapshot>(
                            pendingOrders ??
                            new List<BrokerPendingOrderSnapshot>()));
            }
        }
    
        public sealed class RuntimeAuthority
        {
            public bool AutoTradingEnabled { get; private set; }
            public bool AutomaticOrdersEnabled { get; private set; }
    
            public void InitializeFromConfiguration(
                ConfigSnapshot configuration)
            {
                if (configuration == null)
                    throw new ArgumentNullException("configuration");
    
                AutoTradingEnabled =
                    configuration.Get(
                        "EnableAutoTrading",
                        false);
    
                AutomaticOrdersEnabled =
                    configuration.Get(
                        "EnableAutomaticOrders",
                        false);
            }
    
            public void SetAutoTradingEnabled(bool enabled)
            {
                AutoTradingEnabled = enabled;
            }
    
            public void SetAutomaticOrdersEnabled(bool enabled)
            {
                AutomaticOrdersEnabled = enabled;
            }
        }
    
        public sealed class CTraderBrokerGateway : IBrokerGateway
        {
            private readonly CFIP_MTF_LiveEntryEngine_Clean_v87 _host;
    
            public CTraderBrokerGateway(
                CFIP_MTF_LiveEntryEngine_Clean_v87 host)
            {
                _host = host ?? throw new ArgumentNullException("host");
            }
    
            public ExecutionResult Execute(
                ExecutionIntent intent)
            {
                if (intent == null)
                    return Failure("NULL_INTENT");
    
                try
                {
                    TradeType type =
                        intent.Direction == Direction.Buy
                            ? TradeType.Buy
                            : TradeType.Sell;
    
                    double requested = intent.RequestedEntry.Price;
                    double stop = intent.StopLoss.Price;
                    double target = intent.EffectiveTarget.Price;
                    double pip = _host.Symbol.PipSize;
    
                    if (requested <= 0 || stop <= 0 || target <= 0 ||
                        intent.VolumeInUnits <= 0 || pip <= 0)
                        return Failure("BROKER_ARGUMENT_INVALID");
    
                    double stopPips = Math.Abs(requested - stop) / pip;
                    double targetPips = Math.Abs(target - requested) / pip;
    
                    if (stopPips <= 0 || targetPips <= 0)
                        return Failure("PROTECTION_DISTANCE_INVALID");
    
                    if (!DirectionRules.IsDirectional(intent.Direction))
                        return Failure("PROTECTION_DIRECTION_INVALID");
    
                    string protectionError;
                    if (!ValidateProtectionLevels(
                            intent.Direction,
                            requested,
                            stop,
                            target,
                            out protectionError))
                        return Failure(protectionError);
    
                    string label =
                        _host.Configuration.Get(
                            "AutoTradeLabel",
                            "CFIP-SMART-CLEAN");
    
                    string comment =
                        "CFIP|SIGNAL|" +
                        intent.TradeIdentity.SignalId +
                        "|PLAN|" +
                        intent.TradeIdentity.PlanId;
    
                    TradeResult result;
    
                    if (intent.Kind == ExecutionKind.Market)
                        result = _host.ExecuteMarketOrder(
                            type,
                            _host.SymbolName,
                            intent.VolumeInUnits,
                            label,
                            stopPips,
                            targetPips,
                            comment);
                    else if (intent.Kind == ExecutionKind.Stop)
                        result = _host.PlaceStopOrder(
                            type,
                            _host.SymbolName,
                            intent.VolumeInUnits,
                            requested,
                            label,
                            stopPips,
                            targetPips,
                            ProtectionType.Relative,
                            intent.ExpiryUtc,
                            comment);
                    else if (intent.Kind == ExecutionKind.Limit)
                        result = _host.PlaceLimitOrder(
                            type,
                            _host.SymbolName,
                            intent.VolumeInUnits,
                            requested,
                            label,
                            stopPips,
                            targetPips,
                            ProtectionType.Relative,
                            intent.ExpiryUtc,
                            comment);
                    else
                        return Failure("EXECUTION_KIND_INVALID");
    
                    if (!result.IsSuccessful)
                        return Failure(
                            result.Error != null
                                ? result.Error.ToString()
                                : "BROKER_REJECTED");
    
                    if (intent.Kind == ExecutionKind.Market)
                    {
                        if (result.Position == null)
                            return Failure("POSITION_MISSING_AFTER_EXECUTION");
    
                        bool protectedPosition =
                            result.Position.StopLoss.HasValue &&
                            result.Position.TakeProfit.HasValue;
    
                        double actual =
                            result.Position.EntryPrice;
    
                        return new ExecutionResult(
                            true,
                            string.Empty,
                            result.Position.Id.ToString(),
                            new PriceLevel(
                                actual,
                                "ACTUAL_FILL",
                                Provenance.Direct(
                                    "BROKER",
                                    "MARKET_FILL")),
                            Math.Abs(actual - requested),
                            string.Empty,
                            protectedPosition
                                ? ProtectionState.FullyProtected
                                : ProtectionState.RecoveryRequired,
                            !protectedPosition,
                            "MARKET_EXECUTED");
                    }
    
                    if (result.PendingOrder == null)
                        return Failure("PENDING_ORDER_MISSING_AFTER_ACCEPT");
    
                    bool protectedPending =
                        result.PendingOrder.StopLoss.HasValue &&
                        result.PendingOrder.TakeProfit.HasValue;
    
                    return new ExecutionResult(
                        true,
                        result.PendingOrder.Id.ToString(),
                        string.Empty,
                        null,
                        0,
                        string.Empty,
                        protectedPending
                            ? ProtectionState.FullyProtected
                            : ProtectionState.RecoveryRequired,
                        !protectedPending,
                        "PENDING_ORDER_ACCEPTED");
                }
                catch (Exception ex)
                {
                    return Failure(ex.Message);
                }
            }
    
            public ExecutionResult ModifyProtection(
                string brokerPositionId,
                double? stopLoss,
                double? takeProfit)
            {
                try
                {
                    int id;
                    if (!int.TryParse(brokerPositionId, out id))
                        return Failure("POSITION_ID_INVALID");
    
                    Position position = _host.Positions.FindById(id);
                    if (position == null)
                        return Failure("POSITION_NOT_FOUND");
    
                    if (!IsManagedPosition(position))
                        return Failure("POSITION_NOT_MANAGED");
    
                    double? effectiveStop =
                        stopLoss.HasValue
                            ? stopLoss
                            : position.StopLoss;
    
                    double? effectiveTarget =
                        takeProfit.HasValue
                            ? takeProfit
                            : position.TakeProfit;
    
                    string protectionError;
                    if (!ValidateLivePositionProtection(
                            position,
                            effectiveStop,
                            effectiveTarget,
                            out protectionError))
                        return Failure(protectionError);
    
                    TradeResult result =
                        _host.ModifyPosition(
                            position,
                            stopLoss,
                            takeProfit);
    
                    return result.IsSuccessful
                        ? new ExecutionResult(
                            true,
                            string.Empty,
                            brokerPositionId,
                            null,
                            0,
                            string.Empty,
                            stopLoss.HasValue && takeProfit.HasValue
                                ? ProtectionState.FullyProtected
                                : ProtectionState.PartiallyProtected,
                            false,
                            "PROTECTION_MODIFIED")
                        : Failure(
                            result.Error != null
                                ? result.Error.ToString()
                                : "PROTECTION_MODIFICATION_REJECTED");
                }
                catch (Exception ex)
                {
                    return Failure(ex.Message);
                }
            }
    
            public ExecutionResult PartialClosePosition(
                string brokerPositionId,
                double volumeInUnits)
            {
                try
                {
                    int id;
                    if (!int.TryParse(brokerPositionId, out id))
                        return Failure("POSITION_ID_INVALID");
    
                    Position position =
                        _host.Positions.FindById(id);
    
                    if (position == null)
                        return Failure("POSITION_NOT_FOUND");
    
                    if (!IsManagedPosition(position))
                        return Failure("POSITION_NOT_MANAGED");
    
                    double volume =
                        _host.Symbol.NormalizeVolumeInUnits(
                            volumeInUnits,
                            RoundingMode.Down);
    
                    if (volume < _host.Symbol.VolumeInUnitsMin ||
                        volume >= position.VolumeInUnits)
                        return Failure("PARTIAL_VOLUME_INVALID");
    
                    TradeResult result =
                        _host.ClosePosition(
                            position,
                            volume);
    
                    return result.IsSuccessful
                        ? Success(
                            "PARTIAL_CLOSE_ACCEPTED",
                            brokerPositionId,
                            string.Empty)
                        : Failure(
                            result.Error != null
                                ? result.Error.ToString()
                                : "PARTIAL_CLOSE_REJECTED");
                }
                catch (Exception ex)
                {
                    return Failure(ex.Message);
                }
            }
    
            public ExecutionResult ClosePosition(string brokerPositionId)
            {
                try
                {
                    int id;
                    if (!int.TryParse(brokerPositionId, out id))
                        return Failure("POSITION_ID_INVALID");
    
                    Position position = _host.Positions.FindById(id);
                    if (position == null)
                        return Failure("POSITION_NOT_FOUND");
    
                    if (!IsManagedPosition(position))
                        return Failure("POSITION_NOT_MANAGED");
    
                    TradeResult result = _host.ClosePosition(position);
    
                    return result.IsSuccessful
                        ? Success("POSITION_CLOSE_ACCEPTED", brokerPositionId, string.Empty)
                        : Failure(
                            result.Error != null
                                ? result.Error.ToString()
                                : "POSITION_CLOSE_REJECTED");
                }
                catch (Exception ex)
                {
                    return Failure(ex.Message);
                }
            }
    
            public ExecutionResult ModifyPendingProtection(
                string brokerOrderId,
                double? stopLoss,
                double? takeProfit)
            {
                try
                {
                    int id;
                    if (!int.TryParse(brokerOrderId, out id))
                        return Failure("ORDER_ID_INVALID");
    
                    PendingOrder order = null;
                    foreach (var item in _host.PendingOrders)
                    {
                        if (item.Id == id)
                        {
                            order = item;
                            break;
                        }
                    }
    
                    if (order == null)
                        return Failure("PENDING_ORDER_NOT_FOUND");
    
                    if (!IsManagedPendingOrder(order))
                        return Failure("PENDING_ORDER_NOT_MANAGED");
    
                    double? effectiveStop =
                        stopLoss.HasValue
                            ? stopLoss
                            : order.StopLoss;
    
                    double? effectiveTarget =
                        takeProfit.HasValue
                            ? takeProfit
                            : order.TakeProfit;
    
                    if (!effectiveStop.HasValue ||
                        !effectiveTarget.HasValue)
                        return Failure("PENDING_PROTECTION_INCOMPLETE");
    
                    string protectionError;
                    if (!ValidateProtectionLevels(
                            order.TradeType == TradeType.Buy
                                ? Direction.Buy
                                : Direction.Sell,
                            order.TargetPrice,
                            effectiveStop,
                            effectiveTarget,
                            out protectionError))
                        return Failure(protectionError);
    
                    TradeResult result =
                        _host.ModifyPendingOrder(
                            order,
                            order.TargetPrice,
                            effectiveStop,
                            effectiveTarget,
                            ProtectionType.Absolute,
                            order.ExpirationTime);
    
                    if (!result.IsSuccessful)
                        return Failure(
                            result.Error != null
                                ? result.Error.ToString()
                                : "PENDING_PROTECTION_MODIFICATION_REJECTED");
    
                    return Success(
                        "PENDING_PROTECTION_MODIFIED",
                        brokerOrderId,
                        string.Empty);
                }
                catch (Exception ex)
                {
                    return Failure(ex.Message);
                }
            }
    
            public ExecutionResult CancelPendingOrder(string brokerOrderId)
            {
                try
                {
                    int id;
                    if (!int.TryParse(brokerOrderId, out id))
                        return Failure("ORDER_ID_INVALID");
    
                    PendingOrder found = null;
                    foreach (var order in _host.PendingOrders)
                    {
                        if (order.Id == id)
                        {
                            found = order;
                            break;
                        }
                    }
    
                    if (found == null)
                        return Failure("PENDING_ORDER_NOT_FOUND");
    
                    if (!IsManagedPendingOrder(found))
                        return Failure("PENDING_ORDER_NOT_MANAGED");
    
                    TradeResult result = _host.CancelPendingOrder(found);
    
                    return result.IsSuccessful
                        ? Success("PENDING_CANCEL_ACCEPTED", brokerOrderId, string.Empty)
                        : Failure(
                            result.Error != null
                                ? result.Error.ToString()
                                : "PENDING_CANCEL_REJECTED");
                }
                catch (Exception ex)
                {
                    return Failure(ex.Message);
                }
            }
    
            private bool IsManagedPosition(Position position)
            {
                if (position == null ||
                    _host.Configuration == null)
                    return false;
    
                string label =
                    _host.Configuration.Get(
                        "AutoTradeLabel",
                        "CFIP-SMART-CLEAN");
    
                return
                    string.Equals(
                        position.Label,
                        label,
                        StringComparison.Ordinal) &&
                    string.Equals(
                        position.SymbolName,
                        _host.SymbolName,
                        StringComparison.Ordinal) &&
                    position.Comment != null &&
                    position.Comment.IndexOf(
                        "CFIP|",
                        StringComparison.Ordinal) >= 0;
            }
    
            private bool IsManagedPendingOrder(PendingOrder order)
            {
                if (order == null ||
                    _host.Configuration == null)
                    return false;
    
                string label =
                    _host.Configuration.Get(
                        "AutoTradeLabel",
                        "CFIP-SMART-CLEAN");
    
                return
                    string.Equals(
                        order.Label,
                        label,
                        StringComparison.Ordinal) &&
                    string.Equals(
                        order.SymbolName,
                        _host.SymbolName,
                        StringComparison.Ordinal) &&
                    order.Comment != null &&
                    order.Comment.IndexOf(
                        "CFIP|",
                        StringComparison.Ordinal) >= 0;
            }
    
            private bool ValidateLivePositionProtection(
                Position position,
                double? stopLoss,
                double? takeProfit,
                out string error)
            {
                error = string.Empty;
    
                if (position == null ||
                    _host.Symbol.PipSize <= 0)
                {
                    error = "POSITION_PROTECTION_CONTEXT_INVALID";
                    return false;
                }
    
                Direction direction =
                    position.TradeType == TradeType.Buy
                        ? Direction.Buy
                        : Direction.Sell;
    
                double stopAnchor =
                    direction == Direction.Buy
                        ? _host.Symbol.Bid
                        : _host.Symbol.Ask;
    
                double targetAnchor =
                    direction == Direction.Buy
                        ? _host.Symbol.Ask
                        : _host.Symbol.Bid;
    
                if (stopLoss.HasValue)
                {
                    bool validStop =
                        direction == Direction.Buy
                            ? stopLoss.Value < stopAnchor
                            : stopLoss.Value > stopAnchor;
    
                    if (!validStop)
                    {
                        error = "LIVE_STOP_DIRECTION_INVALID";
                        return false;
                    }
    
                    double minStopPips =
                        MinimumDistancePips(
                            _host.Symbol.MinStopLossDistance);
    
                    if (minStopPips > 0 &&
                        Math.Abs(stopAnchor - stopLoss.Value) /
                        _host.Symbol.PipSize + 0.000001 <
                        minStopPips)
                    {
                        error = "BROKER_STOP_DISTANCE_INVALID";
                        return false;
                    }
                }
    
                if (takeProfit.HasValue)
                {
                    bool validTarget =
                        direction == Direction.Buy
                            ? takeProfit.Value > targetAnchor
                            : takeProfit.Value < targetAnchor;
    
                    if (!validTarget)
                    {
                        error = "LIVE_TARGET_DIRECTION_INVALID";
                        return false;
                    }
    
                    double minTargetPips =
                        MinimumDistancePips(
                            _host.Symbol.MinTakeProfitDistance);
    
                    if (minTargetPips > 0 &&
                        Math.Abs(takeProfit.Value - targetAnchor) /
                        _host.Symbol.PipSize + 0.000001 <
                        minTargetPips)
                    {
                        error = "BROKER_TARGET_DISTANCE_INVALID";
                        return false;
                    }
                }
    
                if (!stopLoss.HasValue &&
                    !takeProfit.HasValue)
                {
                    error = "PROTECTION_INCOMPLETE";
                    return false;
                }
    
                return true;
            }
    
            private bool ValidateProtectionLevels(
                Direction direction,
                double anchor,
                double? stopLoss,
                double? takeProfit,
                out string error)
            {
                error = string.Empty;
    
                if (!DirectionRules.IsDirectional(direction) ||
                    anchor <= 0 ||
                    _host.Symbol.PipSize <= 0)
                {
                    error = "PROTECTION_DIRECTION_INVALID";
                    return false;
                }
    
                if (stopLoss.HasValue)
                {
                    bool validStop =
                        direction == Direction.Buy
                            ? stopLoss.Value < anchor
                            : stopLoss.Value > anchor;
    
                    if (!validStop)
                    {
                        error = "PROTECTION_DIRECTION_INVALID";
                        return false;
                    }
    
                    double minStopPips =
                        MinimumDistancePips(
                            _host.Symbol.MinStopLossDistance);
    
                    if (minStopPips > 0 &&
                        Math.Abs(anchor - stopLoss.Value) /
                        _host.Symbol.PipSize + 0.000001 <
                        minStopPips)
                    {
                        error = "BROKER_STOP_DISTANCE_INVALID";
                        return false;
                    }
                }
    
                if (takeProfit.HasValue)
                {
                    bool validTarget =
                        direction == Direction.Buy
                            ? takeProfit.Value > anchor
                            : takeProfit.Value < anchor;
    
                    if (!validTarget)
                    {
                        error = "TARGET_DIRECTION_INVALID";
                        return false;
                    }
    
                    double minTargetPips =
                        MinimumDistancePips(
                            _host.Symbol.MinTakeProfitDistance);
    
                    if (minTargetPips > 0 &&
                        Math.Abs(takeProfit.Value - anchor) /
                        _host.Symbol.PipSize + 0.000001 <
                        minTargetPips)
                    {
                        error = "BROKER_TARGET_DISTANCE_INVALID";
                        return false;
                    }
                }
    
                if (!stopLoss.HasValue &&
                    !takeProfit.HasValue)
                {
                    error = "PROTECTION_INCOMPLETE";
                    return false;
                }
    
                return true;
            }
    
            private double MinimumDistancePips(double rawDistance)
            {
                if (rawDistance <= 0 ||
                    _host.Symbol.PipSize <= 0)
                    return 0;
    
                if (_host.Symbol.MinDistanceType ==
                    SymbolMinDistanceType.Pips)
                    return rawDistance;
    
                double reference =
                    Math.Max(
                        _host.Symbol.Ask,
                        _host.Symbol.Bid);
    
                return reference > 0
                    ? reference * rawDistance / 100.0 /
                      _host.Symbol.PipSize
                    : 0;
            }
    
            private ExecutionResult Success(
                string detail,
                string orderId,
                string positionId)
            {
                return new ExecutionResult(
                    true,
                    orderId,
                    positionId,
                    null,
                    0,
                    string.Empty,
                    ProtectionState.Unknown,
                    false,
                    detail);
            }
    
            private ExecutionResult Failure(string error)
            {
                return new ExecutionResult(
                    false,
                    string.Empty,
                    string.Empty,
                    null,
                    0,
                    error,
                    ProtectionState.Unknown,
                    false,
                    "EXECUTION_FAILED");
            }
        }
    
        public sealed class CTraderBrokerStateReader : IBrokerStateReader
        {
            private readonly CFIP_MTF_LiveEntryEngine_Clean_v87 _host;
    
            public CTraderBrokerStateReader(
                CFIP_MTF_LiveEntryEngine_Clean_v87 host)
            {
                _host = host ?? throw new ArgumentNullException("host");
            }
    
            public BrokerStateSnapshot ReadManagedState(
                string symbol,
                string strategyId)
            {
                string label =
                    _host.Configuration.Get(
                        "AutoTradeLabel",
                        "CFIP-SMART-CLEAN");
    
                var positions = new List<BrokerPositionSnapshot>();
                foreach (var position in _host.Positions)
                {
                    if (!string.Equals(position.Label, label, StringComparison.Ordinal) ||
                        !string.Equals(position.SymbolName, symbol, StringComparison.Ordinal) ||
                        !HasCurrentIdentity(position.Comment))
                        continue;
    
                    positions.Add(
                        new BrokerPositionSnapshot(
                            position.Id.ToString(),
                            position.Label,
                            position.SymbolName,
                            position.TradeType == TradeType.Buy
                                ? Direction.Buy
                                : Direction.Sell,
                            position.EntryPrice,
                            position.VolumeInUnits,
                            position.StopLoss,
                            position.TakeProfit,
                            position.NetProfit,
                            position.Comment,
                            true));
                }
    
                var orders = new List<BrokerPendingOrderSnapshot>();
                foreach (var order in _host.PendingOrders)
                {
                    if (!string.Equals(order.Label, label, StringComparison.Ordinal) ||
                        !string.Equals(order.SymbolName, symbol, StringComparison.Ordinal) ||
                        !HasCurrentIdentity(order.Comment))
                        continue;
    
                    var kind =
                        order.OrderType == PendingOrderType.Stop
                            ? ExecutionKind.Stop
                            : order.OrderType == PendingOrderType.Limit
                                ? ExecutionKind.Limit
                                : ExecutionKind.None;
    
                    orders.Add(
                        new BrokerPendingOrderSnapshot(
                            order.Id.ToString(),
                            order.Label,
                            order.SymbolName,
                            order.TradeType == TradeType.Buy
                                ? Direction.Buy
                                : Direction.Sell,
                            kind,
                            order.TargetPrice,
                            order.VolumeInUnits,
                            order.StopLoss,
                            order.TakeProfit,
                            true));
                }
    
                return new BrokerStateSnapshot(
                    positions,
                    orders);
            }
    
            private bool HasCurrentIdentity(string comment)
            {
                return
                    comment != null &&
                    comment.IndexOf(
                        "CFIP|",
                        StringComparison.Ordinal) >= 0;
            }
        }
    
        public interface IBrokerGateway
        {
            ExecutionResult Execute(
                ExecutionIntent intent);
    
            ExecutionResult ModifyProtection(
                string brokerPositionId,
                double? stopLoss,
                double? takeProfit);
    
            ExecutionResult ClosePosition(
                string brokerPositionId);
    
            ExecutionResult PartialClosePosition(
                string brokerPositionId,
                double volumeInUnits);
    
            ExecutionResult CancelPendingOrder(
                string brokerOrderId);
    
            ExecutionResult ModifyPendingProtection(
                string brokerOrderId,
                double? stopLoss,
                double? takeProfit);
        }
    
        public interface IBrokerStateReader
        {
            BrokerStateSnapshot ReadManagedState(
                string symbol,
                string strategyId);
        }
    
    
}
