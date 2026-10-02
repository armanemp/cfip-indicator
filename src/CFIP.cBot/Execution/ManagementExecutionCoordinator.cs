using System;
using System.Collections.Generic;
using cAlgo.API;
using CFIP.Contracts;

namespace CFIP.cBot.Execution
{
    internal sealed class ManagementExecutionCoordinator
    {
        private const int MaxReports = 128;
        private const int RetryDelaySeconds = 1;

        public bool Process(
            Robot robot,
            string instanceId,
            DateTime nowUtc,
            int maximumCommandAgeSeconds,
            out string status)
        {
            status = "NO MANAGEMENT COMMAND";

            if (robot == null || string.IsNullOrWhiteSpace(instanceId))
            {
                status = "MANAGEMENT TRANSPORT ID UNAVAILABLE";
                return false;
            }

            if (!TryLoadCommands(robot, instanceId, out ManagementCommand[] commands))
            {
                status = "MANAGEMENT COMMAND QUEUE UNAVAILABLE";
                return false;
            }

            if (commands == null || commands.Length == 0)
                return false;

            TryLoadReports(robot, instanceId, out List<BrokerExecutionReport> reports);

            for (int i = 0; i < commands.Length; i++)
            {
                ManagementCommand command = commands[i];
                if (command == null || string.IsNullOrWhiteSpace(command.CommandIdempotencyKey))
                    continue;

                BrokerExecutionReport latest =
                    FindLatestReport(reports, command.CommandIdempotencyKey);

                if (latest != null && latest.Status == BrokerReportStatus.Confirmed)
                    continue;

                if (latest != null &&
                    (latest.Status == BrokerReportStatus.Accepted ||
                     latest.Status == BrokerReportStatus.Submitted))
                {
                    if (TryConfirmFromBrokerState(robot, command, out BrokerExecutionReport confirmed))
                    {
                        StoreReport(robot, instanceId, reports, confirmed);
                        status = "MANAGEMENT CONFIRMED FROM BROKER STATE";
                        return true;
                    }

                    continue;
                }

                if (latest != null &&
                    (latest.Status == BrokerReportStatus.Rejected ||
                     latest.Status == BrokerReportStatus.RecoveryRequired) &&
                    latest.EventUtc.AddSeconds(RetryDelaySeconds) > nowUtc)
                    continue;

                bool expired;
                if (!Validate(
                        command,
                        robot,
                        nowUtc,
                        maximumCommandAgeSeconds,
                        out string validationReason,
                        out expired))
                {
                    StoreReport(
                        robot,
                        instanceId,
                        reports,
                        BuildReport(
                            command,
                            BrokerAction.None,
                            expired
                                ? BrokerReportStatus.Expired
                                : BrokerReportStatus.Rejected,
                            nowUtc,
                            null,
                            validationReason));
                    status = validationReason;
                    return true;
                }

                ExecuteOne(
                    robot,
                    instanceId,
                    nowUtc,
                    command,
                    reports,
                    out status);
                return true;
            }

            return false;
        }

        private void ExecuteOne(
            Robot robot,
            string instanceId,
            DateTime nowUtc,
            ManagementCommand command,
            List<BrokerExecutionReport> reports,
            out string status)
        {
            switch (command.Command)
            {
                case ManagementCommandType.CancelPending:
                    CancelPending(robot, instanceId, nowUtc, command, reports, out status);
                    return;
                case ManagementCommandType.FullClose:
                    Close(robot, instanceId, nowUtc, command, reports, false, out status);
                    return;
                case ManagementCommandType.PartialClose:
                    Close(robot, instanceId, nowUtc, command, reports, true, out status);
                    return;
                case ManagementCommandType.ModifyProtection:
                case ManagementCommandType.BreakEven:
                    Protect(robot, instanceId, nowUtc, command, reports, out status);
                    return;
                case ManagementCommandType.AdvanceTarget:
                    AdvanceTarget(robot, instanceId, nowUtc, command, reports, out status);
                    return;
                case ManagementCommandType.Keep:
                    StoreReport(
                        robot, instanceId, reports,
                        BuildReport(command, BrokerAction.None, BrokerReportStatus.Confirmed,
                            nowUtc, null, "MANAGEMENT KEEP"));
                    status = "MANAGEMENT KEEP CONFIRMED";
                    return;
                default:
                    StoreReport(
                        robot, instanceId, reports,
                        BuildReport(command, BrokerAction.None, BrokerReportStatus.Rejected,
                            nowUtc, null, "UNSUPPORTED MANAGEMENT COMMAND"));
                    status = "UNSUPPORTED MANAGEMENT COMMAND";
                    return;
            }
        }

        private void CancelPending(
            Robot robot,
            string instanceId,
            DateTime nowUtc,
            ManagementCommand command,
            List<BrokerExecutionReport> reports,
            out string status)
        {
            PendingOrder order = FindPending(robot, command);
            if (order == null)
            {
                StoreReport(robot, instanceId, reports,
                    BuildReport(command, BrokerAction.CancelPending,
                        BrokerReportStatus.Confirmed, nowUtc, null,
                        "PENDING ABSENT"));
                status = "PENDING CANCELLATION CONFIRMED";
                return;
            }

            try
            {
                TradeResult result = robot.CancelPendingOrder(order);
                if (result == null)
                {
                    StoreReport(robot, instanceId, reports,
                        BuildReport(command, BrokerAction.CancelPending,
                            BrokerReportStatus.RecoveryRequired, nowUtc, null,
                            "NULL CANCEL RESULT"));
                    status = "PENDING CANCEL RECOVERY REQUIRED";
                    return;
                }

                if (!result.IsSuccessful)
                {
                    string reason = result.Error.HasValue
                        ? result.Error.Value.ToString()
                        : "BROKER REJECTED PENDING CANCELLATION";

                    StoreReport(robot, instanceId, reports,
                        BuildReport(command, BrokerAction.CancelPending,
                            BrokerReportStatus.Rejected, nowUtc, result, reason));
                    status = reason;
                    return;
                }

                PendingOrder refreshed = FindPending(robot, command);
                BrokerReportStatus state =
                    refreshed == null
                        ? BrokerReportStatus.Confirmed
                        : BrokerReportStatus.Accepted;

                StoreReport(robot, instanceId, reports,
                    BuildReport(command, BrokerAction.CancelPending,
                        state, nowUtc, result,
                        state == BrokerReportStatus.Confirmed
                            ? "PENDING CANCELLATION CONFIRMED"
                            : "PENDING CANCELLATION ACCEPTED"));
                status = state == BrokerReportStatus.Confirmed
                    ? "PENDING CANCELLATION CONFIRMED"
                    : "PENDING CANCELLATION ACCEPTED";
            }
            catch (Exception ex)
            {
                StoreReport(robot, instanceId, reports,
                    BuildReport(command, BrokerAction.CancelPending,
                        BrokerReportStatus.RecoveryRequired, nowUtc, null, ex.Message));
                status = "PENDING CANCELLATION EXCEPTION";
            }
        }

        private void Close(
            Robot robot,
            string instanceId,
            DateTime nowUtc,
            ManagementCommand command,
            List<BrokerExecutionReport> reports,
            bool partial,
            out string status)
        {
            Position position = FindPosition(robot, command);
            BrokerAction action =
                partial ? BrokerAction.PartialClose : BrokerAction.ClosePosition;

            if (position == null)
            {
                StoreReport(robot, instanceId, reports,
                    BuildReport(command, action,
                        BrokerReportStatus.Confirmed, nowUtc, null,
                        "POSITION ABSENT"));
                status = partial ? "PARTIAL CLOSE CONFIRMED" : "FULL CLOSE CONFIRMED";
                return;
            }

            double volume = position.VolumeInUnits;
            if (partial)
            {
                if (!command.PartialCloseVolume.HasValue ||
                    !Finite(command.PartialCloseVolume.Value))
                {
                    StoreReport(robot, instanceId, reports,
                        BuildReport(command, action,
                            BrokerReportStatus.Rejected, nowUtc, null,
                            "PARTIAL CLOSE VOLUME INVALID"));
                    status = "PARTIAL CLOSE VOLUME INVALID";
                    return;
                }

                volume = robot.Symbol.NormalizeVolumeInUnits(
                    Math.Min(command.PartialCloseVolume.Value, position.VolumeInUnits),
                    RoundingMode.Down);

                if (!Finite(volume) || volume <= 0 || volume > position.VolumeInUnits)
                {
                    StoreReport(robot, instanceId, reports,
                        BuildReport(command, action,
                            BrokerReportStatus.Rejected, nowUtc, null,
                            "PARTIAL CLOSE VOLUME NORMALIZATION INVALID"));
                    status = "PARTIAL CLOSE VOLUME INVALID";
                    return;
                }

                double expected =
                    command.ExpectedRemainingVolume.HasValue
                        ? Math.Max(0, command.ExpectedRemainingVolume.Value)
                        : Math.Max(0, position.VolumeInUnits - volume);

                if (position.VolumeInUnits <= expected + VolumeTolerance(robot))
                {
                    StoreReport(robot, instanceId, reports,
                        BuildReport(command, action,
                            BrokerReportStatus.Confirmed, nowUtc, null,
                            "PARTIAL CLOSE ALREADY REFLECTED"));
                    status = "PARTIAL CLOSE CONFIRMED";
                    return;
                }
            }

            try
            {
                TradeResult result =
                    partial
                        ? robot.ClosePosition(position, volume)
                        : robot.ClosePosition(position);

                if (result == null)
                {
                    StoreReport(robot, instanceId, reports,
                        BuildReport(command, action,
                            BrokerReportStatus.RecoveryRequired, nowUtc, null,
                            "NULL CLOSE RESULT"));
                    status = partial
                        ? "PARTIAL CLOSE RECOVERY REQUIRED"
                        : "FULL CLOSE RECOVERY REQUIRED";
                    return;
                }

                if (!result.IsSuccessful)
                {
                    string reason = result.Error.HasValue
                        ? result.Error.Value.ToString()
                        : partial
                            ? "BROKER REJECTED PARTIAL CLOSE"
                            : "BROKER REJECTED FULL CLOSE";

                    StoreReport(robot, instanceId, reports,
                        BuildReport(command, action,
                            BrokerReportStatus.Rejected, nowUtc, result, reason));
                    status = reason;
                    return;
                }

                Position refreshed = FindPosition(robot, command);
                bool confirmed;

                if (refreshed == null)
                {
                    confirmed = true;
                }
                else if (!partial)
                {
                    confirmed = false;
                }
                else
                {
                    double expected =
                        command.ExpectedRemainingVolume.HasValue
                            ? Math.Max(0, command.ExpectedRemainingVolume.Value)
                            : Math.Max(0, position.VolumeInUnits - volume);

                    confirmed =
                        refreshed.VolumeInUnits <=
                        expected + VolumeTolerance(robot);
                }

                StoreReport(robot, instanceId, reports,
                    BuildReport(command, action,
                        confirmed
                            ? BrokerReportStatus.Confirmed
                            : BrokerReportStatus.Accepted,
                        nowUtc, result,
                        confirmed ? "CLOSE CONFIRMED" : "CLOSE ACCEPTED"));
                status = confirmed ? "CLOSE CONFIRMED" : "CLOSE ACCEPTED";
            }
            catch (Exception ex)
            {
                StoreReport(robot, instanceId, reports,
                    BuildReport(command, action,
                        BrokerReportStatus.RecoveryRequired, nowUtc, null, ex.Message));
                status = partial
                    ? "PARTIAL CLOSE EXCEPTION"
                    : "FULL CLOSE EXCEPTION";
            }
        }

        private void Protect(
            Robot robot,
            string instanceId,
            DateTime nowUtc,
            ManagementCommand command,
            List<BrokerExecutionReport> reports,
            out string status)
        {
            Position position = FindPosition(robot, command);
            if (position == null)
            {
                StoreReport(robot, instanceId, reports,
                    BuildReport(command, BrokerAction.None,
                        BrokerReportStatus.Confirmed, nowUtc, null,
                        "POSITION ABSENT"));
                status = "PROTECTION NO LONGER REQUIRED";
                return;
            }

            if (command.DesiredStop.HasValue &&
                !StopSatisfied(robot, position, command.DesiredStop.Value))
            {
                if (!SafeStop(robot, position, command.DesiredStop.Value))
                {
                    StoreReport(robot, instanceId, reports,
                        BuildReport(command, BrokerAction.ModifyStop,
                            BrokerReportStatus.Rejected, nowUtc, null,
                            "STOP REQUEST IS NOT PROTECTIVE"));
                    status = "STOP REQUEST IS NOT PROTECTIVE";
                    return;
                }

                if (!TryModifyStop(
                        position,
                        command.DesiredStop.Value,
                        out TradeResult result,
                        out string reason))
                {
                    StoreReport(robot, instanceId, reports,
                        BuildReport(command, BrokerAction.ModifyStop,
                            result == null
                                ? BrokerReportStatus.RecoveryRequired
                                : BrokerReportStatus.Rejected,
                            nowUtc, result, reason));
                    status = reason;
                    return;
                }

                Position refreshed = FindPosition(robot, command);
                if (refreshed == null)
                {
                    StoreReport(robot, instanceId, reports,
                        BuildReport(command, BrokerAction.ModifyStop,
                            BrokerReportStatus.RecoveryRequired, nowUtc, result,
                            "POSITION DISAPPEARED DURING STOP RECONCILIATION"));
                    status = "STOP RECONCILIATION RECOVERY REQUIRED";
                    return;
                }

                if (!StopSatisfied(robot, refreshed, command.DesiredStop.Value))
                {
                    StoreReport(robot, instanceId, reports,
                        BuildReport(command, BrokerAction.ModifyStop,
                            BrokerReportStatus.Accepted, nowUtc, result,
                            "STOP ACCEPTED; RECONCILIATION PENDING"));
                    status = "STOP ACCEPTED";
                    return;
                }

                position = refreshed;
            }

            if (command.DesiredTarget.HasValue ||
                command.DesiredTargetPips.HasValue)
            {
                if (!TargetSatisfied(
                        robot,
                        position,
                        command.DesiredTarget,
                        command.DesiredTargetPips))
                {
                    if (!SafeTarget(
                            robot,
                            position,
                            command.DesiredTarget,
                            command.DesiredTargetPips))
                    {
                        StoreReport(robot, instanceId, reports,
                            BuildReport(command, BrokerAction.ModifyTarget,
                                BrokerReportStatus.Rejected, nowUtc, null,
                                "TARGET REQUEST IS NOT FORWARD"));
                        status = "TARGET REQUEST IS NOT FORWARD";
                        return;
                    }

                    TradeResult result;
                    string reason;

                    bool ok =
                        command.DesiredTargetPips.HasValue
                            ? TryModifyTargetPips(
                                position,
                                command.DesiredTargetPips.Value,
                                out result,
                                out reason)
                            : TryModifyTargetPrice(
                                position,
                                command.DesiredTarget.Value,
                                out result,
                                out reason);

                    if (!ok)
                    {
                        StoreReport(robot, instanceId, reports,
                            BuildReport(command, BrokerAction.ModifyTarget,
                                result == null
                                    ? BrokerReportStatus.RecoveryRequired
                                    : BrokerReportStatus.Rejected,
                                nowUtc, result, reason));
                        status = reason;
                        return;
                    }

                    position = FindPosition(robot, command);
                    if (position == null)
                    {
                        StoreReport(robot, instanceId, reports,
                            BuildReport(command, BrokerAction.ModifyTarget,
                                BrokerReportStatus.RecoveryRequired, nowUtc, result,
                                "POSITION DISAPPEARED DURING TARGET RECONCILIATION"));
                        status = "TARGET RECONCILIATION RECOVERY REQUIRED";
                        return;
                    }

                    if (!TargetSatisfied(
                            robot,
                            position,
                            command.DesiredTarget,
                            command.DesiredTargetPips))
                    {
                        StoreReport(robot, instanceId, reports,
                            BuildReport(command, BrokerAction.ModifyTarget,
                                BrokerReportStatus.Accepted, nowUtc, result,
                                "TARGET ACCEPTED; RECONCILIATION PENDING"));
                        status = "TARGET ACCEPTED";
                        return;
                    }
                }
            }

            StoreReport(robot, instanceId, reports,
                BuildReport(command,
                    command.DesiredStop.HasValue
                        ? BrokerAction.ModifyStop
                        : BrokerAction.ModifyTarget,
                    BrokerReportStatus.Confirmed,
                    nowUtc, null, "PROTECTION CONFIRMED"));
            status = "PROTECTION CONFIRMED";
        }

        private void AdvanceTarget(
            Robot robot,
            string instanceId,
            DateTime nowUtc,
            ManagementCommand command,
            List<BrokerExecutionReport> reports,
            out string status)
        {
            Position position = FindPosition(robot, command);
            if (position == null)
            {
                StoreReport(robot, instanceId, reports,
                    BuildReport(command, BrokerAction.ModifyTarget,
                        BrokerReportStatus.Confirmed, nowUtc, null,
                        "POSITION ABSENT"));
                status = "TARGET ADVANCE CONFIRMED";
                return;
            }

            bool ladder =
                command.LadderFirstVolume.HasValue &&
                command.LadderFirstTargetPips.HasValue &&
                command.LadderFinalTargetPips.HasValue;

            if (ladder && LadderSatisfied(robot, position, command))
            {
                StoreReport(robot, instanceId, reports,
                    BuildReport(command, BrokerAction.ModifyTargetLadder,
                        BrokerReportStatus.Confirmed, nowUtc, null,
                        "TARGET LADDER ALREADY REFLECTED"));
                status = "TARGET LADDER CONFIRMED";
                return;
            }

            if (!ladder &&
                (command.DesiredTarget.HasValue ||
                 command.DesiredTargetPips.HasValue) &&
                TargetSatisfied(
                    robot,
                    position,
                    command.DesiredTarget,
                    command.DesiredTargetPips))
            {
                StoreReport(robot, instanceId, reports,
                    BuildReport(command, BrokerAction.ModifyTarget,
                        BrokerReportStatus.Confirmed, nowUtc, null,
                        "TARGET ALREADY FORWARD"));
                status = "TARGET ADVANCE CONFIRMED";
                return;
            }

            if (ladder)
            {
                if (!SafeLadder(robot, position, command))
                {
                    StoreReport(robot, instanceId, reports,
                        BuildReport(command, BrokerAction.ModifyTargetLadder,
                            BrokerReportStatus.Rejected, nowUtc, null,
                            "TARGET LADDER GEOMETRY INVALID"));
                    status = "TARGET LADDER GEOMETRY INVALID";
                    return;
                }

                if (!TryModifyLadder(
                        position,
                        command,
                        robot,
                        out TradeResult result,
                        out string reason))
                {
                    StoreReport(robot, instanceId, reports,
                        BuildReport(command, BrokerAction.ModifyTargetLadder,
                            result == null
                                ? BrokerReportStatus.RecoveryRequired
                                : BrokerReportStatus.Rejected,
                            nowUtc, result, reason));
                    status = reason;
                    return;
                }

                Position refreshed = FindPosition(robot, command);
                bool confirmed =
                    refreshed != null &&
                    LadderSatisfied(robot, refreshed, command);

                StoreReport(robot, instanceId, reports,
                    BuildReport(command, BrokerAction.ModifyTargetLadder,
                        confirmed
                            ? BrokerReportStatus.Confirmed
                            : BrokerReportStatus.Accepted,
                        nowUtc, result,
                        confirmed
                            ? "TARGET LADDER CONFIRMED"
                            : "TARGET LADDER ACCEPTED"));
                status = confirmed
                    ? "TARGET LADDER CONFIRMED"
                    : "TARGET LADDER ACCEPTED";
                return;
            }

            if (!command.DesiredTarget.HasValue &&
                !command.DesiredTargetPips.HasValue)
            {
                StoreReport(robot, instanceId, reports,
                    BuildReport(command, BrokerAction.ModifyTarget,
                        BrokerReportStatus.Rejected, nowUtc, null,
                        "TARGET ADVANCE VALUE MISSING"));
                status = "TARGET ADVANCE VALUE MISSING";
                return;
            }

            if (!SafeTarget(
                    robot,
                    position,
                    command.DesiredTarget,
                    command.DesiredTargetPips))
            {
                StoreReport(robot, instanceId, reports,
                    BuildReport(command, BrokerAction.ModifyTarget,
                        BrokerReportStatus.Rejected, nowUtc, null,
                        "TARGET REQUEST IS NOT FORWARD"));
                status = "TARGET REQUEST IS NOT FORWARD";
                return;
            }

            TradeResult targetResult;
            string targetReason;
            bool mutationOk =
                command.DesiredTargetPips.HasValue
                    ? TryModifyTargetPips(
                        position,
                        command.DesiredTargetPips.Value,
                        out targetResult,
                        out targetReason)
                    : TryModifyTargetPrice(
                        position,
                        command.DesiredTarget.Value,
                        out targetResult,
                        out targetReason);

            if (!mutationOk)
            {
                StoreReport(robot, instanceId, reports,
                    BuildReport(command, BrokerAction.ModifyTarget,
                        targetResult == null
                            ? BrokerReportStatus.RecoveryRequired
                            : BrokerReportStatus.Rejected,
                        nowUtc, targetResult, targetReason));
                status = targetReason;
                return;
            }

            Position refreshedTarget = FindPosition(robot, command);
            bool confirmedAfter =
                refreshedTarget != null &&
                TargetSatisfied(
                    robot,
                    refreshedTarget,
                    command.DesiredTarget,
                    command.DesiredTargetPips);

            StoreReport(robot, instanceId, reports,
                BuildReport(command, BrokerAction.ModifyTarget,
                    confirmedAfter
                        ? BrokerReportStatus.Confirmed
                        : BrokerReportStatus.Accepted,
                    nowUtc, targetResult,
                    confirmedAfter
                        ? "TARGET ADVANCE CONFIRMED"
                        : "TARGET ADVANCE ACCEPTED"));
            status = confirmedAfter
                ? "TARGET ADVANCE CONFIRMED"
                : "TARGET ADVANCE ACCEPTED";
        }

        private static bool Validate(
            ManagementCommand command,
            Robot robot,
            DateTime nowUtc,
            int maximumCommandAgeSeconds,
            out string reason,
            out bool expired)
        {
            reason = "OK";
            expired = false;

            if (command.Identity == null)
            {
                reason = "MANAGEMENT IDENTITY MISSING";
                return false;
            }

            if (!string.Equals(
                    command.Identity.Symbol,
                    robot.SymbolName,
                    StringComparison.Ordinal))
            {
                reason = "MANAGEMENT SYMBOL MISMATCH";
                return false;
            }

            if (command.Identity.Direction != TradeDirection.Buy &&
                command.Identity.Direction != TradeDirection.Sell)
            {
                reason = "MANAGEMENT DIRECTION INVALID";
                return false;
            }

            if (command.RequestedUtc > nowUtc.AddSeconds(30))
            {
                reason = "MANAGEMENT REQUEST TIME IS IN FUTURE";
                return false;
            }

            double ageSeconds =
                (nowUtc - command.RequestedUtc).TotalSeconds;

            if (ageSeconds >
                Math.Max(
                    1,
                    maximumCommandAgeSeconds))
            {
                expired = true;
                reason = "MANAGEMENT COMMAND EXPIRED";
                return false;
            }

            if (string.IsNullOrWhiteSpace(command.CommandIdempotencyKey))
            {
                reason = "MANAGEMENT IDEMPOTENCY KEY MISSING";
                return false;
            }

            if (string.IsNullOrWhiteSpace(command.ExecutionLabel))
            {
                reason = "MANAGEMENT EXECUTION LABEL MISSING";
                return false;
            }

            return true;
        }

        private static Position FindPosition(Robot robot, ManagementCommand command)
        {
            Position match = null;
            int count = 0;

            foreach (Position position in robot.Positions)
            {
                if (position == null ||
                    !string.Equals(position.SymbolName, robot.SymbolName, StringComparison.Ordinal) ||
                    !string.Equals(position.Label, command.ExecutionLabel, StringComparison.Ordinal))
                    continue;

                if (command.PositionId.HasValue &&
                    position.Id != command.PositionId.Value)
                    continue;

                match = position;
                count++;
            }

            return count == 1 ? match : null;
        }

        private static PendingOrder FindPending(Robot robot, ManagementCommand command)
        {
            string label = command.ExecutionLabel + "-PENDING";
            PendingOrder match = null;
            int count = 0;

            foreach (PendingOrder order in robot.PendingOrders)
            {
                if (order == null ||
                    !string.Equals(order.SymbolName, robot.SymbolName, StringComparison.Ordinal) ||
                    !string.Equals(order.Label, label, StringComparison.Ordinal))
                    continue;

                if (command.PendingOrderId.HasValue &&
                    order.Id != command.PendingOrderId.Value)
                    continue;

                match = order;
                count++;
            }

            return count == 1 ? match : null;
        }

        private static bool StopSatisfied(Robot robot, Position position, double desired)
        {
            if (position == null || !Finite(desired))
                return false;
            if (!position.StopLoss.HasValue)
                return false;

            double tolerance =
                Math.Max(robot.Symbol.TickSize * 2, robot.Symbol.PipSize * 0.25);

            return position.TradeType == TradeType.Buy
                ? position.StopLoss.Value >= desired - tolerance
                : position.StopLoss.Value <= desired + tolerance;
        }

        private static bool SafeStop(Robot robot, Position position, double desired)
        {
            double market =
                position.TradeType == TradeType.Buy
                    ? robot.Symbol.Bid
                    : robot.Symbol.Ask;

            double offset =
                Math.Max(
                    robot.Symbol.TickSize,
                    robot.Symbol.PipSize * 0.10);

            if (!Finite(market) ||
                !Finite(position.EntryPrice) ||
                !Finite(desired))
                return false;

            return position.TradeType == TradeType.Buy
                ? desired < position.EntryPrice &&
                  desired < market - offset
                : desired > position.EntryPrice &&
                  desired > market + offset;
        }

        private static double DesiredTarget(
            Robot robot,
            Position position,
            double? absoluteTarget,
            double? targetPips)
        {
            if (absoluteTarget.HasValue)
                return absoluteTarget.Value;

            if (!targetPips.HasValue ||
                !Finite(targetPips.Value))
                return 0;

            return position.TradeType == TradeType.Buy
                ? position.EntryPrice + targetPips.Value * robot.Symbol.PipSize
                : position.EntryPrice - targetPips.Value * robot.Symbol.PipSize;
        }

        private static bool TargetSatisfied(
            Robot robot,
            Position position,
            double? absoluteTarget,
            double? targetPips)
        {
            double desired =
                DesiredTarget(robot, position, absoluteTarget, targetPips);

            if (!Finite(desired) || !position.TakeProfit.HasValue)
                return false;

            double tolerance =
                Math.Max(robot.Symbol.TickSize * 2, robot.Symbol.PipSize * 0.25);

            return position.TradeType == TradeType.Buy
                ? position.TakeProfit.Value >= desired - tolerance
                : position.TakeProfit.Value <= desired + tolerance;
        }

        private static bool SafeTarget(
            Robot robot,
            Position position,
            double? absoluteTarget,
            double? targetPips)
        {
            double desired =
                DesiredTarget(robot, position, absoluteTarget, targetPips);

            double market =
                position.TradeType == TradeType.Buy
                    ? robot.Symbol.Bid
                    : robot.Symbol.Ask;

            if (!Finite(desired) ||
                !Finite(market) ||
                !Finite(position.EntryPrice))
                return false;

            return position.TradeType == TradeType.Buy
                ? desired > position.EntryPrice && desired > market
                : desired < position.EntryPrice && desired < market;
        }

        private static bool SafeLadder(
            Robot robot,
            Position position,
            ManagementCommand command)
        {
            if (position == null ||
                !command.LadderFirstVolume.HasValue ||
                !command.LadderFirstTargetPips.HasValue ||
                !command.LadderFinalTargetPips.HasValue)
                return false;

            double first =
                command.LadderFirstTargetPips.Value;
            double final =
                command.LadderFinalTargetPips.Value;

            if (!Finite(first) ||
                !Finite(final) ||
                first <= 0 ||
                final <= first)
                return false;

            if (command.LadderSecondVolume.HasValue !=
                command.LadderSecondTargetPips.HasValue)
                return false;

            if (command.LadderSecondTargetPips.HasValue &&
                (!Finite(command.LadderSecondTargetPips.Value) ||
                 command.LadderSecondTargetPips.Value <= first ||
                 command.LadderSecondTargetPips.Value >= final))
                return false;

            double finalPrice =
                position.TradeType == TradeType.Buy
                    ? position.EntryPrice + final * robot.Symbol.PipSize
                    : position.EntryPrice - final * robot.Symbol.PipSize;

            return SafeTarget(robot, position, finalPrice, null);
        }

        private static bool LadderSatisfied(
            Robot robot,
            Position position,
            ManagementCommand command)
        {
            if (position == null ||
                !position.TakeProfit.HasValue ||
                !command.LadderFinalTargetPips.HasValue)
                return false;

            double finalPrice =
                position.TradeType == TradeType.Buy
                    ? position.EntryPrice +
                      command.LadderFinalTargetPips.Value *
                      robot.Symbol.PipSize
                    : position.EntryPrice -
                      command.LadderFinalTargetPips.Value *
                      robot.Symbol.PipSize;

            double tolerance =
                Math.Max(robot.Symbol.TickSize * 2, robot.Symbol.PipSize * 0.25);

            return position.TradeType == TradeType.Buy
                ? position.TakeProfit.Value >= finalPrice - tolerance
                : position.TakeProfit.Value <= finalPrice + tolerance;
        }

        private static bool TryModifyStop(
            Position position,
            double desired,
            out TradeResult result,
            out string reason)
        {
            result = null;
            reason = "OK";

            try
            {
                result = position.ModifyStopLossPrice(desired);
                if (result == null)
                {
                    reason = "NULL STOP RESULT";
                    return false;
                }

                if (!result.IsSuccessful)
                {
                    reason = result.Error.HasValue
                        ? result.Error.Value.ToString()
                        : "BROKER REJECTED STOP MODIFICATION";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                reason = ex.Message;
                return false;
            }
        }

        private static bool TryModifyTargetPrice(
            Position position,
            double desired,
            out TradeResult result,
            out string reason)
        {
            result = null;
            reason = "OK";

            try
            {
                result = position.ModifyTakeProfitPrice(desired);
                if (result == null)
                {
                    reason = "NULL TARGET RESULT";
                    return false;
                }

                if (!result.IsSuccessful)
                {
                    reason = result.Error.HasValue
                        ? result.Error.Value.ToString()
                        : "BROKER REJECTED TARGET MODIFICATION";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                reason = ex.Message;
                return false;
            }
        }

        private static bool TryModifyTargetPips(
            Position position,
            double desired,
            out TradeResult result,
            out string reason)
        {
            result = null;
            reason = "OK";

            try
            {
                result = position.ModifyTakeProfitPips(desired);
                if (result == null)
                {
                    reason = "NULL TARGET PIPS RESULT";
                    return false;
                }

                if (!result.IsSuccessful)
                {
                    reason = result.Error.HasValue
                        ? result.Error.Value.ToString()
                        : "BROKER REJECTED TARGET PIPS";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                reason = ex.Message;
                return false;
            }
        }

        private static bool TryModifyLadder(
            Position position,
            ManagementCommand command,
            Robot robot,
            out TradeResult result,
            out string reason)
        {
            result = null;
            reason = "OK";

            double firstVolume =
                robot.Symbol.NormalizeVolumeInUnits(
                    command.LadderFirstVolume.Value,
                    RoundingMode.Down);

            if (!Finite(firstVolume) ||
                firstVolume <= 0 ||
                firstVolume >= position.VolumeInUnits)
            {
                reason = "TARGET LADDER FIRST VOLUME INVALID";
                return false;
            }

            try
            {
                RelativeTakeProfitProtections protections;

                if (command.LadderSecondVolume.HasValue)
                {
                    double secondVolume =
                        robot.Symbol.NormalizeVolumeInUnits(
                            command.LadderSecondVolume.Value,
                            RoundingMode.Down);

                    if (!Finite(secondVolume) ||
                        secondVolume <= 0 ||
                        firstVolume + secondVolume >= position.VolumeInUnits)
                    {
                        reason = "TARGET LADDER SECOND VOLUME INVALID";
                        return false;
                    }

                    protections =
                        new RelativeTakeProfitProtections(
                            new RelativeTakeProfitProtection(
                                firstVolume,
                                command.LadderFirstTargetPips.Value),
                            new RelativeTakeProfitProtection(
                                secondVolume,
                                command.LadderSecondTargetPips.Value),
                            new RelativeTakeProfitLastProtection(
                                command.LadderFinalTargetPips.Value));
                }
                else
                {
                    protections =
                        new RelativeTakeProfitProtections(
                            new RelativeTakeProfitProtection(
                                firstVolume,
                                command.LadderFirstTargetPips.Value),
                            new RelativeTakeProfitLastProtection(
                                command.LadderFinalTargetPips.Value));
                }

                result = position.ModifyTakeProfit(protections);
                if (result == null)
                {
                    reason = "NULL TARGET LADDER RESULT";
                    return false;
                }

                if (!result.IsSuccessful)
                {
                    reason = result.Error.HasValue
                        ? result.Error.Value.ToString()
                        : "BROKER REJECTED TARGET LADDER";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                reason = ex.Message;
                return false;
            }
        }

        private static bool TryConfirmFromBrokerState(
            Robot robot,
            ManagementCommand command,
            out BrokerExecutionReport report)
        {
            report = null;

            if (command.Command == ManagementCommandType.CancelPending)
            {
                if (FindPending(robot, command) == null)
                {
                    report = BuildReport(
                        command, BrokerAction.CancelPending,
                        BrokerReportStatus.Confirmed,
                        robot.Server.TimeInUtc, null,
                        "PENDING ABSENCE CONFIRMED");
                    return true;
                }
                return false;
            }

            Position position = FindPosition(robot, command);

            if (command.Command == ManagementCommandType.FullClose ||
                command.Command == ManagementCommandType.PartialClose)
            {
                if (position == null)
                {
                    report = BuildReport(
                        command,
                        command.Command == ManagementCommandType.FullClose
                            ? BrokerAction.ClosePosition
                            : BrokerAction.PartialClose,
                        BrokerReportStatus.Confirmed,
                        robot.Server.TimeInUtc, null,
                        "POSITION ABSENCE CONFIRMED");
                    return true;
                }

                if (command.Command == ManagementCommandType.PartialClose &&
                    command.ExpectedRemainingVolume.HasValue &&
                    position.VolumeInUnits <=
                    command.ExpectedRemainingVolume.Value + VolumeTolerance(robot))
                {
                    report = BuildReport(
                        command, BrokerAction.PartialClose,
                        BrokerReportStatus.Confirmed,
                        robot.Server.TimeInUtc, null,
                        "PARTIAL VOLUME CONFIRMED");
                    return true;
                }

                return false;
            }

            if (command.Command == ManagementCommandType.ModifyProtection ||
                command.Command == ManagementCommandType.BreakEven)
            {
                if (position == null)
                {
                    report = BuildReport(
                        command, BrokerAction.None,
                        BrokerReportStatus.Confirmed,
                        robot.Server.TimeInUtc, null,
                        "POSITION ABSENCE CONFIRMED");
                    return true;
                }

                bool stopDone =
                    !command.DesiredStop.HasValue ||
                    StopSatisfied(robot, position, command.DesiredStop.Value);

                bool targetDone =
                    (!command.DesiredTarget.HasValue &&
                     !command.DesiredTargetPips.HasValue) ||
                    TargetSatisfied(
                        robot,
                        position,
                        command.DesiredTarget,
                        command.DesiredTargetPips);

                if (stopDone && targetDone)
                {
                    report = BuildReport(
                        command,
                        command.DesiredStop.HasValue
                            ? BrokerAction.ModifyStop
                            : BrokerAction.ModifyTarget,
                        BrokerReportStatus.Confirmed,
                        robot.Server.TimeInUtc, null,
                        "PROTECTION STATE CONFIRMED");
                    return true;
                }

                return false;
            }

            if (command.Command == ManagementCommandType.AdvanceTarget)
            {
                if (position == null)
                {
                    report = BuildReport(
                        command, BrokerAction.ModifyTarget,
                        BrokerReportStatus.Confirmed,
                        robot.Server.TimeInUtc, null,
                        "POSITION ABSENCE CONFIRMED");
                    return true;
                }

                bool ladder =
                    command.LadderFirstVolume.HasValue &&
                    command.LadderFirstTargetPips.HasValue &&
                    command.LadderFinalTargetPips.HasValue;

                if ((ladder && LadderSatisfied(robot, position, command)) ||
                    (!ladder &&
                     (command.DesiredTarget.HasValue ||
                      command.DesiredTargetPips.HasValue) &&
                     TargetSatisfied(
                         robot,
                         position,
                         command.DesiredTarget,
                         command.DesiredTargetPips)))
                {
                    report = BuildReport(
                        command,
                        ladder
                            ? BrokerAction.ModifyTargetLadder
                            : BrokerAction.ModifyTarget,
                        BrokerReportStatus.Confirmed,
                        robot.Server.TimeInUtc, null,
                        "TARGET STATE CONFIRMED");
                    return true;
                }
            }

            return false;
        }

        private static void StoreReport(
            Robot robot,
            string instanceId,
            List<BrokerExecutionReport> reports,
            BrokerExecutionReport report)
        {
            if (report == null)
                return;

            for (int i = reports.Count - 1; i >= 0; i--)
            {
                if (string.Equals(
                        reports[i].CommandIdempotencyKey,
                        report.CommandIdempotencyKey,
                        StringComparison.Ordinal))
                {
                    reports[i] = report;
                    PublishReports(robot, instanceId, reports);
                    return;
                }
            }

            reports.Add(report);
            while (reports.Count > MaxReports)
                reports.RemoveAt(0);

            PublishReports(robot, instanceId, reports);
        }

        private static void PublishReports(
            Robot robot,
            string instanceId,
            List<BrokerExecutionReport> reports)
        {
            robot.LocalStorage.SetString(
                ManagementBusKey.ReportKeyForInstance(instanceId),
                BrokerExecutionReportCodec.Serialize(reports.ToArray()),
                LocalStorageScope.Device);
            robot.LocalStorage.Flush(LocalStorageScope.Device);
        }

        private static bool TryLoadCommands(
            Robot robot,
            string instanceId,
            out ManagementCommand[] commands)
        {
            try
            {
                string payload =
                    robot.LocalStorage.GetString(
                        ManagementBusKey.CommandKeyForInstance(instanceId),
                        LocalStorageScope.Device);

                return ManagementCommandCodec.TryDeserialize(
                    payload,
                    out commands);
            }
            catch
            {
                commands = null;
                return false;
            }
        }

        private static bool TryLoadReports(
            Robot robot,
            string instanceId,
            out List<BrokerExecutionReport> reports)
        {
            reports = new List<BrokerExecutionReport>();

            try
            {
                string payload =
                    robot.LocalStorage.GetString(
                        ManagementBusKey.ReportKeyForInstance(instanceId),
                        LocalStorageScope.Device);

                if (!BrokerExecutionReportCodec.TryDeserialize(
                        payload,
                        out BrokerExecutionReport[] parsed) ||
                    parsed == null)
                    return true;

                for (int i = 0; i < parsed.Length; i++)
                {
                    if (parsed[i] != null &&
                        !string.IsNullOrWhiteSpace(parsed[i].CommandIdempotencyKey))
                        reports.Add(parsed[i]);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static BrokerExecutionReport FindLatestReport(
            List<BrokerExecutionReport> reports,
            string key)
        {
            if (reports == null)
                return null;

            for (int i = reports.Count - 1; i >= 0; i--)
            {
                if (string.Equals(
                        reports[i].CommandIdempotencyKey,
                        key,
                        StringComparison.Ordinal))
                    return reports[i];
            }

            return null;
        }

        private static BrokerExecutionReport BuildReport(
            ManagementCommand command,
            BrokerAction action,
            BrokerReportStatus status,
            DateTime nowUtc,
            TradeResult result,
            string reason)
        {
            Position position =
                result != null ? result.Position : null;

            PendingOrder pending =
                result != null ? result.PendingOrder : null;

            return new BrokerExecutionReport(
                command.Identity,
                action,
                status,
                nowUtc,
                nowUtc,
                status == BrokerReportStatus.Confirmed
                    ? nowUtc
                    : (DateTime?)null,
                position != null
                    ? position.Id
                    : command.PositionId,
                pending != null
                    ? pending.Id
                    : command.PendingOrderId,
                position != null
                    ? position.EntryPrice
                    : (double?)null,
                position != null && position.StopLoss.HasValue
                    ? position.StopLoss.Value
                    : command.DesiredStop,
                position != null && position.TakeProfit.HasValue
                    ? position.TakeProfit.Value
                    : command.DesiredTarget,
                position != null
                    ? position.Id.ToString()
                    : pending != null
                        ? pending.Id.ToString()
                        : "",
                result != null && result.Error.HasValue
                    ? result.Error.Value.ToString()
                    : "",
                reason ?? "",
                command.CommandRevision)
            {
                CommandIdempotencyKey =
                    command.CommandIdempotencyKey
            };
        }

        private static double VolumeTolerance(Robot robot) =>
            Math.Max(
                robot.Symbol.VolumeInUnitsStep,
                robot.Symbol.VolumeInUnitsMin * 0.5);

        private static bool Finite(double value) =>
            !double.IsNaN(value) && !double.IsInfinity(value) && value > 0;
    }
}
