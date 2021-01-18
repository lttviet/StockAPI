using System.ComponentModel.DataAnnotations;

namespace StockBE
{
  public class Order
  {
    [Required]
    public string symbol { get; set; }

    [Required]
    [Range(1, long.MaxValue)]
    public long price { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int quantity { get; set; }

    public override string ToString()
    {
      return $"Symbol: {this.symbol} - Quantity: {this.quantity} - Price: {this.price}";
    }
  }
}
