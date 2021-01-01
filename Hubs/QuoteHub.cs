using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

using StockBE.DataAccess;

namespace StockBE.Hubs
{
  public class QuoteHub : Hub
  {
    private readonly QuoteClient client;

    public QuoteHub(QuoteClient client)
    {
      this.client = client;
    }

    public async Task Subscribe(string symbol)
    {
      await client.SubscribeAsync(symbol);
    }

    public async Task Unsubscribe(string symbol)
    {
      await client.UnsubscribeAsync(symbol);
    }
  }
}
