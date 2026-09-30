using System;
using System.Collections.Generic;
using System.Linq;

namespace cAlgo
{
    internal static class EconomicNewsCurrencyRule
    {
        internal static string[] Resolve(
            string symbolName,
            string additionalCurrencies,
            string symbolCurrencyMap)
        {
            HashSet<string> currencies =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

            string symbol =
                NormalizeSymbol(symbolName);

            string[] known =
            {
                "USD", "EUR", "GBP", "JPY",
                "AUD", "CAD", "NZD", "CHF",
                "CNY", "CNH", "HKD", "SGD",
                "NOK", "SEK", "ZAR"
            };

            for (int i = 0; i < known.Length; i++)
            {
                if (symbol.Contains(known[i]))
                    currencies.Add(known[i]);
            }

            AddCurrencyList(
                currencies,
                additionalCurrencies);

            AddSymbolMapMatches(
                currencies,
                symbol,
                symbolCurrencyMap);

            return currencies
                .Select(x => x.ToUpperInvariant())
                .OrderBy(x => x, StringComparer.Ordinal)
                .ToArray();
        }

        internal static string NormalizeSymbol(
            string symbolName)
        {
            if (string.IsNullOrWhiteSpace(symbolName))
                return "";

            char[] buffer =
                new char[symbolName.Length];

            int count = 0;

            for (int i = 0; i < symbolName.Length; i++)
            {
                char c =
                    char.ToUpperInvariant(
                        symbolName[i]);

                if ((c >= 'A' && c <= 'Z') ||
                    (c >= '0' && c <= '9'))
                {
                    buffer[count++] = c;
                }
            }

            return new string(buffer, 0, count);
        }

        private static void AddCurrencyList(
            HashSet<string> currencies,
            string values)
        {
            if (string.IsNullOrWhiteSpace(values))
                return;

            string[] tokens =
                values.Split(
                    new[] { ',', ';', ' ', '|' },
                    StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < tokens.Length; i++)
            {
                string normalized =
                    tokens[i]
                        .Trim()
                        .ToUpperInvariant();

                if (IsCurrencyCode(normalized))
                    currencies.Add(normalized);
            }
        }

        private static void AddSymbolMapMatches(
            HashSet<string> currencies,
            string normalizedSymbol,
            string map)
        {
            if (string.IsNullOrWhiteSpace(map) ||
                string.IsNullOrWhiteSpace(normalizedSymbol))
                return;

            string[] entries =
                map.Split(
                    new[] { ';', '|', '\n' },
                    StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < entries.Length; i++)
            {
                string entry =
                    entries[i].Trim();

                int separator =
                    entry.IndexOf('=');

                if (separator <= 0 ||
                    separator >= entry.Length - 1)
                    continue;

                string symbolPattern =
                    NormalizeSymbol(
                        entry.Substring(
                            0,
                            separator));

                if (symbolPattern.Length == 0 ||
                    !normalizedSymbol.Contains(
                        symbolPattern,
                        StringComparison.Ordinal))
                    continue;

                AddCurrencyList(
                    currencies,
                    entry.Substring(separator + 1));
            }
        }

        private static bool IsCurrencyCode(
            string value)
        {
            if (string.IsNullOrWhiteSpace(value) ||
                value.Length != 3)
                return false;

            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] < 'A' ||
                    value[i] > 'Z')
                    return false;
            }

            return true;
        }
    }
}
