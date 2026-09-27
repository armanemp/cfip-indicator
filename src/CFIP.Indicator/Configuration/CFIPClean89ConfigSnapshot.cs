// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public sealed class CFIPClean89ConfigSnapshot
        {
            private readonly ReadOnlyDictionary<string, object> _values;
            private readonly ReadOnlyDictionary<string, CFIPClean89ConfigurationSectionSnapshot> _sections;
    
            public string StrategyId { get; private set; }
            public int ParameterCount { get { return _values.Count; } }
            public bool ManualTradeEntryControlsSupported { get { return false; } }
    
            public IReadOnlyDictionary<string, object> Values { get { return _values; } }
            public IReadOnlyDictionary<string, CFIPClean89ConfigurationSectionSnapshot> Sections
            {
                get { return _sections; }
            }
    
            private CFIPClean89ConfigSnapshot(
                string strategyId,
                IDictionary<string, object> values,
                IDictionary<string, CFIPClean89ConfigurationSectionSnapshot> sections)
            {
                StrategyId = strategyId ?? string.Empty;
                _values = new ReadOnlyDictionary<string, object>(
                    new Dictionary<string, object>(values));
    
                _sections =
                    new ReadOnlyDictionary<string, CFIPClean89ConfigurationSectionSnapshot>(
                        new Dictionary<string, CFIPClean89ConfigurationSectionSnapshot>(sections));
            }
    
            public static CFIPClean89ConfigSnapshot Build(
                object parameterSource,
                string strategyId)
            {
                if (parameterSource == null)
                    throw new ArgumentNullException("parameterSource");
    
                var values =
                    new Dictionary<string, object>(StringComparer.Ordinal);
    
                var buckets =
                    new Dictionary<CFIPClean89ConfigurationSection, IDictionary<string, object>>();
    
                foreach (CFIPClean89ConfigurationSection section in Enum.GetValues(
                    typeof(CFIPClean89ConfigurationSection)))
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
    
                    CFIPClean89ConfigurationSection section =
                        ResolveSection(group);
    
                    buckets[section][property.Name] = value;
                }
    
                var sections =
                    new Dictionary<string, CFIPClean89ConfigurationSectionSnapshot>(
                        StringComparer.Ordinal);
    
                foreach (var pair in buckets)
                {
                    sections[pair.Key.ToString()] =
                        new CFIPClean89ConfigurationSectionSnapshot(
                            pair.Key,
                            pair.Value);
                }
    
                return new CFIPClean89ConfigSnapshot(
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
    
            public static CFIPClean89ConfigurationSection ResolveSection(
                string group)
            {
                string value = group ?? string.Empty;
    
                if (value.Contains("01 · Decision"))
                    return CFIPClean89ConfigurationSection.Decision;
                if (value.Contains("02 · MTF"))
                    return CFIPClean89ConfigurationSection.Mtf;
                if (value.Contains("03 · Structure"))
                    return CFIPClean89ConfigurationSection.Structure;
                if (value.Contains("04 · Zones"))
                    return CFIPClean89ConfigurationSection.Zones;
                if (value.Contains("05 · Liquidity"))
                    return CFIPClean89ConfigurationSection.Liquidity;
                if (value.Contains("06 · Indicators"))
                    return CFIPClean89ConfigurationSection.Indicators;
                if (value.Contains("07 · Entry"))
                    return CFIPClean89ConfigurationSection.Entry;
                if (value.Contains("08 · Smart Weights"))
                    return CFIPClean89ConfigurationSection.SmartWeights;
                if (value.Contains("09 · Risk & Targets"))
                    return CFIPClean89ConfigurationSection.RiskTargets;
                if (value.Contains("10 · Live Management"))
                    return CFIPClean89ConfigurationSection.LiveManagement;
                if (value.Contains("11 · Filters"))
                    return CFIPClean89ConfigurationSection.Filters;
                if (value.Contains("12 · ALERTS"))
                    return CFIPClean89ConfigurationSection.Alerts;
                if (value.Contains("13 · AUTO TRADING"))
                    return CFIPClean89ConfigurationSection.Automation;
                if (value.Contains("24 · SMART EXECUTION"))
                    return CFIPClean89ConfigurationSection.SmartExecution;
                if (value.Contains("14 · DISPLAY"))
                    return CFIPClean89ConfigurationSection.Display;
                if (value.Contains("15 · CONTROL"))
                    return CFIPClean89ConfigurationSection.Control;
                if (value.Contains("20 · Confluence"))
                    return CFIPClean89ConfigurationSection.ConfluenceExtensions;
                if (value.Contains("21 · Complete Intelligence"))
                    return CFIPClean89ConfigurationSection.CompleteIntelligence;
                if (value.Contains("23 · Structural Execution"))
                    return CFIPClean89ConfigurationSection.StructuralExecution;
                if (value.Contains("22 · Safety"))
                    return CFIPClean89ConfigurationSection.SafetyPrecision;
                if (value.Contains("15 · INTELLIGENCE — EARLY"))
                    return CFIPClean89ConfigurationSection.EarlyIntelligence;
                if (value.Contains("16 · Accuracy"))
                    return CFIPClean89ConfigurationSection.Accuracy;
                if (value.Contains("17 · Smart Engine"))
                    return CFIPClean89ConfigurationSection.SmartEngine;
    
                return CFIPClean89ConfigurationSection.Unknown;
            }
        }
}
