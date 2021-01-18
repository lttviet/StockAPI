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

    [HttpPost("portfolio/{id}/stocks/buy")]
    public async Task<ActionResult> BuyStock(string id, Order order)
    {
      if (await brokerDB.BuyStock(id, order))
      {
        return NoContent();
      }
      return StatusCode(500);
    }

    [HttpPost("portfolio/{id}/stocks/sell")]
    public async Task<ActionResult> SellStock(string id, Order order)
    {
      if (await brokerDB.SellStock(id, order))
      {
        return NoContent();
      }
      return StatusCode(500);
    }

  }
}
