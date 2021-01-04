using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.Firestore;

namespace StockBE.DataAccess
{
  public class BrokerDataAccess
  {
    private readonly string projectID = "";
    private readonly string credentialFile = @"";
    private readonly FirestoreDb db;

    public BrokerDataAccess()
    {
      Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialFile);
      db = FirestoreDb.Create(projectID);
    }

    public async Task<double?> GetCashAsync(string portfolioId)
    {
      DocumentReference document = db.Document($"portfolio/{portfolioId}");
      DocumentSnapshot snapshot = await document.GetSnapshotAsync();
      if (snapshot.Exists)
      {
        return snapshot.GetValue<double?>("cash");
      }
      return null;
    }

    public async Task UpdateCashAsync(string portfolioId, double newValue)
    {
      DocumentReference document = db.Document($"portfolio/{portfolioId}");
      await document.UpdateAsync("cash", newValue);
    }

    public async Task<List<Stock>> GetStocks(string portfolioId)
    {
      CollectionReference collection = db.Collection($"portfolio/{portfolioId}/stocks");
      QuerySnapshot querySnapshot = await collection.GetSnapshotAsync();

      List<Stock> stocks = new List<Stock>();
      foreach (DocumentSnapshot queryResult in querySnapshot.Documents)
      {
        Stock stock = queryResult.ConvertTo<Stock>();
        stocks.Add(stock);
      }
      return stocks;
    }

    public async Task<bool> BuyStock(string portfolioId, Order order)
    {
      double price;
      if (!Double.TryParse(order.price, out price))
      {
        throw new ArgumentException($"{order.price} isn't a decimal number");
      }

      string symbol = order.symbol;
      int quantity = order.quantity;

      double totalCost = price * order.quantity;
      double? cash = await GetCashAsync(portfolioId);
      if (cash == null || cash < totalCost)
      {
        Console.WriteLine($"Not enought cash. Cash: {cash} - Cost: {totalCost}");
        return false;
      }

      try
      {
        await db.RunTransactionAsync(async transaction =>
        {
          // Read
          DocumentReference cashRef = db.Document($"portfolio/{portfolioId}");
          DocumentSnapshot cashSnapshot = await transaction.GetSnapshotAsync(cashRef);

          CollectionReference stocksRef = cashRef.Collection("stocks");
          Query query = stocksRef.WhereEqualTo("symbol", symbol).Limit(1);
          QuerySnapshot querySnapshot = await transaction.GetSnapshotAsync(query);

          // Update cash         
          double newCash = cashSnapshot.GetValue<double>("cash") - totalCost;
          transaction.Update(cashRef, "cash", newCash);

          // Update stocks
          if (querySnapshot.Count == 1)
          {
            DocumentSnapshot stockSnapshot = querySnapshot.Documents[0];
            int newQuantity = stockSnapshot.GetValue<int>("quantity") + quantity;
            double newCost = stockSnapshot.GetValue<double>("cost") + totalCost;
            Dictionary<string, object> updates =
              new Dictionary<string, object>
              {
                {"quantity", newQuantity},
                {"cost", newCost}
              };
            transaction.Update(stockSnapshot.Reference, updates);
          }
          else
          {
            Stock stock = new Stock
            {
              symbol = symbol,
              quantity = quantity,
              cost = totalCost
            };
            transaction.Create(stocksRef.Document(), stock);
          }
        });
        return true;
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex);
        return false;
      }
    }

    public async Task<bool> SellStock(string portfolioId, Order order)
    {
      double price;
      if (!Double.TryParse(order.price, out price))
      {
        throw new ArgumentException($"{order.price} isn't a decimal number");
      }

      string symbol = order.symbol;
      int quantity = order.quantity;

      CollectionReference stocksRef = db.Collection($"portfolio/{portfolioId}/stocks");
      Query query = stocksRef.WhereEqualTo("symbol", symbol).Limit(1);
      QuerySnapshot querySnapshot = await query.GetSnapshotAsync();
      if (querySnapshot.Count != 1)
      {
        Console.WriteLine("No stock to sell.");
        return false;
      }
      else if (querySnapshot.Documents[0].GetValue<int>("quantity") < quantity)
      {
        Console.WriteLine("Not enought stocks to sell.");
        return false;
      }

      double totalCost = price * quantity;
      try
      {
        await db.RunTransactionAsync(async transaction =>
        {
          // Read
          DocumentReference cashRef = db.Document($"portfolio/{portfolioId}");
          DocumentSnapshot cashSnapshot = await transaction.GetSnapshotAsync(cashRef);

          // Update cash
          double newCash = cashSnapshot.GetValue<double>("cash") + totalCost;
          transaction.Update(cashRef, "cash", newCash);

          // Update stock
          DocumentSnapshot stockSnapshot = querySnapshot.Documents[0];
          int newQuantity = stockSnapshot.GetValue<int>("quantity") - quantity;
          double newCost = stockSnapshot.GetValue<double>("cost") - totalCost;
          Dictionary<string, object> updates =
            new Dictionary<string, object>
            {
              {"quantity", newQuantity},
              {"cost", newCost}
            };
          transaction.Update(querySnapshot.Documents[0].Reference, updates);
        });
        return true;
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex);
        return false;
      }
    }

    public async Task SubscribeCashDocumentAsync(
      string portfolioId,
      Action<DocumentSnapshot> callback,
      CancellationToken stoppingToken = default
    )
    {
      DocumentReference document = db.Document($"portfolio/{portfolioId}");
      DocumentSnapshot snapshot = await document.GetSnapshotAsync();
      if (snapshot.Exists)
      {
        document.Listen(callback, stoppingToken);
      }
    }

    public async Task SubscribeStockCollectionAsync(
      string portfolioId,
      Action<QuerySnapshot> callback,
      CancellationToken stoppingToken = default
    )
    {
      DocumentReference document = db.Document($"portfolio/{portfolioId}");
      DocumentSnapshot snapshot = await document.GetSnapshotAsync();
      if (snapshot.Exists)
      {
        CollectionReference collection = document.Collection("stocks");
        collection.Listen(callback, stoppingToken);
      }
    }
  }
}