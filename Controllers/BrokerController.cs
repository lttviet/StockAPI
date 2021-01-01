using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using StockBE.DataAccess;

namespace StockBE.Controllers
{
  [Route("api")]
  [ApiController]
  public class BrokerController : ControllerBase
  {
    private readonly BrokerDataAccess brokerDB;
    private readonly QuoteClient quoteClient;

    public BrokerController(BrokerDataAccess brokerDB, QuoteClient quoteClient)
    {
      this.brokerDB = brokerDB;
      this.quoteClient = quoteClient;
    }

    [HttpGet("portfolio/{id}/cash")]
    public async Task<ActionResult<double>> GetCash(string id)
    {
      double? cash = await this.brokerDB.GetCashAsync(id);
      if (cash is null)
      {
        return NotFound();
      }
      return cash;
    }

    [HttpPost("portfolio/{id}/stocks/buy")]
    public async Task<ActionResult> BuyStock(string id, Order order)
    {
      // TODO refactor to use Order in brokerDB
      // bool resultCode = await brokerDB.BuyStock(id, order.symbol, order.price, order.quantity);
      bool resultCode = true;
      await Task.Delay(TimeSpan.FromSeconds(1));
      if (resultCode)
      {
        return NoContent();
      }
      return StatusCode(500);
    }

    [HttpPost("portfolio/{id}/stocks/sell")]
    public async Task<ActionResult> SellStock(string id, Order order)
    {
      // bool resultCode = await brokerDB.SellStock(portfolioId, order.symbol, order.price, order.quantity);
      bool resultCode = true;
      await Task.Delay(TimeSpan.FromSeconds(1));
      if (resultCode)
      {
        return NoContent();
      }
      return StatusCode(500);
    }

  }
}
