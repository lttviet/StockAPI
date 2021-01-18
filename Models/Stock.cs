using Google.Cloud.Firestore;

namespace StockBE
{
  [FirestoreData]
  public class Stock
  {
    [FirestoreProperty]
    public long cost { get; set; }

    [FirestoreProperty]
    public int quantity { get; set; }

    [FirestoreProperty]
    public string symbol { get; set; }

    [FirestoreProperty]
    public long closePrice { get; set; }

    public override string ToString()
    {
      return $"Symbol: {this.symbol} - Quantity: {this.quantity} - Cost: {this.cost} - Close price: {this.closePrice}";
    }
  }
}
