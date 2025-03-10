using System;
using System.Text.Json;
using StackExchange.Redis;
using StockDashBoard.API.Models;

namespace StockDashBoard.API.Repositories
{
	public class StockRepository : IStockRepository
	{
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<StockRepository> _logger;
        private readonly IConfiguration _config;
        private readonly IDatabase _redisCache;

        public StockRepository(IHttpClientFactory httpClientFactory, ILogger<StockRepository> logger, IConfiguration config, IConnectionMultiplexer redisConnection)
		{
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _redisCache = redisConnection.GetDatabase() ?? throw new ArgumentNullException(nameof(redisConnection));
        } 

        public async Task<StockResponse> GetStockData(string symbol)
        {
            var cachedData = await _redisCache.StringGetAsync(symbol);
            if (!cachedData.IsNullOrEmpty)
            {
                _logger.LogInformation("Cache hit for symbol {symbol}", symbol);
                return JsonSerializer.Deserialize<StockResponse>(cachedData);
            }

            _logger.LogInformation($"Cache miss for {symbol}. Fetching from API...");

            string apiKey = _config["AlphaVantage:ApiKey"];
            string baseUrl = _config["AlphaVantage:BaseUrl"];
        
            string url = $"{baseUrl}?function=TIME_SERIES_DAILY&symbol={symbol}&outputsize=compact&datatype=json";

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("X-RapidAPI-Key", apiKey);
            client.DefaultRequestHeaders.Add("X-RapidAPI-Host", "alpha-vantage.p.rapidapi.com");

            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Error fetching stock data: {response.ReasonPhrase}");

            var json = await response.Content.ReadAsStringAsync();
            var stockResponse = JsonSerializer.Deserialize<StockResponse>(json, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (stockResponse == null)
            {
                throw new Exception("Error deserializing stock data.");
            }

            await _redisCache.StringSetAsync(symbol, json, TimeSpan.FromMinutes(5));
            _logger.LogInformation($"Cached data for {symbol} in Redis.");

            return stockResponse;
        }
    }
}

