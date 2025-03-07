using System.Text.Json.Serialization;
namespace StockDashBoard.API.Models
{
    public class StockResponse
    {
        [JsonPropertyName("Meta Data")]
        public StockMetaData? MetaData { get; set; }

        [JsonPropertyName("Time Series (Daily)")]
        public Dictionary<string, StockData>? TimeSeries { get; set; }
    }
}