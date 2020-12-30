using System;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;

namespace StockBE.DataAccess
{
  public class QuoteClient
  {
    private readonly string endpoint = "ws://echo.websocket.org";
    private readonly ClientWebSocket socket;
    private readonly CancellationTokenSource source;

    public QuoteClient()
    {
      socket = new ClientWebSocket();
      source = new CancellationTokenSource();
    }

    public async Task ConnectAsync()
    {
      if (socket.State != WebSocketState.Open)
      {
        await socket.ConnectAsync(new Uri(endpoint), source.Token);
      }
    }

    public async Task DisconnectAsync()
    {
      await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "close", source.Token);
    }

    public async Task SubscribeAsync(string symbol)
    {
      Subscription sub = new Subscription("subscribe", symbol.ToUpper());
      await SendSubscriptionAsync(sub);
    }

    public async Task UnsubscribeAsync(string symbol)
    {
      Subscription sub = new Subscription("unsubscribe", symbol.ToUpper());
      await SendSubscriptionAsync(sub);
    }

    public async Task ReceiveAsync(Action<string> callback)
    {
      var buffer = new ArraySegment<byte>(new byte[1024]);
      WebSocketReceiveResult result;
      while (socket.State == WebSocketState.Open && !source.Token.IsCancellationRequested)
      {
        do
        {
          result = await socket.ReceiveAsync(buffer, source.Token);
        }
        while (!result.EndOfMessage);

        if (result.MessageType == WebSocketMessageType.Close) break;
        callback(Encoding.UTF8.GetString(buffer.Array, 0, result.Count));
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