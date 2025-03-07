using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StockDashBoard.API.Models;
using StockDashBoard.API.Repositories;
namespace StockDashBoard.API.Services
{ 
    public class StockPriceService : IStockPriceService
    {
        private readonly IStockRepository _stockRepository;
        public StockPriceService(IStockRepository stockRepository)
        {
            _stockRepository = stockRepository ?? throw new ArgumentNullException(nameof(stockRepository)); 
        }

        public async Task<StockResponse> GetStockData(string symbol)
        {
            var response = await _stockRepository.GetStockData(symbol);

            return response;
        }
    }
}