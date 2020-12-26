using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace StockBE.Controllers
{
  [Route("api")]
  [ApiController]
  public class BrokerController : ControllerBase
  {
    // GET api/cash
    [HttpGet("cash")]
    public async Task<ActionResult<int>> GetCash()
    {
      await Task.Delay(TimeSpan.FromSeconds(1));
      return 10;
    }
  }
}
