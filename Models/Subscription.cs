namespace StockBE
{
  public class Subscription
  {
    public Subscription(string type, string symbol)
    {
      this.symbol = symbol.ToUpper();
      this.type = type;
    }

    public string symbol { get; set; }
    public string type { get; set; }
  }
}
