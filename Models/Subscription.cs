namespace StockBE
{
  public class Subscription
  {
    public Subscription(string symbol)
    {
      this.symbol = symbol.ToUpper();
      this.type = "subscribe";
    }

    public string symbol { get; set; }
    public string type { get; set; }
  }
}
