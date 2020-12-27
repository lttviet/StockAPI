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

    public async Task SubscribeCashDocAsync(string portfolioId)
    {
      DocumentReference document = db.Document($"portfolio/{portfolioId}");
      DocumentSnapshot snapshot = await document.GetSnapshotAsync();
      if (snapshot.Exists)
      {
        FirestoreChangeListener listener = document.Listen(snapshot =>
        {
          Console.WriteLine($"Cash value: {snapshot.GetValue<double?>("cash")}");
        });
      }
    }

    public async Task SubscribeStockCollectionAsync(string portfolioId)
    {
      DocumentReference document = db.Document($"portfolio/{portfolioId}");
      DocumentSnapshot snapshot = await document.GetSnapshotAsync();
      if (snapshot.Exists)
      {
        CollectionReference collection = document.Collection("stocks");

        collection.Listen(snapshot =>
        {
          List<Stock> stocks = new List<Stock>();
          foreach (DocumentSnapshot queryResult in snapshot.Documents)
          {
            Stock stock = queryResult.ConvertTo<Stock>();
            stocks.Add(stock);
          }
        });    
      }
    }
  }
}