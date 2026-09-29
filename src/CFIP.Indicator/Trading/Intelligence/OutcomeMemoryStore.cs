using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Reflection;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator
    {
        private const string OutcomeMemorySchema = "CFIP-OUTCOME";
        private const int OutcomeMemoryMaxAgeDays = 90;

        private string OutcomeMemoryKey()
        {
            string symbol =
                string.IsNullOrWhiteSpace(SymbolName)
                    ? "UNKNOWN"
                    : SymbolName;

            string timeframe =
                Bars == null
                    ? "UNKNOWN"
                    : Bars.TimeFrame.ToString();

            StringBuilder key =
                new StringBuilder("CFIP.OUTCOME.");

            foreach (char ch in symbol)
                key.Append(
                    char.IsLetterOrDigit(ch) ? ch : '_');

            key.Append('.');

            foreach (char ch in timeframe)
                key.Append(
                    char.IsLetterOrDigit(ch) ? ch : '_');

            key.Append('.');
            key.Append(MemoryConfigurationFingerprint());
            key.Append(".MEM");
            return key.ToString();
        }

        private string MemoryConfigurationFingerprint()
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

            StringBuilder raw =
                new StringBuilder();

            int parameterCount = 0;

            foreach (PropertyInfo property in properties)
            {
                if (property.GetCustomAttributes(
                        typeof(ParameterAttribute),
                        true).Length == 0)
                    continue;

                object value;

                try
                {
                    value = property.GetValue(this, null);
                }
                catch
                {
                    value = null;
                }

                string formatted;

                IFormattable formattable =
                    value as IFormattable;

                if (formattable != null)
                {
                    formatted =
                        formattable.ToString(
                            null,
                            CultureInfo.InvariantCulture);
                }
                else
                {
                    formatted =
                        value == null
                            ? ""
                            : value.ToString();
                }

                raw.Append(property.Name);
                raw.Append('=');
                raw.Append(formatted);
                raw.Append(';');
                parameterCount++;
            }

            unchecked
            {
                uint hash = 2166136261;

                hash ^= (uint)parameterCount;
                hash *= 16777619;

                for (int i = 0; i < raw.Length; i++)
                {
                    hash ^= raw[i];
                    hash *= 16777619;
                }

                return hash.ToString(
                    "X8",
                    CultureInfo.InvariantCulture);
            }
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
                string stored =
                    LocalStorage.GetString(
                        OutcomeMemoryKey(),
                        LocalStorageScope.Type);

                if (string.IsNullOrWhiteSpace(stored))
                    return false;

                string[] lines =
                    stored.Split(
                        new[] { '\n' },
                        StringSplitOptions.RemoveEmptyEntries);

                if (lines.Length == 0 ||
                    !string.Equals(
                        lines[0].Trim(),
                        OutcomeMemorySchema,
                        StringComparison.Ordinal))
                    return false;

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
                    Regime = Unescape(parts[4]),
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

                LocalStorage.Flush(
                    LocalStorageScope.Type);
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP outcome memory persist failed: {0}",
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
