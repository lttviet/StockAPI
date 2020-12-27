using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

using StockBE.DataAccess;

namespace StockBE.Services
{
  public class BrokerService : BackgroundService
  {
    private readonly BrokerDataAccess db;

    public BrokerService(BrokerDataAccess db)
    {
      this.db = db;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      // For testing, subscribe to portfolio 1 document and stocks collection
      // TODO: look into queued background task
      await db.SubscribeCashDocAsync("1");
      await db.SubscribeStockCollectionAsync("1");
    }
  }
}