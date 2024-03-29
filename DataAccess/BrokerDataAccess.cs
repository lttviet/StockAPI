using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;

namespace StockBE.DataAccess
{
  public class BrokerDataAccess
  {
    private readonly FirestoreDb db;

    public BrokerDataAccess(IConfiguration configuration)
    {
      string credentialFile = configuration["GoogleCredential"];
      string projectID = configuration["GoogleProjectId"];

      Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialFile);
      db = FirestoreDb.Create(projectID);
    }

    public async Task<long?> GetCashAsync(string portfolioId)
    {
      DocumentSnapshot snapshot = await GetPorfolioSnapshotAsync(portfolioId);
      if (snapshot.Exists)
      {
        return snapshot.GetValue<long>("cash");
      }
      return null;
    }

    public async Task UpdateCashAsync(string portfolioId, long newValue)
    {
      DocumentSnapshot snapshot = await GetPorfolioSnapshotAsync(portfolioId);
      await snapshot.Reference.UpdateAsync("cash", newValue);
    }

    public async Task<long?> GetPortfolioValueAsync(string portfolioId)
    {
      DocumentSnapshot snapshot = await GetPorfolioSnapshotAsync(portfolioId);
      if (snapshot.Exists)
      {
        return snapshot.GetValue<long>("value");
      }
      return null;
    }

    public async Task UpdatePortfolioValueAsync(string portfolioId, long newValue)
    {
      DocumentSnapshot snapshot = await GetPorfolioSnapshotAsync(portfolioId);
      await snapshot.Reference.UpdateAsync("value", newValue);
    }

    // TODO use cloud functions
    public void CreateAutoUpdatePortfolioValueListener(string portfolioId, CancellationToken stoppingToken)
    {
      CollectionReference collection = db.Collection($"portfolio/{portfolioId}/stocks");
      FirestoreChangeListener listener = collection.Listen(
        async snapshot =>
        {
          long newValue = 0;
          foreach (DocumentSnapshot document in snapshot.Documents)
          {
            if (document.Exists)
            {
              long cost = document.GetValue<long>("cost");
              newValue = checked(newValue + cost);
            }
          }
          await UpdatePortfolioValueAsync(portfolioId, newValue);
        }
      );
    }

    public async Task<List<Stock>> GetStocks(string portfolioId)
    {
      QuerySnapshot querySnapshot = await GetAllStockQuerySnapshotAsync(portfolioId);

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
      string symbol = order.symbol;
      int orderQuantity = order.quantity;
      long orderPrice = order.price;
      long cash = 0;
      long orderCost = 0;

      try
      {
        orderCost = checked(orderPrice * orderQuantity);

        bool done = await db.RunTransactionAsync<bool>(async transaction =>
        {
          // Read
          DocumentSnapshot portfolioSnapshot = await GetPorfolioSnapshotAsync(portfolioId);
          bool found = portfolioSnapshot.TryGetValue<long>("cash", out cash);

          if (!found || cash < orderCost)
          {
            Console.WriteLine($"Not enought cash. Cash: {cash} - Cost: {orderCost}");
            return false;
          }

          CollectionReference stockCollection = portfolioSnapshot.Reference.Collection("stocks");
          Query query = stockCollection.WhereEqualTo("symbol", symbol).Limit(1);
          QuerySnapshot querySnapshot = await transaction.GetSnapshotAsync(query);

          // Update cash
          long newCash = cash - orderCost;
          transaction.Update(portfolioSnapshot.Reference, "cash", newCash);

          // Update stocks
          if (querySnapshot.Count == 1)
          {
            DocumentSnapshot stockSnapshot = querySnapshot.Documents[0];

            int currentQuantity = stockSnapshot.GetValue<int>("quantity");
            int newQuantity = checked(currentQuantity + orderQuantity);

            long currentCost = stockSnapshot.GetValue<long>("cost");
            long newCost = checked(currentCost + orderCost);

            Dictionary<string, object> updates =
              new Dictionary<string, object>
              {
                {"quantity",  newQuantity},
                {"cost",  newCost}
              };
            transaction.Update(stockSnapshot.Reference, updates);
          }
          else
          {
            Stock stock = new Stock
            {
              symbol = symbol,
              quantity = orderQuantity,
              cost = orderCost
            };
            transaction.Create(stockCollection.Document(), stock);
          }

          return true;
        });

        return done;
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex);
        return false;
      }
    }

    public async Task<bool> SellStock(string portfolioId, Order order)
    {
      string symbol = order.symbol;
      int orderQuantity = order.quantity;
      long orderPrice = order.price;
      long orderCost = 0;

      try
      {
        orderCost = checked(orderPrice * orderQuantity);

        bool done = await db.RunTransactionAsync<bool>(async transaction =>
        {
          // Read
          DocumentSnapshot portfolioSnapshot = await GetPorfolioSnapshotAsync(portfolioId);

          CollectionReference stockCollection = portfolioSnapshot.Reference.Collection("stocks");
          Query query = stockCollection.WhereEqualTo("symbol", symbol).Limit(1);
          QuerySnapshot querySnapshot = await transaction.GetSnapshotAsync(query);

          if (querySnapshot.Count != 1)
          {
            Console.WriteLine("No stock to sell.");
            return false;
          }
          else if (querySnapshot.Documents[0].GetValue<int>("quantity") < orderQuantity)
          {
            Console.WriteLine("Not enought stocks to sell.");
            return false;
          }

          // Update cash
          long currentCash = portfolioSnapshot.GetValue<long>("cash");
          long newCash = checked(currentCash + orderCost);
          transaction.Update(portfolioSnapshot.Reference, "cash", newCash);

          // Update stock
          DocumentSnapshot stockSnapshot = querySnapshot.Documents[0];

          int currentQuantity = stockSnapshot.GetValue<int>("quantity");
          int newQuantity = currentQuantity - orderQuantity;

          long currentCost = stockSnapshot.GetValue<long>("cost");
          long newCost = currentCost - orderCost;

          Dictionary<string, object> updates =
            new Dictionary<string, object>
            {
              {"quantity", newQuantity},
              {"cost", newCost}
            };
          transaction.Update(querySnapshot.Documents[0].Reference, updates);

          return true;
        });

        return done;
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex);
        return false;
      }
    }

    public async Task SubscribePortfolioDocumentAsync(
      string portfolioId,
      Action<DocumentSnapshot> callback,
      CancellationToken stoppingToken = default
    )
    {
      DocumentSnapshot snapshot = await GetPorfolioSnapshotAsync(portfolioId);
      if (snapshot.Exists)
      {
        snapshot.Reference.Listen(callback, stoppingToken);
      }
    }

    public async Task SubscribeStockCollectionAsync(
      string portfolioId,
      Action<QuerySnapshot> callback,
      CancellationToken stoppingToken = default
    )
    {
      QuerySnapshot querySnapshot = await GetAllStockQuerySnapshotAsync(portfolioId);
      if (querySnapshot.Count > 0)
      {
        querySnapshot.Query.Listen(callback, stoppingToken);
      }
    }

    public async Task<QuerySnapshot> GetAllStockQuerySnapshotAsync(string portfolioId)
    {
      CollectionReference collection = db.Collection($"portfolio/{portfolioId}/stocks");
      return await collection.GetSnapshotAsync();
    }

    private async Task<DocumentSnapshot> GetPorfolioSnapshotAsync(string portfolioId)
    {
      DocumentReference document = db.Document($"portfolio/{portfolioId}");
      return await document.GetSnapshotAsync();
    }
  }
}