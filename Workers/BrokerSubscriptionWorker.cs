using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.SignalR;
using Google.Cloud.Firestore;

using StockBE.Hubs;
using StockBE.DataAccess;

namespace StockBE.Services
{
  public class BrokerSubscriptionWorker : BackgroundService
  {
    private readonly BrokerDataAccess db;
    private readonly IHubContext<BrokerHub> hubContext;
    private readonly string portfolioId = "1";

    public BrokerSubscriptionWorker(BrokerDataAccess db, IHubContext<BrokerHub> hubContext)
    {
      this.db = db;
      this.hubContext = hubContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      await db.SubscribeCashDocumentAsync(
        portfolioId,
        snapshot => hubContext.Clients.All.SendAsync("ReceiveCash", snapshot.GetValue<double?>("cash")),
        stoppingToken
      );

      await db.SubscribeStockCollectionAsync(
        portfolioId,
        snapshot =>
        {
          List<Stock> stocks = new List<Stock>();
          foreach (DocumentSnapshot queryResult in snapshot.Documents)
          {
            Stock stock = queryResult.ConvertTo<Stock>();
            stocks.Add(stock);
          }
          hubContext.Clients.All.SendAsync(
            "ReceiveStocks",
            new ReadOnlyCollection<Stock>(stocks)
          );
        },
        stoppingToken
      );
    }
  }
}