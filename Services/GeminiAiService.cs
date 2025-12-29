using System.Text;
using System.Text.Json;

namespace Billbyte_BE.Services
{
    public class GeminiAiService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;

        public GeminiAiService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _apiKey = config["Gemini:ApiKey"]
                      ?? throw new Exception("Gemini API key missing");
        }

        public async Task<string> GenerateSqlAsync(string userText)
        {
            var url =
                $"https://generativelanguage.googleapis.com/v1beta/models/gemini-flash-lite-latest:generateContent?key={_apiKey}";

            var prompt = $@"
You generate PostgreSQL SQL for an EXISTING database created by EF Core.

VERY IMPORTANT RULES:
- Use EXACT table names WITH double quotes
- Table names are case-sensitive
- NEVER invent table names
- Generate ONLY ONE SELECT query
- NEVER use semicolon (;)
- ALWAYS add LIMIT 100

DATABASE TABLES (EXACT):

""CompletedOrders""(
  ""Id"", ""RestaurantId"", ""TableId"", ""OrderType"",
  ""Subtotal"", ""Tax"", ""Discount"", ""Total"",
  ""PaymentMode"", ""TableTimeMinutes"", ""CreatedDate""
)

""CompletedOrderItems""(
  ""Id"", ""CompletedOrderId"", ""ItemName"", ""Price"", ""Qty""
)

USER REQUEST:
{userText}
";

            var body = new
            {
                contents = new[]
                {
            new
            {
                parts = new[]
                {
                    new { text = prompt }
                }
            }
        }
            };

            var response = await _http.PostAsync(
                url,
                new StringContent(
                    JsonSerializer.Serialize(body),
                    Encoding.UTF8,
                    "application/json")
            );

            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Gemini Error: {response.StatusCode} - {responseText}");

            using var doc = JsonDocument.Parse(responseText);

            return doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString()!
                .Trim();
        }
    }
}
