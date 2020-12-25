using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using Microsoft.Extensions.Hosting;

namespace StockBE.Services
{
  public class QuoteService : BackgroundService
  {
    private readonly string endpoint = "ws://echo.websocket.org";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      while (!stoppingToken.IsCancellationRequested)
      {
        using (ClientWebSocket ws = new ClientWebSocket())
        {
          await ws.ConnectAsync(new Uri(endpoint), stoppingToken);

          await SubscribeAsync(ws, new Subscription("AAPL"), stoppingToken);
          await SubscribeAsync(ws, new Subscription("MSFT"), stoppingToken);

          await ReceiveAsync(ws, stoppingToken);
        }
      }
    }

    private async Task SubscribeAsync(ClientWebSocket socket, Subscription subscription, CancellationToken stoppingToken)
    {
      await socket.SendAsync(
        Encoding.UTF8.GetBytes(JsonSerializer.Serialize(subscription)),
        WebSocketMessageType.Text,
        true,
        stoppingToken
      );
    }

    private async Task ReceiveAsync(ClientWebSocket socket, CancellationToken stoppingToken)
    {
      var buffer = new ArraySegment<byte>(new byte[1024]);
      WebSocketReceiveResult result;

      while (!stoppingToken.IsCancellationRequested)
      {
        do
        {
          result = await socket.ReceiveAsync(buffer, stoppingToken);
        }
        while (!result.EndOfMessage);

        if (result.MessageType == WebSocketMessageType.Close) break;

        Console.WriteLine(
          "{0}",
          Encoding.UTF8.GetString(buffer.Array, 0, result.Count)
        );
      }
    }
  }
}