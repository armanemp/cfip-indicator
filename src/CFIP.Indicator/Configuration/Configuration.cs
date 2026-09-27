using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CFIP.Indicator
{
        public enum ConfigurationSection
        {
            Decision,
            Mtf,
            Structure,
            Zones,
            Liquidity,
            Indicators,
            Entry,
            SmartWeights,
            RiskTargets,
            LiveManagement,
            Filters,
            Alerts,
            Automation,
            SmartExecution,
            Display,
            Control,
            ConfluenceExtensions,
            CompleteIntelligence,
            StructuralExecution,
            SafetyPrecision,
            EarlyIntelligence,
            Accuracy,
            SmartEngine,
            Unknown
        }
    
        public sealed class ConfigurationSectionSnapshot
        {
            private readonly ReadOnlyDictionary<string, object> _values;
    
            public ConfigurationSection Section { get; private set; }
            public IReadOnlyDictionary<string, object> Values { get { return _values; } }
    
            public ConfigurationSectionSnapshot(
                ConfigurationSection section,
                IDictionary<string, object> values)
            {
                Section = section;
                _values = new ReadOnlyDictionary<string, object>(
                    new Dictionary<string, object>(
                        values ??
                        new Dictionary<string, object>()));
            }
    
            public T Get<T>(string name, T fallback)
            {
                object value;
                if (!_values.TryGetValue(name ?? string.Empty, out value) ||
                    value == null)
                    return fallback;
    
                if (value is T)
                    return (T)value;
    
                try
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch
                {
                    return fallback;
                }
            }
        }
    
        public sealed class ConfigSnapshot
        {
            private readonly ReadOnlyDictionary<string, object> _values;
            private readonly ReadOnlyDictionary<string, ConfigurationSectionSnapshot> _sections;
    
            public string StrategyId { get; private set; }
            public int ParameterCount { get { return _values.Count; } }
            public bool ManualTradeEntryControlsSupported { get { return false; } }
    
            public IReadOnlyDictionary<string, object> Values { get { return _values; } }
            public IReadOnlyDictionary<string, ConfigurationSectionSnapshot> Sections
            {
                get { return _sections; }
            }
    
            private ConfigSnapshot(
                string strategyId,
                IDictionary<string, object> values,
                IDictionary<string, ConfigurationSectionSnapshot> sections)
            {
                StrategyId = strategyId ?? string.Empty;
                _values = new ReadOnlyDictionary<string, object>(
                    new Dictionary<string, object>(values));
    
                _sections =
                    new ReadOnlyDictionary<string, ConfigurationSectionSnapshot>(
                        new Dictionary<string, ConfigurationSectionSnapshot>(sections));
            }
    
            public static ConfigSnapshot Build(
                object parameterSource,
                string strategyId)
            {
                if (parameterSource == null)
                    throw new ArgumentNullException("parameterSource");
    
                var values =
                    new Dictionary<string, object>(StringComparer.Ordinal);
    
                var buckets =
                    new Dictionary<ConfigurationSection, IDictionary<string, object>>();
    
                foreach (ConfigurationSection section in Enum.GetValues(
                    typeof(ConfigurationSection)))
                {
                    buckets[section] =
                        new Dictionary<string, object>(StringComparer.Ordinal);
                }
    
                foreach (var property in parameterSource.GetType().GetProperties())
                {
                    if (!property.CanRead ||
                        property.GetIndexParameters().Length != 0)
                        continue;
    
                    var attributes = property.GetCustomAttributes(false);
                    bool isParameter = false;
                    string group = string.Empty;
    
                    for (int i = 0; i < attributes.Length; i++)
                    {
                        object attribute = attributes[i];
                        if (attribute == null)
                            continue;
    
                        if (!string.Equals(
                            attribute.GetType().Name,
                            "ParameterAttribute",
                            StringComparison.Ordinal))
                            continue;
    
                        isParameter = true;
    
                        var groupProperty =
                            attribute.GetType().GetProperty("Group");
    
                        if (groupProperty != null)
                        {
                            object rawGroup =
                                groupProperty.GetValue(
                                    attribute,
                                    null);
    
                            group =
                                rawGroup == null
                                    ? string.Empty
                                    : rawGroup.ToString();
                        }
    
                        break;
                    }
    
                    if (!isParameter)
                        continue;
    
                    object value = property.GetValue(parameterSource, null);
                    values[property.Name] = value;
    
                    ConfigurationSection section =
                        ResolveSection(group);
    
                    buckets[section][property.Name] = value;
                }
    
                var sections =
                    new Dictionary<string, ConfigurationSectionSnapshot>(
                        StringComparer.Ordinal);
    
                foreach (var pair in buckets)
                {
                    sections[pair.Key.ToString()] =
                        new ConfigurationSectionSnapshot(
                            pair.Key,
                            pair.Value);
                }
    
                return new ConfigSnapshot(
                    strategyId,
                    values,
                    sections);
            }
    
            public object Get(
                string name,
                object fallback)
            {
                object value;
                return
                    _values.TryGetValue(
                        name ?? string.Empty,
                        out value)
                        ? value
                        : fallback;
            }
    
            public T Get<T>(
                string name,
                T fallback)
            {
                object value;
                if (!_values.TryGetValue(
                        name ?? string.Empty,
                        out value) ||
                    value == null)
                    return fallback;
    
                if (value is T)
                    return (T)value;
    
                try
                {
                    return (T)Convert.ChangeType(
                        value,
                        typeof(T));
                }
                catch
                {
                    return fallback;
                }
            }
    
            public static ConfigurationSection ResolveSection(
                string group)
            {
                string value = group ?? string.Empty;
    
                if (value.Contains("01 · Decision"))
                    return ConfigurationSection.Decision;
                if (value.Contains("02 · MTF"))
                    return ConfigurationSection.Mtf;
                if (value.Contains("03 · Structure"))
                    return ConfigurationSection.Structure;
                if (value.Contains("04 · Zones"))
                    return ConfigurationSection.Zones;
                if (value.Contains("05 · Liquidity"))
                    return ConfigurationSection.Liquidity;
                if (value.Contains("06 · Indicators"))
                    return ConfigurationSection.Indicators;
                if (value.Contains("07 · Entry"))
                    return ConfigurationSection.Entry;
                if (value.Contains("08 · Smart Weights"))
                    return ConfigurationSection.SmartWeights;
                if (value.Contains("09 · Risk & Targets"))
                    return ConfigurationSection.RiskTargets;
                if (value.Contains("10 · Live Management"))
                    return ConfigurationSection.LiveManagement;
                if (value.Contains("11 · Filters"))
                    return ConfigurationSection.Filters;
                if (value.Contains("12 · ALERTS"))
                    return ConfigurationSection.Alerts;
                if (value.Contains("13 · AUTO TRADING"))
                    return ConfigurationSection.Automation;
                if (value.Contains("24 · SMART EXECUTION"))
                    return ConfigurationSection.SmartExecution;
                if (value.Contains("14 · DISPLAY"))
                    return ConfigurationSection.Display;
                if (value.Contains("15 · CONTROL"))
                    return ConfigurationSection.Control;
                if (value.Contains("20 · Confluence"))
                    return ConfigurationSection.ConfluenceExtensions;
                if (value.Contains("21 · Complete Intelligence"))
                    return ConfigurationSection.CompleteIntelligence;
                if (value.Contains("23 · Structural Execution"))
                    return ConfigurationSection.StructuralExecution;
                if (value.Contains("22 · Safety"))
                    return ConfigurationSection.SafetyPrecision;
                if (value.Contains("15 · INTELLIGENCE — EARLY"))
                    return ConfigurationSection.EarlyIntelligence;
                if (value.Contains("16 · Accuracy"))
                    return ConfigurationSection.Accuracy;
                if (value.Contains("17 · Smart Engine"))
                    return ConfigurationSection.SmartEngine;
    
                return ConfigurationSection.Unknown;
            }
        }
    
        public enum PanelCorner
        {
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
        }
    
        public enum SizingMode
        {
            RiskPercentEquity = 0,
            FixedLots = 1
        }
    
        public enum PendingOrderMode
        {
            Adaptive = 0,
            ContinuationStop = 1,
            ReversalLimit = 2,
            Both = 3
        }
    
    
}
