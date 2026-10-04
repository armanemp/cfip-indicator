using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace cAlgo
{
    internal sealed class CfipEconomicNewsEvent
    {
        public string Title { get; set; }
        public string Currency { get; set; }
        public string UtcDate { get; set; }
        public string UtcTime { get; set; }
        public string Impact { get; set; }
        public string Previous { get; set; }
        public string Forecast { get; set; }
        public DateTimeOffset TimeUtc { get; set; }

        public int ImpactRank
        {
            get
            {
                string impact =
                    string.IsNullOrWhiteSpace(Impact)
                        ? ""
                        : Impact.Trim().ToUpperInvariant();

                if (impact.Contains("HIGH"))
                    return 3;

                if (impact.Contains("MED"))
                    return 2;

                if (impact.Contains("LOW"))
                    return 1;

                return 0;
            }
        }
    }

    [XmlRoot("weeklyevents")]
    public sealed class CfipEconomicCalendar
    {
        [XmlElement("event")]
        public List<CfipEconomicCalendarEventXml> Events { get; set; }
    }

    public sealed class CfipEconomicCalendarEventJson
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("country")]
        public string Currency { get; set; }

        [JsonPropertyName("date")]
        public string UtcTimestamp { get; set; }

        [JsonPropertyName("impact")]
        public string Impact { get; set; }

        [JsonPropertyName("previous")]
        public string Previous { get; set; }

        [JsonPropertyName("forecast")]
        public string Forecast { get; set; }
    }

    public sealed class CfipEconomicCalendarEventXml
    {
        [XmlElement("title")]
        public string Title { get; set; }

        [XmlElement("country")]
        public string Currency { get; set; }

        [XmlElement("date")]
        public string UtcDate { get; set; }

        [XmlElement("time")]
        public string UtcTime { get; set; }

        [XmlElement("impact")]
        public string Impact { get; set; }

        [XmlElement("previous")]
        public string Previous { get; set; }

        [XmlElement("forecast")]
        public string Forecast { get; set; }
    }

    internal static class EconomicNewsCalendarParser
    {
        public static string NormalizeUri(
            string configuredUri)
        {
            string uri =
                string.IsNullOrWhiteSpace(configuredUri)
                    ? "https://nfs.faireconomy.media/ff_calendar_thisweek.json"
                    : configuredUri.Trim();

            if (string.Equals(
                    uri,
                    "https://nfs.faireconomy.media/ff_calendar_thisweek.xml",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "https://nfs.faireconomy.media/ff_calendar_thisweek.json";
            }

            return uri;
        }

        public static CfipEconomicNewsEvent[] Parse(
            string payload,
            string[] relevantCurrencies)
        {
            List<CfipEconomicNewsEvent> next =
                new List<CfipEconomicNewsEvent>();

            if (string.IsNullOrWhiteSpace(payload))
                return next.ToArray();

            string trimmed =
                payload.TrimStart();

            if (trimmed.StartsWith(
                    "[",
                    StringComparison.Ordinal))
            {
                CfipEconomicCalendarEventJson[] parsed =
                    JsonSerializer.Deserialize<CfipEconomicCalendarEventJson[]>(
                        payload,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                if (parsed == null)
                    return next.ToArray();

                for (int i = 0; i < parsed.Length; i++)
                {
                    CfipEconomicCalendarEventJson raw =
                        parsed[i];

                    if (raw == null)
                        continue;

                    DateTimeOffset eventTime;
                    if (!DateTimeOffset.TryParse(
                            raw.UtcTimestamp,
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.AssumeUniversal |
                            DateTimeStyles.AdjustToUniversal,
                            out eventTime))
                        continue;

                    AddEconomicNewsEvent(
                        next,
                        raw.Title,
                        raw.Currency,
                        raw.UtcTimestamp,
                        "",
                        raw.Impact,
                        raw.Previous,
                        raw.Forecast,
                        eventTime,
                        relevantCurrencies);
                }
            }
            else
            {
                XmlSerializer serializer =
                    new XmlSerializer(
                        typeof(CfipEconomicCalendar));

                CfipEconomicCalendar parsed =
                    serializer.Deserialize(
                        new StringReader(payload))
                    as CfipEconomicCalendar;

                if (parsed == null ||
                    parsed.Events == null)
                    return next.ToArray();

                for (int i = 0;
                     i < parsed.Events.Count;
                     i++)
                {
                    CfipEconomicCalendarEventXml raw =
                        parsed.Events[i];

                    if (raw == null)
                        continue;

                    DateTimeOffset eventTime;
                    if (!TryParseEconomicEventTime(
                            raw.UtcDate,
                            raw.UtcTime,
                            out eventTime))
                        continue;

                    AddEconomicNewsEvent(
                        next,
                        raw.Title,
                        raw.Currency,
                        raw.UtcDate,
                        raw.UtcTime,
                        raw.Impact,
                        raw.Previous,
                        raw.Forecast,
                        eventTime,
                        relevantCurrencies);
                }
            }

            return next
                .OrderBy(x => x.TimeUtc)
                .ThenByDescending(x => x.ImpactRank)
                .ToArray();
        }

        private static bool TryParseEconomicEventTime(
            string date,
            string time,
            out DateTimeOffset value)
        {
            value =
                default(DateTimeOffset);

            if (string.IsNullOrWhiteSpace(date) ||
                string.IsNullOrWhiteSpace(time))
                return false;

            string combined =
                date.Trim() +
                " " +
                time.Trim();

            string[] formats =
            {
                "MM-dd-yyyy h:mmtt",
                "MM-dd-yyyy hh:mmtt",
                "MM/dd/yyyy h:mmtt",
                "MM/dd/yyyy hh:mmtt",
                "yyyy-MM-dd HH:mm",
                "yyyy-MM-ddTHH:mm:ss",
                "yyyy-MM-ddTHH:mm:ssZ"
            };

            for (int i = 0;
                 i < formats.Length;
                 i++)
            {
                if (DateTimeOffset.TryParseExact(
                        combined,
                        formats[i],
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal |
                        DateTimeStyles.AdjustToUniversal,
                        out value))
                    return true;
            }

            return DateTimeOffset.TryParse(
                combined,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal |
                DateTimeStyles.AdjustToUniversal,
                out value);
        }

        private static void AddEconomicNewsEvent(
            List<CfipEconomicNewsEvent> target,
            string title,
            string currency,
            string utcDate,
            string utcTime,
            string impact,
            string previous,
            string forecast,
            DateTimeOffset eventTime,
            string[] relevantCurrencies)
        {
            CfipEconomicNewsEvent item =
                new CfipEconomicNewsEvent
                {
                    Title = title ?? "",
                    Currency = currency ?? "",
                    UtcDate = utcDate ?? "",
                    UtcTime = utcTime ?? "",
                    Impact = impact ?? "",
                    Previous = previous ?? "",
                    Forecast = forecast ?? "",
                    TimeUtc = eventTime
                };

            if (item.ImpactRank <= 0)
                return;

            if (relevantCurrencies == null)
                return;

            string normalizedCurrency =
                item.Currency.Trim().ToUpperInvariant();

            for (int i = 0;
                 i < relevantCurrencies.Length;
                 i++)
            {
                if (normalizedCurrency == relevantCurrencies[i])
                {
                    target.Add(item);
                    return;
                }
            }
        }
    }
}
