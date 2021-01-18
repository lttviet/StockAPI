using System;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Net.Http;

namespace StockBE.DataAccess
{
  public class QuoteClient
  {
    private readonly string wsEndpoint = "ws://echo.websocket.org";
    private readonly ClientWebSocket socket;
    private readonly CancellationTokenSource source;
    private readonly ConcurrentDictionary<string, bool> symbols;

    public QuoteClient()
    {
      socket = new ClientWebSocket();
      source = new CancellationTokenSource();
      symbols = new ConcurrentDictionary<string, bool>();
    }

    public async Task ConnectAsync()
    {
      if (socket.State != WebSocketState.Open)
      {
        await socket.ConnectAsync(new Uri(wsEndpoint), source.Token);
      }
    }

    public async Task DisconnectAsync()
    {
      await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "close", source.Token);
    }

    public async Task SubscribeAsync(string symbol)
    {
      if (symbols.TryAdd(symbol, true))
      {
        Subscription sub = new Subscription("subscribe", symbol.ToUpper());
        await SendSubscriptionAsync(sub);
      }
    }

    public async Task UnsubscribeAsync(string symbol)
    {
      if (symbols.TryRemove(symbol, out _))
      {
        Subscription sub = new Subscription("unsubscribe", symbol.ToUpper());
        await SendSubscriptionAsync(sub);
      }
    }

    public async Task ReceiveAsync(Action<Quote> callback, CancellationToken stoppingToken)
    {
      var buffer = new ArraySegment<byte>(new byte[1024]);
      WebSocketReceiveResult result;
      while (socket.State == WebSocketState.Open && !stoppingToken.IsCancellationRequested)
      {
        do
        {
          result = await socket.ReceiveAsync(buffer, stoppingToken);
        }
        while (!result.EndOfMessage);

        if (result.MessageType == WebSocketMessageType.Close) break;

        string message = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
        QuoteSocketResponse response = JsonSerializer.Deserialize<QuoteSocketResponse>(message);
        Quote quote = response.ToQuote();
        if (quote != null)
        {
          callback(quote);
        }
      }
    }

    private async Task SendSubscriptionAsync(Subscription sub)
    {
      await socket.SendAsync(
        Encoding.UTF8.GetBytes(JsonSerializer.Serialize(sub)),
        WebSocketMessageType.Text,
        true,
        source.Token
      );
    }
  }
}