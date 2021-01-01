namespace StockBE
{
  public class QuoteResponse
  {
    public QuoteData[] data { get; set; }
    public string type { get; set; }

    public Quote ToQuote()
    {
      if (this.type == "trade")
      {
        return new Quote
        {
          price = this.data[0].p,
          symbol = this.data[0].s,
          timestamp = this.data[0].t
        };
      }
      return null;
    }
  }

  public class QuoteData
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
    public decimal price { get; set; }
    public long timestamp { get; set; }
  }
}
