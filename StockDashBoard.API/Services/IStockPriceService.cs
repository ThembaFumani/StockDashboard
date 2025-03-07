

using StockDashBoard.API.Models;

namespace StockDashBoard.API.Services
{
    public interface IStockPriceService
    {
        Task<StockResponse> GetStockData(string symbol);
    }
}