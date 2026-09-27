// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public sealed class CFIPClean89ConfigurationSectionSnapshot
        {
            private readonly ReadOnlyDictionary<string, object> _values;
    
            public CFIPClean89ConfigurationSection Section { get; private set; }
            public IReadOnlyDictionary<string, object> Values { get { return _values; } }
    
            public CFIPClean89ConfigurationSectionSnapshot(
                CFIPClean89ConfigurationSection section,
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
}
