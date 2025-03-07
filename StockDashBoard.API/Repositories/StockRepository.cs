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
        private readonly string _apiKey;
        private readonly string _apiHost;

        public StockRepository(IHttpClientFactory httpClientFactory, ILogger<StockRepository> logger, IConfiguration config)
		{
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        } 

        public async Task<StockResponse> GetStockData(string symbol)
        {
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

        return stockResponse;
        }
    }
}

