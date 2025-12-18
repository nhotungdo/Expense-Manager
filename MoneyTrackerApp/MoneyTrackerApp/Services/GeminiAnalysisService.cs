using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MoneyTrackerApp.DTOs;

namespace MoneyTrackerApp.Services
{
    public class GeminiAnalysisService : IGeminiAnalysisService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public GeminiAnalysisService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task<string> AnalyzeTransactionsAsync(List<TransactionAnalysisDto> transactions)
        {
            var apiKey = _configuration["GeminiAI:ApiKey"];
            var model = _configuration["GeminiAI:Model"] ?? "gemini-1.5-flash"; 
            var baseUrl = _configuration["GeminiAI:BaseUrl"] ?? "https://generativelanguage.googleapis.com/v1beta"; // Not strictly needed if hardcoded but good for future.

            if (string.IsNullOrEmpty(apiKey))
            {
                return "API Key is missing. Please configure GeminiAI:ApiKey.";
            }

            try
            {
                var dataJson = JsonSerializer.Serialize(transactions);
                var prompt = $"Bạn là một chuyên gia tài chính cá nhân. Dưới đây là lịch sử giao dịch của tôi trong tháng qua dưới dạng JSON. Hãy phân tích và đưa ra:\n1. Nhận xét ngắn gọn về tình hình tài chính (Cân đối thu chi).\n2. Chỉ ra 1 thói quen chi tiêu cần điều chỉnh (nếu có).\n3. Một lời khuyên cụ thể để tiết kiệm tốt hơn.\n\nTrả lời bằng tiếng Việt, giọng văn thân thiện, ngắn gọn dưới 100 từ.\nDữ liệu: {dataJson}";

                var requestBody = new
                {
                    contents = new[]
                    {
                        new { parts = new[] { new { text = prompt } } }
                    }
                };

                var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                
                // Using configured model or default
                var response = await _httpClient.PostAsync($"{baseUrl}/models/{model}:generateContent?key={apiKey}", jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return $"AI Service Error: {response.StatusCode} - {errorContent}";
                }

                var responseString = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseString);
                
                if (doc.RootElement.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                {
                     var text = candidates[0]
                         .GetProperty("content")
                         .GetProperty("parts")[0]
                         .GetProperty("text").GetString();
                     return text;
                }
                
                return "No analysis returned from AI.";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}
