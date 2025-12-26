using System.Text;
using System.Text.Json;
using MoneyTrackerApp.DTOs;

namespace MoneyTrackerApp.Services
{
    public class GeminiAnalysisService : IGeminiAnalysisService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public GeminiAnalysisService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> AnalyzeTransactionsAsync(List<TransactionAnalysisDto> transactions)
        {
            var apiKey = _configuration["GeminiAI:ApiKey"];
            var model = _configuration["GeminiAI:Model"] ?? "gemini-1.5-flash";
            var baseUrl = _configuration["GeminiAI:BaseUrl"] ?? "https://generativelanguage.googleapis.com/v1beta";

            if (string.IsNullOrEmpty(apiKey)) return "⚠️ Chưa cấu hình API Key cho Gemini AI.";

            // Optimize payload size
            var summaryData = transactions.Take(50).Select(t => new {
                d = t.Date,
                a = t.Amount,
                c = t.Category,
                t = t.Type,
                n = t.Note
            });

            var prompt = "Bạn là chuyên gia tài chính cá nhân. Hãy phân tích danh sách giao dịch (d:date, a:amount, c:category, t:type, n:note) dưới đây:\n" +
                         "1. Đưa ra nhận xét tổng quan về tình hình thu chi.\n" +
                         "2. Chỉ ra 1 điểm cần cải thiện hoặc rủi ro tiềm ẩn.\n" +
                         "3. Đưa ra 1 lời khuyên ngắn gọn, thiết thực.\n" +
                         "Trả lời bằng Tiếng Việt, sử dụng Markdown (bold các ý chính), giọng văn chuyên nghiệp nhưng thân thiện.\n" +
                         $"Dữ liệu: {JsonSerializer.Serialize(summaryData)}";

            var requestBody = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = prompt } } }
                }
            };

            try
            {
                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                
                // Add explicit timeout
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
                
                var response = await _httpClient.PostAsync($"{baseUrl}/models/{model}:generateContent?key={apiKey}", content, cts.Token);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync(cts.Token);
                    using var doc = JsonDocument.Parse(json);
                    try {
                        var text = doc.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
                        return text ?? "AI không trả về kết quả.";
                    } catch { return "Không thể đọc phản hồi từ AI."; }
                }
                else
                {
                    return $"Lỗi kết nối AI: {response.StatusCode}. Vui lòng thử lại sau.";
                }
            }
            catch (TaskCanceledException)
            {
                return "Hệ thống AI đang bận, vui lòng thử lại sau giây lát.";
            }
            catch (Exception ex)
            {
                return $"Lỗi hệ thống: {ex.Message}";
            }
        }
    }
}
