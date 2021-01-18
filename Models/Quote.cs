using System;
using System.Text.Json.Serialization;

namespace StockBE
{
  public class QuoteSocketResponse
  {
    public QuoteSocketData[] data { get; set; }
    public string type { get; set; }

    public Quote ToQuote()
    {
      if (this.type == "trade")
      {
        return new Quote
        {
          price = (long)Decimal.Round(this.data[0].p * 100),
          symbol = this.data[0].s,
          timestamp = this.data[0].t
        };
      }
      return null;
    }
  }

  public class QuoteSocketData
  {
    public string[] c { get; set; }
    public decimal p { get; set; }
    public string s { get; set; }
    public long t { get; set; }
    public int v { get; set; }
  }

  public class Quote
  {
    public string symbol { get; set; }
    public long price { get; set; }
    public long timestamp { get; set; }

    public override string ToString()
    {
      return $"Quote: ${symbol} for ${price} at {timestamp}";
    }
  }
}
