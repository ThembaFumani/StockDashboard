using System;

using Microsoft.AspNetCore.Mvc;
using StockDashBoard.API.Services;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace StockDashBoard.API.Controllers
{
    [ApiController]
    [Route("api/stocks")]
    public class StockController : Controller
    {
        private readonly IStockPriceService _stockService;
        public StockController(IStockPriceService stockService)
        {
            _stockService = stockService ?? throw new ArgumentNullException(nameof(stockService));
        }

       [HttpGet("{symbol}")]
        public async Task<IActionResult> GetStockData(string symbol)
        {
            var stockResponse = await _stockService.GetStockData(symbol);
            return Ok(stockResponse);  // Returning the full API model
        }
    }
}

