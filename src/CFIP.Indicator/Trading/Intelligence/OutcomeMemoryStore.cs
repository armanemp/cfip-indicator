using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Reflection;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator
    {
        private const string OutcomeMemorySchema = "CFIP-OUTCOME,2";
        private const string PriorOutcomeMemorySchema = "CFIP-OUTCOME";
        private const int OutcomeMemoryMaxAgeDays = 90;

        private string _memoryConfigurationFingerprintCache;
        private string _priorMemoryConfigurationFingerprintCache;
        private string OutcomeMemoryKey()
        {
            string symbol =
                string.IsNullOrWhiteSpace(SymbolName)
                    ? "UNKNOWN"
                    : SymbolName;

            string timeframe =
                ExecutionTimeframePolicy.PrimaryExecution;

            return OutcomeMemoryIdentityRule.BuildMemoryKey(
                symbol,
                timeframe,
                MemoryAccountScopeToken(),
                MemoryConfigurationFingerprint());
        }

        private string PriorOutcomeMemoryKey()
        {
            string symbol =
                string.IsNullOrWhiteSpace(SymbolName)
                    ? "UNKNOWN"
                    : SymbolName;

            string timeframe =
                ExecutionTimeframePolicy.PrimaryExecution;

            return OutcomeMemoryIdentityRule.BuildLegacyMemoryKey(
                symbol,
                timeframe,
                PriorMemoryConfigurationFingerprint());
        }

        private string MemoryAccountScopeToken()
        {
            try
            {
                return OutcomeMemoryIdentityRule.BuildAccountScopeToken(
                    Account.BrokerName,
                    Account.Number,
                    Account.AccountType.ToString(),
                    Account.IsLive);
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP outcome memory account identity unavailable: {0}",
                    ex.Message);

                return OutcomeMemoryIdentityRule.BuildAccountScopeToken(
                    "UNKNOWN",
                    0,
                    "UNKNOWN",
                    false);
            }
        }

        private List<OutcomeMemoryParameter> CollectOutcomeMemoryParameters()
        {
            PropertyInfo[] properties =
                GetType().GetProperties(
                    BindingFlags.Instance |
                    BindingFlags.Public);

            Array.Sort(
                properties,
                delegate(PropertyInfo left, PropertyInfo right)
                {
                    return string.CompareOrdinal(
                        left.Name,
                        right.Name);
                });

            List<OutcomeMemoryParameter> parameters =
                new List<OutcomeMemoryParameter>();

            foreach (PropertyInfo property in properties)
            {
                object[] attributes =
                    property.GetCustomAttributes(
                        typeof(ParameterAttribute),
                        true);

                if (attributes == null ||
                    attributes.Length == 0)
                    continue;

                ParameterAttribute parameterAttribute =
                    attributes[0] as ParameterAttribute;

                string group =
                    parameterAttribute == null
                        ? ""
                        : parameterAttribute.Group;

                object value;

                try
                {
                    value =
                        property.GetValue(
                            this,
                            null);
                }
                catch
                {
                    value = null;
                }

                IFormattable formattable =
                    value as IFormattable;

                string formatted =
                    formattable != null
                        ? formattable.ToString(
                            null,
                            CultureInfo.InvariantCulture)
                        : value == null
                            ? ""
                            : value.ToString();

                parameters.Add(
                    new OutcomeMemoryParameter(
                        property.Name,
                        group,
                        formatted));
            }

            return parameters;
        }

        private string MemoryConfigurationFingerprint()
        {
            if (!string.IsNullOrWhiteSpace(
                    _memoryConfigurationFingerprintCache))
                return _memoryConfigurationFingerprintCache;

            _memoryConfigurationFingerprintCache =
                OutcomeMemoryIdentityRule.BuildFingerprint(
                    CollectOutcomeMemoryParameters(),
                    true);

            return _memoryConfigurationFingerprintCache;
        }

        private string PriorMemoryConfigurationFingerprint()
        {
            if (!string.IsNullOrWhiteSpace(
                    _priorMemoryConfigurationFingerprintCache))
                return _priorMemoryConfigurationFingerprintCache;

            _priorMemoryConfigurationFingerprintCache =
                OutcomeMemoryIdentityRule.BuildFingerprint(
                    CollectOutcomeMemoryParameters(),
                    false);

            return _priorMemoryConfigurationFingerprintCache;
        }

        private string SerializeOutcomeHistory()
        {
            if (_outcomeHistory == null ||
                _outcomeHistory.Count == 0)
                return OutcomeMemorySchema + "\n";

            StringBuilder text =
                new StringBuilder();

            text.Append(OutcomeMemorySchema);
            text.Append('\n');

            int start =
                Math.Max(
                    0,
                    _outcomeHistory.Count - 128);

            for (int i = start;
                 i < _outcomeHistory.Count;
                 i++)
            {
                OutcomeObservation o =
                    _outcomeHistory[i];

                if (o == null ||
                    o.PositionId <= 0)
                    continue;

                text.Append(o.PositionId.ToString(CultureInfo.InvariantCulture));
                text.Append('|');
                text.Append(o.Direction);
                text.Append('|');
                text.Append((int)o.Lane);
                text.Append('|');
                text.Append((int)o.EntryMode);
                text.Append('|');
                text.Append(Encode(o.Regime));
                text.Append('|');
                text.Append(o.Confidence);
                text.Append('|');
                text.Append(o.ConfidenceBucket);
                text.Append('|');
                text.Append(o.CreatedM5);
                text.Append('|');
                text.Append(o.ClosedM5);
                text.Append('|');
                text.Append(o.LifecycleBars);
                text.Append('|');
                text.Append(o.Pips.ToString("R", CultureInfo.InvariantCulture));
                text.Append('|');
                text.Append(o.NetProfit.ToString("R", CultureInfo.InvariantCulture));
                text.Append('|');
                text.Append(o.RealizedR.ToString("R", CultureInfo.InvariantCulture));
                text.Append('|');
                text.Append(o.Profitable ? '1' : '0');
                text.Append('|');
                text.Append(o.CalibrationEligible ? '1' : '0');
                text.Append('|');
                text.Append(o.ProtectionRecoveryAtClose ? '1' : '0');
                text.Append('|');
                text.Append(o.ServerSideTakeProfitLadderActive ? '1' : '0');
                text.Append('|');
                text.Append(o.ObservedUtcTicks);
                text.Append('\n');
            }

            return text.ToString();
        }

        private bool RestoreOutcomeHistory()
        {
            try
            {
                bool priorMemory =
                    false;

                string stored =
                    LocalStorage.GetString(
                        OutcomeMemoryKey(),
                        LocalStorageScope.Type);

                if (string.IsNullOrWhiteSpace(stored))
                {
                    stored =
                        LocalStorage.GetString(
                            PriorOutcomeMemoryKey(),
                            LocalStorageScope.Type);

                    priorMemory =
                        !string.IsNullOrWhiteSpace(stored);
                }

                if (string.IsNullOrWhiteSpace(stored))
                {
                    string portablePayload;

                    if (TryRestorePortableMemorySnapshot(
                            out portablePayload))
                    {
                        stored = portablePayload;
                    }
                }

                if (string.IsNullOrWhiteSpace(stored))
                    return false;

                string[] lines =
                    stored.Split(
                        new[] { '\n' },
                        StringSplitOptions.RemoveEmptyEntries);

                if (lines.Length == 0)
                    return false;

                string schema =
                    lines[0].Trim();

                bool currentSchema =
                    string.Equals(
                        schema,
                        OutcomeMemorySchema,
                        StringComparison.Ordinal);

                bool priorSchema =
                    string.Equals(
                        schema,
                        PriorOutcomeMemorySchema,
                        StringComparison.Ordinal);

                if (!currentSchema &&
                    !priorSchema)
                    return false;

                // A legacy key/snapshot has no account identity. Only adopt a
                // legacy observation when the current account's broker history
                // proves that the PositionId belongs to this account.
                if (priorSchema)
                    priorMemory = true;

                DateTime cutoff =
                    Server.TimeInUtc.AddDays(
                        -OutcomeMemoryMaxAgeDays);

                _outcomeHistory.Clear();

                for (int i = 1;
                     i < lines.Length;
                     i++)
                {
                    OutcomeObservation item;

                    if (!TryParseOutcome(
                            lines[i],
                            out item))
                        continue;

                    if (priorMemory &&
                        !IsLegacyOutcomeOwnedByCurrentAccount(
                            item.PositionId))
                        continue;

                    if (item.ObservedUtcTicks > 0)
                    {
                        DateTime observed =
                            new DateTime(
                                item.ObservedUtcTicks,
                                DateTimeKind.Utc);

                        if (observed < cutoff)
                            continue;
                    }

                    if (HasRecordedOutcome(
                            item.PositionId))
                        continue;

                    _outcomeHistory.Add(item);
                }

                TrimOutcomeHistory();
                RebuildOutcomeAggregates();

                if (priorMemory &&
                    _outcomeHistory.Count > 0)
                {
                    PersistOutcomeHistory();

                    Print(
                        "CFIP outcome memory migrated safely: {0} observations",
                        _outcomeHistory.Count);
                }

                return _outcomeHistory.Count > 0;
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP outcome memory restore failed: {0}",
                    ex.Message);
                return false;
            }
        }

        private bool IsLegacyOutcomeOwnedByCurrentAccount(
            long positionId)
        {
            if (positionId <= 0 ||
                positionId > int.MaxValue)
                return false;

            try
            {
                HistoricalTrade[] historicalTrades =
                    History.FindByPositionId(
                        (int)positionId);

                return historicalTrades != null &&
                    historicalTrades.Length > 0;
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP legacy outcome account validation failed for #{0}: {1}",
                    positionId,
                    ex.Message);

                return false;
            }
        }

        private bool TryParseOutcome(
            string line,
            out OutcomeObservation item)
        {
            item = null;

            if (string.IsNullOrWhiteSpace(line))
                return false;

            string[] parts =
                line.Split('|');

            if (parts.Length < 18)
                return false;

            long positionId;
            int direction;
            int lane;
            int entryMode;
            int confidence;
            int confidenceBucket;
            int createdM5;
            int closedM5;
            int lifecycleBars;
            double pips;
            double netProfit;
            double realizedR;
            long ticks;

            if (!long.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out positionId) ||
                !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out direction) ||
                !int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out lane) ||
                !int.TryParse(parts[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out entryMode) ||
                !int.TryParse(parts[5], NumberStyles.Integer, CultureInfo.InvariantCulture, out confidence) ||
                !int.TryParse(parts[6], NumberStyles.Integer, CultureInfo.InvariantCulture, out confidenceBucket) ||
                !int.TryParse(parts[7], NumberStyles.Integer, CultureInfo.InvariantCulture, out createdM5) ||
                !int.TryParse(parts[8], NumberStyles.Integer, CultureInfo.InvariantCulture, out closedM5) ||
                !int.TryParse(parts[9], NumberStyles.Integer, CultureInfo.InvariantCulture, out lifecycleBars) ||
                !double.TryParse(parts[10], NumberStyles.Float, CultureInfo.InvariantCulture, out pips) ||
                !double.TryParse(parts[11], NumberStyles.Float, CultureInfo.InvariantCulture, out netProfit) ||
                !double.TryParse(parts[12], NumberStyles.Float, CultureInfo.InvariantCulture, out realizedR) ||
                !long.TryParse(parts[17], NumberStyles.Integer, CultureInfo.InvariantCulture, out ticks))
                return false;

            if (positionId <= 0 ||
                (direction != 1 && direction != -1) ||
                !Enum.IsDefined(typeof(OpportunityLane), lane) ||
                !Enum.IsDefined(typeof(ExecutionMode), entryMode))
                return false;

            item =
                new OutcomeObservation
                {
                    PositionId = positionId,
                    Direction = direction,
                    Lane = (OpportunityLane)lane,
                    EntryMode = (ExecutionMode)entryMode,
                    Regime = Decode(parts[4]),
                    Confidence = NumericGuards.ClampInt(confidence, 0, 100),
                    ConfidenceBucket = confidenceBucket,
                    CreatedM5 = createdM5,
                    ClosedM5 = closedM5,
                    LifecycleBars = Math.Max(0, lifecycleBars),
                    Pips = pips,
                    NetProfit = netProfit,
                    RealizedR = realizedR,
                    Profitable = parts[13] == "1",
                    CalibrationEligible = parts[14] == "1",
                    ProtectionRecoveryAtClose = parts[15] == "1",
                    ServerSideTakeProfitLadderActive = parts[16] == "1",
                    ObservedUtcTicks = ticks
                };

            return true;
        }

        private static string Encode(string value)
        {
            return Convert.ToBase64String(
                Encoding.UTF8.GetBytes(
                    value ?? ""));
        }

        private static string Decode(string value)
        {
            try
            {
                return Encoding.UTF8.GetString(
                    Convert.FromBase64String(
                        value ?? ""));
            }
            catch
            {
                return "";
            }
        }

        private void PersistOutcomeHistory()
        {
            try
            {
                LocalStorage.SetString(
                    OutcomeMemoryKey(),
                    SerializeOutcomeHistory(),
                    LocalStorageScope.Type);

                // SetString updates the local-storage value; cTrader persists
                // local storage automatically. The explicit disk flush is
                // deferred to the runtime heartbeat so outcome recording never
                // blocks the calculation hot path.
                MarkOutcomeMemoryPersistenceDirty();
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP outcome memory queue failed: {0}",
                    ex.Message);
            }
        }

        private void RebuildOutcomeAggregates()
        {
            _directionSamples.Clear();
            _directionWins.Clear();
            _calibrationSamples.Clear();
            _calibrationWins.Clear();

            foreach (OutcomeObservation item
                     in _outcomeHistory)
            {
                if (item == null)
                    continue;

                if (!_directionSamples.ContainsKey(item.Direction))
                    _directionSamples[item.Direction] = 0;
                if (!_directionWins.ContainsKey(item.Direction))
                    _directionWins[item.Direction] = 0;

                _directionSamples[item.Direction]++;

                if (item.Profitable)
                    _directionWins[item.Direction]++;

                if (!item.CalibrationEligible)
                    continue;

                ConfidenceCalibrationKey key =
                    new ConfidenceCalibrationKey(
                        item.Direction,
                        item.Lane,
                        item.Regime,
                        EmpiricalConfidenceCalibrator.ConfidenceBucket(
                            item.Confidence));

                if (!_calibrationSamples.ContainsKey(key))
                    _calibrationSamples[key] = 0;
                if (!_calibrationWins.ContainsKey(key))
                    _calibrationWins[key] = 0;

                _calibrationSamples[key]++;

                if (item.Profitable)
                    _calibrationWins[key]++;
            }
        }
    }
}
