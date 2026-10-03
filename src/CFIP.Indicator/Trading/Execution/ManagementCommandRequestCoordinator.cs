// CFIP Indicator — ManagementCommandRequestCoordinator.cs
// Indicator-side management command publisher. No broker mutation is allowed here.

using System;
using System.Collections.Generic;
using System.Globalization;
using cAlgo.API;
using cAlgo.API.Internals;
using CFIP.Contracts;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private const int MaxManagementCommands = 64;
        private const int MaxRememberedManagementConfirmations = 128;

        private readonly HashSet<string> _confirmedManagementKeys =
            new HashSet<string>(StringComparer.Ordinal);

        private readonly Queue<string> _confirmedManagementOrder =
            new Queue<string>();

        private readonly HashSet<string> _expiredManagementKeys =
            new HashSet<string>(StringComparer.Ordinal);

        private readonly Queue<string> _expiredManagementOrder =
            new Queue<string>();

        private long _managementCommandRevision;

        private ManagementCommand[] _managementCommands =
            Array.Empty<ManagementCommand>();

        private bool _managementCommandsLoaded;
        private bool _managementCommandsPersistenceDirty;

        private ManagementCommandRequestStatus RequestCancelPendingOrder(PendingOrder order, string context)
        {
            if (order == null)
                return ManagementCommandRequestStatus.Rejected;

            return RequestManagementCommand(
                ManagementCommandType.CancelPending,
                null, order, null, null, null, null, null, null, null, null, null, null, context);
        }

        private ManagementCommandRequestStatus RequestClosePosition(Position position, string context, double? volumeInUnits = null)
        {
            if (position == null)
                return ManagementCommandRequestStatus.Rejected;

            double closeVolume = volumeInUnits.HasValue
                ? Math.Max(0, Math.Min(volumeInUnits.Value, position.VolumeInUnits))
                : position.VolumeInUnits;

            if (closeVolume <= 0)
                return ManagementCommandRequestStatus.Rejected;

            double expectedRemaining =
                Math.Max(0, position.VolumeInUnits - closeVolume);

            if (expectedRemaining < Symbol.VolumeInUnitsMin)
                expectedRemaining = 0;

            return RequestManagementCommand(
                volumeInUnits.HasValue ? ManagementCommandType.PartialClose : ManagementCommandType.FullClose,
                position, null, null, null, null, volumeInUnits, expectedRemaining,
                null, null, null, null, null, context);
        }

        private ManagementCommandRequestStatus RequestModifyStopLoss(Position position, double price, string context)
        {
            if (position == null || !IsFinitePositive(price))
                return ManagementCommandRequestStatus.Rejected;

            return RequestManagementCommand(
                ManagementCommandType.ModifyProtection,
                position, null, NormalizePrice(price), null, null, null, null,
                null, null, null, null, null, context);
        }

        private ManagementCommandRequestStatus RequestModifyTakeProfit(Position position, double price, string context)
        {
            if (position == null || !IsFinitePositive(price))
                return ManagementCommandRequestStatus.Rejected;

            return RequestManagementCommand(
                ManagementCommandType.ModifyProtection,
                position, null, null, NormalizePrice(price), null, null, null,
                null, null, null, null, null, context);
        }

        private ManagementCommandRequestStatus RequestModifyTakeProfitLadder(
            Position position,
            double firstVolume,
            double firstTargetPips,
            double? secondVolume,
            double? secondTargetPips,
            double finalTargetPips,
            string context)
        {
            if (position == null ||
                !IsFinitePositive(firstVolume) ||
                !IsFinitePositive(firstTargetPips) ||
                !IsFinitePositive(finalTargetPips))
                return ManagementCommandRequestStatus.Rejected;

            return RequestManagementCommand(
                ManagementCommandType.AdvanceTarget,
                position, null, null, null, null, null, null,
                firstVolume, firstTargetPips, secondVolume, secondTargetPips,
                finalTargetPips, context);
        }

        private ManagementCommandRequestStatus RequestModifyTakeProfitPips(Position position, double targetPips, string context)
        {
            if (position == null || !IsFinitePositive(targetPips))
                return ManagementCommandRequestStatus.Rejected;

            return RequestManagementCommand(
                ManagementCommandType.AdvanceTarget,
                position, null, null, null, targetPips, null, null,
                null, null, null, null, null, context);
        }

        private ManagementCommandRequestStatus RequestManagementCommand(
            ManagementCommandType command,
            Position position,
            PendingOrder pendingOrder,
            double? desiredStop,
            double? desiredTarget,
            double? desiredTargetPips,
            double? partialCloseVolume,
            double? expectedRemainingVolume,
            double? ladderFirstVolume,
            double? ladderFirstTargetPips,
            double? ladderSecondVolume,
            double? ladderSecondTargetPips,
            double? ladderFinalTargetPips,
            string context)
        {
            ContractIdentity identity =
                _cfipProviderEnvelope == null ? null : _cfipProviderEnvelope.Identity;

            if (identity == null ||
                string.IsNullOrWhiteSpace(identity.PlanId) ||
                identity.Symbol != (SymbolName ?? ""))
                return ManagementCommandRequestStatus.Rejected;

            string executionLabel = ManagedExecutionLabel();
            if (string.IsNullOrWhiteSpace(executionLabel))
                return ManagementCommandRequestStatus.Rejected;

            long? positionId = position == null ? (long?)null : position.Id;
            long? pendingId = pendingOrder == null ? (long?)null : pendingOrder.Id;

            string key = BuildManagementIdempotencyKey(
                identity, command, positionId, pendingId, executionLabel,
                desiredStop, desiredTarget, desiredTargetPips,
                partialCloseVolume, expectedRemainingVolume,
                ladderFirstVolume, ladderFirstTargetPips,
                ladderSecondVolume, ladderSecondTargetPips, ladderFinalTargetPips);

            if (_confirmedManagementKeys.Contains(key))
                return ManagementCommandRequestStatus.AlreadyConfirmed;

            if (_expiredManagementKeys.Contains(key))
                return ManagementCommandRequestStatus.AlreadyExpired;

            ManagementCommand[] current = LoadManagementCommands();
            for (int i = 0; i < current.Length; i++)
            {
                if (current[i] != null &&
                    string.Equals(current[i].CommandIdempotencyKey, key, StringComparison.Ordinal))
                    return ManagementCommandRequestStatus.AlreadyPending;
            }

            ManagementCommand request =
                new ManagementCommand(
                    identity,
                    command,
                    ++_managementCommandRevision,
                    positionId,
                    pendingId,
                    desiredStop,
                    desiredTarget,
                    partialCloseVolume,
                    Server.TimeInUtc,
                    context ?? "CFIP MANAGEMENT",
                    key)
                {
                    ExecutionLabel = executionLabel,
                    DesiredTargetPips = desiredTargetPips,
                    ExpectedRemainingVolume = expectedRemainingVolume,
                    LadderFirstVolume = ladderFirstVolume,
                    LadderFirstTargetPips = ladderFirstTargetPips,
                    LadderSecondVolume = ladderSecondVolume,
                    LadderSecondTargetPips = ladderSecondTargetPips,
                    LadderFinalTargetPips = ladderFinalTargetPips
                };

            List<ManagementCommand> commands =
                new List<ManagementCommand>(current);

            commands.Add(request);
            while (commands.Count > MaxManagementCommands)
                commands.RemoveAt(0);

            _managementCommands = commands.ToArray();
            _managementCommandsLoaded = true;
            _managementCommandsPersistenceDirty = true;

            return ManagementCommandRequestStatus.Queued;
        }

        private ManagementCommand[] LoadManagementCommands()
        {
            if (_managementCommandsLoaded)
                return _managementCommands;

            try
            {
                string payload =
                    LocalStorage.GetString(
                        ManagementBusKey.CommandKeyForInstance(InstanceId),
                        LocalStorageScope.Device);

                if (!ManagementCommandCodec.TryDeserialize(payload, out ManagementCommand[] commands) ||
                    commands == null)
                {
                    _managementCommands = Array.Empty<ManagementCommand>();
                    _managementCommandsLoaded = true;
                    return _managementCommands;
                }

                List<ManagementCommand> unique = new List<ManagementCommand>();
                HashSet<string> keys = new HashSet<string>(StringComparer.Ordinal);

                for (int i = 0; i < commands.Length; i++)
                {
                    ManagementCommand command = commands[i];
                    if (command == null ||
                        string.IsNullOrWhiteSpace(command.CommandIdempotencyKey) ||
                        !keys.Add(command.CommandIdempotencyKey))
                        continue;

                    unique.Add(command);
                }

                _managementCommands = unique.ToArray();

                for (int i = 0; i < _managementCommands.Length; i++)
                {
                    if (_managementCommands[i] != null)
                    {
                        _managementCommandRevision =
                            Math.Max(
                                _managementCommandRevision,
                                _managementCommands[i].CommandRevision);
                    }
                }

                _managementCommandsLoaded = true;
                return _managementCommands;
            }
            catch (Exception ex)
            {
                Print("CFIP MANAGEMENT COMMAND READ FAILED | {0}", ex.Message);
                _managementCommands = Array.Empty<ManagementCommand>();
                _managementCommandsLoaded = true;
                return _managementCommands;
            }
        }

        private void ProcessManagementReports()
        {
            try
            {
                string payload =
                    LocalStorage.GetString(
                        ManagementBusKey.ReportKeyForInstance(InstanceId),
                        LocalStorageScope.Device);

                if (!BrokerExecutionReportCodec.TryDeserialize(
                        payload,
                        out BrokerExecutionReport[] reports) ||
                    reports == null)
                    return;

                List<ManagementCommand> pending =
                    new List<ManagementCommand>(LoadManagementCommands());

                bool changed = false;
                bool newTerminalReport = false;

                for (int i = 0; i < reports.Length; i++)
                {
                    BrokerExecutionReport report = reports[i];
                    if (report == null ||
                        string.IsNullOrWhiteSpace(report.CommandIdempotencyKey) ||
                        (report.Status != BrokerReportStatus.Confirmed &&
                         report.Status != BrokerReportStatus.Expired))
                        continue;

                    if (report.Status == BrokerReportStatus.Confirmed)
                    {
                        if (_confirmedManagementKeys.Add(report.CommandIdempotencyKey))
                        {
                            _expiredManagementKeys.Remove(
                                report.CommandIdempotencyKey);
                            _confirmedManagementOrder.Enqueue(
                                report.CommandIdempotencyKey);
                            newTerminalReport = true;

                            ApplyBrokerConfirmedProtectionState(
                                report.BrokerPositionId,
                                report.ConfirmedEntry,
                                report.ConfirmedStop,
                                report.ConfirmedTarget,
                                true);
                        }
                    }
                    else if (_expiredManagementKeys.Add(
                                 report.CommandIdempotencyKey))
                    {
                        _confirmedManagementKeys.Remove(
                            report.CommandIdempotencyKey);
                        _expiredManagementOrder.Enqueue(
                            report.CommandIdempotencyKey);
                        newTerminalReport = true;
                    }

                    for (int j = pending.Count - 1; j >= 0; j--)
                    {
                        if (string.Equals(
                                pending[j].CommandIdempotencyKey,
                                report.CommandIdempotencyKey,
                                StringComparison.Ordinal))
                        {
                            pending.RemoveAt(j);
                            changed = true;
                        }
                    }
                }

                if (changed)
                    PersistManagementCommands(pending);

                TrimConfirmedKeys();
                TrimExpiredKeys();

                if (newTerminalReport)
                    MarkBrokerStateDirty();
            }
            catch (Exception ex)
            {
                Print("CFIP MANAGEMENT REPORT RECONCILIATION FAILED | {0}", ex.Message);
            }
        }

        private void PersistManagementCommands(List<ManagementCommand> commands)
        {
            _managementCommands =
                (commands ?? new List<ManagementCommand>()).ToArray();
            _managementCommandsLoaded = true;
            _managementCommandsPersistenceDirty = true;
        }

        private bool HasPendingManagementCommandPersistence()
        {
            return _managementCommandsPersistenceDirty;
        }

        private void MarkManagementCommandPersistenceFlushed()
        {
            _managementCommandsPersistenceDirty = false;
        }

        private bool FlushManagementCommandPersistenceToLocalStorage()
        {
            if (!_managementCommandsPersistenceDirty)
                return true;

            try
            {
                LocalStorage.SetString(
                    ManagementBusKey.CommandKeyForInstance(InstanceId),
                    ManagementCommandCodec.Serialize(_managementCommands),
                    LocalStorageScope.Device);

                return true;
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP MANAGEMENT COMMAND WRITE FAILED | {0}",
                    ex.Message);
                return false;
            }
        }

        private void TrimConfirmedKeys()
        {
            while (_confirmedManagementKeys.Count >
                   MaxRememberedManagementConfirmations &&
                   _confirmedManagementOrder.Count > 0)
            {
                string oldest = _confirmedManagementOrder.Dequeue();
                _confirmedManagementKeys.Remove(oldest);
            }
        }

        private void TrimExpiredKeys()
        {
            while (_expiredManagementKeys.Count >
                   MaxRememberedManagementConfirmations &&
                   _expiredManagementOrder.Count > 0)
            {
                string oldest = _expiredManagementOrder.Dequeue();
                _expiredManagementKeys.Remove(oldest);
            }
        }

        private static string BuildManagementIdempotencyKey(
            ContractIdentity identity,
            ManagementCommandType command,
            long? positionId,
            long? pendingOrderId,
            string executionLabel,
            double? desiredStop,
            double? desiredTarget,
            double? desiredTargetPips,
            double? partialCloseVolume,
            double? expectedRemainingVolume,
            double? ladderFirstVolume,
            double? ladderFirstTargetPips,
            double? ladderSecondVolume,
            double? ladderSecondTargetPips,
            double? ladderFinalTargetPips)
        {
            return string.Join(
                "|",
                "CFIP-MGMT",
                identity.PlanId ?? "",
                identity.SignalId ?? "",
                command.ToString(),
                positionId.HasValue ? positionId.Value.ToString(CultureInfo.InvariantCulture) : "",
                pendingOrderId.HasValue ? pendingOrderId.Value.ToString(CultureInfo.InvariantCulture) : "",
                executionLabel ?? "",
                Format(desiredStop),
                Format(desiredTarget),
                Format(desiredTargetPips),
                Format(partialCloseVolume),
                Format(expectedRemainingVolume),
                Format(ladderFirstVolume),
                Format(ladderFirstTargetPips),
                Format(ladderSecondVolume),
                Format(ladderSecondTargetPips),
                Format(ladderFinalTargetPips));
        }

        private static string Format(double? value) =>
            value.HasValue
                ? value.Value.ToString("R", CultureInfo.InvariantCulture)
                : "";
    }
}
