
using StockDashBoard.API.Models;

namespace StockDashBoard.API.Repositories
{
	public interface IStockRepository
	{
		Task<StockResponse> GetStockData(string symbol);
	}
}

