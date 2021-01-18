using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Google.Cloud.Firestore;

using StockBE.DataAccess;

namespace StockBE.Services
{
  public class BrokerUpdateWorker : BackgroundService
  {
    private readonly BrokerDataAccess db;
    private readonly QuoteClient quoteClient;
    private readonly string portfolioId = "1";
    private readonly int updateInterval = 1;

    public BrokerUpdateWorker(BrokerDataAccess db, QuoteClient quoteClient)
    {
      this.db = db;
      this.quoteClient = quoteClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      // Update portfolio value whenever firestore changes
      // TODO use cloud functions
      db.CreateAutoUpdatePortfolioValueListener(portfolioId, stoppingToken);

      // Every hour, update last close price for all stocks
      // TODO use cloud functions
      while (!stoppingToken.IsCancellationRequested)
      {
        await UpdateLastClosePrice();
        await Task.Delay(TimeSpan.FromHours(updateInterval), stoppingToken);
      }
    }

    private async Task UpdateLastClosePrice()
    {
      QuerySnapshot querySnapshot = await db.GetAllStockQuerySnapshotAsync(portfolioId);
      foreach (DocumentSnapshot document in querySnapshot.Documents)
      {
        string symbol = document.GetValue<string>("symbol");
        long closePrice = await quoteClient.GetLastClosePrice(symbol);
        await document.Reference.UpdateAsync("closePrice", closePrice);
      }
    }
  }
}