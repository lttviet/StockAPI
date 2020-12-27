using Google.Cloud.Firestore;

namespace StockBE
{
  [FirestoreData]
  public class Stock
  {
    [FirestoreProperty]
    public double cost { get; set; }

    [FirestoreProperty]
    public int quantity { get; set; }

    [FirestoreProperty]
    public string symbol { get; set; }

    public override string ToString()
    {
      return $"Symbol: {this.symbol} - Quantity: {this.quantity} - Cost: {this.cost}";
    }
  }
}
