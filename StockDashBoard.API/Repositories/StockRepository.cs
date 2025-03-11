using System;
using System.Text.Json;
using Azure.Security.KeyVault.Secrets;
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
        private readonly SecretClient _keyVaultClient;

        public StockRepository
        (IHttpClientFactory httpClientFactory, 
        ILogger<StockRepository> logger, 
        IConfiguration config, 
        IConnectionMultiplexer redisConnection,
        SecretClient keyVaultClient)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _redisCache = redisConnection.GetDatabase() ?? throw new ArgumentNullException(nameof(redisConnection));
            _keyVaultClient = keyVaultClient ?? throw new ArgumentNullException(nameof(keyVaultClient));
        }

        public async Task<StockResponse> GetStockData(string symbol)
        {
            try
            {
                var cachedData = await GetCachedDataAsync(symbol);
                if (cachedData != null)
                {
                    _logger.LogInformation("Cache hit for symbol {symbol}", symbol);
                    return cachedData;
                }

                await CheckApiRateLimitsAsync();

                _logger.LogInformation($"Cache miss for {symbol}. Fetching from API...");
                var stockResponse = await FetchStockDataFromApiAsync(symbol);

                await CacheStockDataAsync(symbol, stockResponse);

                return stockResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving stock data for {symbol}", symbol);
                throw new Exception("An error occurred while retrieving stock data. Please try again later.");
            }
        }

        private async Task<StockResponse?> GetCachedDataAsync(string symbol)
        {
            try
            {
                var cachedData = await _redisCache.StringGetAsync(symbol);
                if (!cachedData.IsNullOrEmpty)
                {
                    return JsonSerializer.Deserialize<StockResponse>(cachedData);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cache for symbol {symbol}", symbol);
            }
            return null;
        }

        private async Task CheckApiRateLimitsAsync()
        {
            try
            {
                string minuteKey = "stock:requests:minute";
                string dailyKey = "stock:requests:day";

                int minuteRequests = (int)await _redisCache.StringGetAsync(minuteKey);
                int dailyRequests = (int)await _redisCache.StringGetAsync(dailyKey);

                if (minuteRequests >= 5)
                {
                    _logger.LogWarning("API limit reached: 5 requests per minute exceeded.");
                    throw new Exception("API rate limit exceeded. Please wait a minute.");
                }
                if (dailyRequests >= 500)
                {
                    _logger.LogWarning("API limit reached: 500 requests per day exceeded.");
                    throw new Exception("Daily API limit exceeded. Try again tomorrow.");
                }

                await _redisCache.StringIncrementAsync(minuteKey);
                await _redisCache.StringIncrementAsync(dailyKey);

                if (minuteRequests == 0)
                    await _redisCache.KeyExpireAsync(minuteKey, TimeSpan.FromMinutes(1));
                if (dailyRequests == 0)
                    await _redisCache.KeyExpireAsync(dailyKey, TimeSpan.FromHours(24));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking API rate limits");
                throw new Exception("Error occurred while checking API rate limits.");
            }
        }

        private async Task<StockResponse> FetchStockDataFromApiAsync(string symbol)
        {
            try
            {
                string apiKey = await GetSecretFromKeyVault("AlphaVantageApiKey");
                string baseUrl = await GetSecretFromKeyVault("AlphaVantageBaseUrl");
                string url = $"{baseUrl}?function=TIME_SERIES_DAILY&symbol={symbol}&outputsize=compact&datatype=json";

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Add("X-RapidAPI-Key", apiKey);
                client.DefaultRequestHeaders.Add("X-RapidAPI-Host", "alpha-vantage.p.rapidapi.com");

                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Error fetching stock data: {status}", response.StatusCode);
                    throw new Exception($"Error fetching stock data: {response.ReasonPhrase}");
                }

                var json = await response.Content.ReadAsStringAsync();
                var stockResponse = JsonSerializer.Deserialize<StockResponse>(json, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                if (stockResponse == null)
                {
                    throw new Exception("Error deserializing stock data.");
                }

                return stockResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching stock data for {symbol}", symbol);
                throw new Exception("Error fetching stock data. Please try again later.");
            }
        }

        private async Task CacheStockDataAsync(string symbol, StockResponse stockResponse)
        {
            try
            {
                var json = JsonSerializer.Serialize(stockResponse, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                await _redisCache.StringSetAsync(symbol, json, TimeSpan.FromMinutes(5));
                _logger.LogInformation($"Cached data for {symbol} in Redis.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error caching stock data for {symbol}", symbol);
            }
        }

        private async Task<string> GetSecretFromKeyVault(string secretName)
        {
            try
            {
                var secret = await _keyVaultClient.GetSecretAsync(secretName);
                return secret.Value.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving secret {secretName} from Key Vault", secretName);
                throw new Exception("Error retrieving secrets from Key Vault.");
            }
        }
    }
}
