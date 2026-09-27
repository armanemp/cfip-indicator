using FacioQuo.Stock.Indicators;

var quotes = new List<Quote>();
var start = new DateTime(2020, 1, 1);

for (int i = 0; i < 260; i++)
{
    DateTime date = start.AddMinutes(i * 5);
    decimal open = 100m + i * 0.03m + (i % 7) * 0.08m;
    decimal close = open + (((i % 9) - 4) * 0.04m);
    decimal high = Math.Max(open, close) + 0.12m + (i % 3) * 0.02m;
    decimal low = Math.Min(open, close) - 0.10m - (i % 4) * 0.02m;

    quotes.Add(new Quote
    {
        Date = date,
        Open = open,
        High = high,
        Low = low,
        Close = close,
        Volume = 1000m + i * 5m
    });
}

var rsi = quotes.ToRsi(14);
var macd = quotes.ToMacd(12, 26, 9);
var bollinger = quotes.ToBollingerBands(20, 2);
var mfi = quotes.ToMfi(14);
var stochastic = quotes.ToStoch(14, 3, 3);
var superTrend = quotes.ToSuperTrend(10, 3);

if (rsi.Count != quotes.Count ||
    macd.Count != quotes.Count ||
    bollinger.Count != quotes.Count ||
    mfi.Count != quotes.Count ||
    stochastic.Count != quotes.Count ||
    superTrend.Count != quotes.Count)
{
    throw new InvalidOperationException("Stock Indicators benchmark returned an unexpected series length.");
}

Console.WriteLine(
    $"Stock Indicators benchmark OK: {quotes.Count} bars, " +
    $"RSI={rsi[^1].Rsi:F2}, MACD={macd[^1].Macd:F4}, " +
    $"MFI={mfi[^1].Mfi:F2}.");
