using System;
using System.Text.Json.Serialization;

namespace StockBE
{
  public class CandleAPIResponse
  {
    [JsonPropertyName("o")]
    public decimal[] openPrice { get; set; }

    [JsonPropertyName("c")]
    public decimal[] closePrice { get; set; }

    [JsonPropertyName("t")]
    public long[] timestamp { get; set; }

    public Candle ToCandle(string symbol)
    {
      Candle candle = new Candle();
      candle.symbol = symbol;
      candle.timestamp = this.timestamp;

      candle.dailyChange = new long[this.openPrice.Length];
      for (int i = 0; i < candle.dailyChange.Length; i++)
      {
        candle.dailyChange[i] = (long)Decimal.Round(this.closePrice[i] * 100) - (long)Decimal.Round(this.openPrice[i] * 100);
      }

      return candle;
    }
  }

  public class Candle
  {
    public string symbol { get; set; }
    public long[] dailyChange { get; set; }
    public long[] timestamp { get; set; }
  }
}
