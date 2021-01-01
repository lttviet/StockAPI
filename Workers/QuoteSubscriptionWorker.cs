using System;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.SignalR;

using StockBE.Hubs;
using StockBE.DataAccess;

namespace StockBE.Services
{
  public class QuoteReceiveWorker : BackgroundService
  {
    private readonly QuoteClient quoteClient;
    private readonly BrokerDataAccess brokerDB;
    private readonly IHubContext<QuoteHub> quoteHub;
    private readonly string portfolioId = "1";

    public QuoteReceiveWorker(
      QuoteClient quoteClient,
      BrokerDataAccess brokerDB,
      IHubContext<QuoteHub> quoteHub
    )
    {
      this.quoteClient = quoteClient;
      this.brokerDB = brokerDB;
      this.quoteHub = quoteHub;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      while (!stoppingToken.IsCancellationRequested)
      {
        await quoteClient.ConnectAsync();
        await SubscribeInitialStocks();
        await quoteClient.ReceiveAsync(Test, stoppingToken);
      }
    }

    private async Task SubscribeInitialStocks()
    {
      List<Stock> stocks = await brokerDB.GetStocks(portfolioId);
      foreach (Stock stock in stocks)
      {
        await quoteClient.SubscribeAsync(stock.symbol);
      }
      await quoteClient.SubscribeAsync("SPY");
      await quoteClient.SubscribeAsync("TSLA");
    }

    private void Test(string s)
    {
      QuoteResponse response = JsonSerializer.Deserialize<QuoteResponse>(s);
      Quote quote = response.ToQuote();
      if (quote != null)
      {
        quoteHub.Clients.All.SendAsync("ReceiveQuote", quote);
        Console.WriteLine(quote);
      }
    }
  }
}