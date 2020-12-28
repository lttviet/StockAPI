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