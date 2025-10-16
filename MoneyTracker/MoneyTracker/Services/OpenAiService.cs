using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace MoneyTracker.Services
{
    public class OpenAiService : IAiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public OpenAiService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<List<string>> GetSuggestionsAsync(IEnumerable<AiTransactionInput> transactions)
        {
            var apiKey = _configuration["AI:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                // Fallback suggestions when API key missing
                return new List<string>
                {
                    "Theo dõi các khoản chi nhỏ lẻ để cắt giảm.",
                    "Ưu tiên nhu yếu phẩm và hạn chế mua sắm cảm xúc.",
                    "Đặt ngân sách cho từng danh mục chi tiêu."
                };
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

            var model = _configuration["AI:Model"] ?? "gpt-4o-mini";
            var txLines = transactions.Take(30)
                .Select(t => $"{t.Date:yyyy-MM-dd} - {(t.CategoryId?.ToString() ?? "N/A")} - {t.Amount:N0}");

            var prompt = "Bạn là trợ lý chi tiêu thông minh.\n" +
                         "Dưới đây là danh sách các giao dịch 30 ngày gần đây của người dùng:\n" +
                         string.Join("\n", txLines) +
                         "\nHãy cho 3 lời khuyên ngắn gọn để giúp người dùng tiết kiệm và cân đối chi tiêu.";

            var payload = new
            {
                model,
                messages = new[]
                {
                    new { role = "system", content = "Bạn là trợ lý tài chính cá nhân." },
                    new { role = "user", content = prompt }
                }
            };

            var response = await client.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", payload);
            if (!response.IsSuccessStatusCode)
            {
                return new List<string> { "Không thể lấy gợi ý AI lúc này." };
            }

            using var stream = await response.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);
            var content = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? string.Empty;

            var tips = content.Split('\n')
                .Select(x => x.TrimStart('-', ' ', '*'))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Take(5)
                .ToList();
            return tips.Count > 0 ? tips : new List<string> { content };
        }
    }
}

