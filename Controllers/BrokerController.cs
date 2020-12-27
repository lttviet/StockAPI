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

    public BrokerController(BrokerDataAccess brokerDataAccess)
    {
      this.brokerDataAccess = brokerDataAccess;
    }

    [HttpGet("portfolio/{id}/cash")]
    public async Task<ActionResult<double>> GetCash(string id)
    {
      double? cash = await this.brokerDataAccess.GetCashAsync(id);
      if (cash is null)
      {
        return NotFound();
      }
      return cash;
    }

    [HttpPut("portfolio/{id}/cash")]
    public async Task<IActionResult> UpdateCash(string id, Cash cash)
    {
      await this.brokerDataAccess.UpdateCashAsync(id, cash.cash);
      return NoContent();
    }
  }
}
