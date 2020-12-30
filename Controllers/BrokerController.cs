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
    private readonly BrokerDataAccess brokerDataAccess;
    private readonly QuoteClient quoteClient;

    public BrokerController(BrokerDataAccess brokerDataAccess, QuoteClient quoteClient)
    {
      this.brokerDataAccess = brokerDataAccess;
      this.quoteClient = quoteClient;
    }

    [HttpGet("portfolio/{id}/cash")]
    public async Task<ActionResult<double>> GetCash(string id)
    {
      await quoteClient.SubscribeAsync(id);
      double? cash = await this.brokerDataAccess.GetCashAsync(id);
      if (cash is null)
      {
        return NotFound();
      }
      return cash;
    }
  }
}
