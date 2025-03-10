
using Azure.Security.KeyVault.Secrets;

namespace StockDashBoard.API.Middleware
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string APIKEYNAME = "ApiKey";
        private readonly ILogger<ApiKeyMiddleware> _logger;
        private readonly SecretClient _keyVaultClient;
        public ApiKeyMiddleware(RequestDelegate next, ILogger<ApiKeyMiddleware> logger, SecretClient keyVaultClient)
        {
            _next = next;
            _logger = logger;
            _keyVaultClient = keyVaultClient;
        }

        public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
        {
            if (!context.Request.Headers.TryGetValue(APIKEYNAME, out var extractedApiKey))
            {
                _logger.LogWarning("API Key was not provided.");
                context.Response.StatusCode = 401; // Unauthorized
                await context.Response.WriteAsync("API Key was not provided.");
                return;
            }

            var secret = await _keyVaultClient.GetSecretAsync(APIKEYNAME);
            var apiKeyFromVault = secret.Value.Value;
            
            if (!apiKeyFromVault.Equals(extractedApiKey))
            {
                _logger.LogWarning("Unauthorized client.");
                context.Response.StatusCode = 401; // Unauthorized
                await context.Response.WriteAsync("Unauthorized client.");
                return;
            }

            await _next(context);
        }
    }
}