using System;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;

namespace StockBE.DataAccess
{
  public class CandleClient
  {
    private readonly string endpoint = "https://finnhub.io/api/v1/stock/candle?token=";
    private readonly CancellationTokenSource source;

    public CandleClient()
    {
      source = new CancellationTokenSource();
    }

    // DEMO get last 7 days' data
    public async Task<Candle> GetDailyChangeCandle(string symbol)
    {
      DateTimeOffset currentDate = DateTimeOffset.Now;
      long to = currentDate.ToUnixTimeSeconds();

      DateTimeOffset previousWeekDate = currentDate.AddDays(-7);
      long from = previousWeekDate.ToUnixTimeSeconds();

      using (var client = new HttpClient())
      {
        string url = $"{endpoint}&symbol={symbol}&from={from}&to={to}&resolution=D";
        using (var streamTask = client.GetStreamAsync(url))
        {
          var response = await JsonSerializer.DeserializeAsync<CandleAPIResponse>(await streamTask);
          return response.ToCandle(symbol);
        }
      }
    }
  }
}