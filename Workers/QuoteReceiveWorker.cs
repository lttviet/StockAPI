using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using Microsoft.Extensions.Hosting;

using StockBE.Hubs;
using StockBE.DataAccess;

namespace StockBE.Services
{
  public class QuoteReceiveWorker : BackgroundService
  {
    private readonly QuoteClient quoteClient;

    public QuoteReceiveWorker(QuoteClient quoteClient)
    {
      this.quoteClient = quoteClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      while (!stoppingToken.IsCancellationRequested)
      {
        await quoteClient.ConnectAsync();
        await quoteClient.ReceiveAsync(Test);
      }
    }

    private void Test(string s)
    {
      Console.WriteLine(s);
    }
  }
}