using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

using StockBE.DataAccess;

namespace StockBE.Hubs
{
  public class BrokerHub : Hub
  {
    private readonly BrokerDataAccess db;

    public BrokerHub(BrokerDataAccess db)
    {
      this.db = db;
    }

    public async Task GetCash(string portfolioId)
    {
      long? cash = await db.GetCashAsync(portfolioId);
      if (cash != null)
      {
        await Clients.Caller.SendAsync("ReceiveCash", cash);
      }
    }

    public async Task GetPortfolioValue(string portfolioId)
    {
      long? value = await db.GetPortfolioValueAsync(portfolioId);
      if (value != null)
      {
        await Clients.Caller.SendAsync("ReceivePortfolioValue", value);
      }
    }

    public async Task GetStocks(string portfolioId)
    {
      List<Stock> stocks = await db.GetStocks(portfolioId);
      if (stocks.Count > 0)
      {
        await Clients.Caller.SendAsync(
          "ReceiveStocks",
          new ReadOnlyCollection<Stock>(stocks)
        );
      }
    }
  }
}
