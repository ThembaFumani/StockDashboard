using System.Text.Json.Serialization;

namespace StockDashBoard.API.Models
{
    public class StockData
    {
        [JsonPropertyName("1. open")]
        public string? Open { get; set; }

        [JsonPropertyName("2. high")]
        public string? High { get; set; }

        [JsonPropertyName("3. low")]
        public string? Low { get; set; }

        [JsonPropertyName("4. close")]
        public string? Close { get; set; }

        [JsonPropertyName("5. volume")]
        public string? Volume { get; set; }
    }
}