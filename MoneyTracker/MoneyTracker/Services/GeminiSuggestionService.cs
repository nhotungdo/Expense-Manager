using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;
using System.Text;
using System.Text.Json;

namespace MoneyTracker.Services
{
    public class GeminiSuggestionService : IGeminiSuggestionService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GeminiSuggestionService> _logger;
        private readonly string _apiKey = "AIzaSyD8qIcTRV9H02_UWYq7NHjllr-VkqpQN4U";
        private readonly string _apiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent";

        public GeminiSuggestionService(HttpClient httpClient, ILogger<GeminiSuggestionService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<string> GetFinancialSuggestionAsync(IEnumerable<Transaction> recentTransactions)
        {
            try
            {
                // Format transactions for the prompt
                var formattedTransactions = FormatTransactionsForPrompt(recentTransactions);

                // Create the prompt in Vietnamese
                var prompt = $@"Dựa trên danh sách các giao dịch gần đây của tôi, hãy đóng vai một chuyên gia tài chính và đưa ra 3 gợi ý ngắn gọn, hữu ích để tôi có thể cải thiện tình hình chi tiêu và tiết kiệm hiệu quả hơn.

Đây là dữ liệu giao dịch:
{formattedTransactions}

Hãy trình bày kết quả dưới dạng một danh sách có gạch đầu dòng.";

                // Create the request body for Gemini API
                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new
                                {
                                    text = prompt
                                }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = 0.7,
                        topK = 40,
                        topP = 0.95,
                        maxOutputTokens = 1024
                    }
                };

                // Serialize to JSON
                var jsonContent = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Make the API request
                var response = await _httpClient.PostAsync($"{_apiUrl}?key={_apiKey}", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseContent);

                    if (geminiResponse?.Candidates?.Length > 0 &&
                        geminiResponse.Candidates[0].Content?.Parts?.Length > 0)
                    {
                        var suggestion = geminiResponse.Candidates[0].Content.Parts[0].Text;
                        _logger.LogInformation("Successfully generated AI suggestion");
                        return suggestion ?? "Không thể tạo gợi ý từ AI.";
                    }
                }

                _logger.LogError("Failed to get response from Gemini API. Status: {StatusCode}", response.StatusCode);
                return "Xin lỗi, tôi không thể tạo gợi ý tài chính vào lúc này. Vui lòng thử lại sau.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Gemini API");
                return "Xin lỗi, đã xảy ra lỗi khi tạo gợi ý tài chính. Vui lòng thử lại sau.";
            }
        }

        private string FormatTransactionsForPrompt(IEnumerable<Transaction> transactions)
        {
            if (!transactions.Any())
            {
                return "Không có giao dịch nào trong thời gian gần đây.";
            }

            var formatted = new StringBuilder();

            foreach (var transaction in transactions.OrderByDescending(t => t.TransactionDate))
            {
                var type = transaction.Type.ToLower() == "income" ? "Thu" : "Chi";
                var amount = transaction.Amount.ToString("N0");
                var date = transaction.TransactionDate.ToString("yyyy-MM-dd");
                var note = !string.IsNullOrEmpty(transaction.Note) ? $" Ghi chú: {transaction.Note}" : "";

                formatted.AppendLine($"- {type} {amount} {transaction.Currency} vào ngày {date}.{note}");
            }

            return formatted.ToString();
        }
    }

    // Response models for Gemini API
    public class GeminiResponse
    {
        public Candidate[]? Candidates { get; set; }
    }

    public class Candidate
    {
        public Content? Content { get; set; }
    }

    public class Content
    {
        public Part[]? Parts { get; set; }
    }

    public class Part
    {
        public string? Text { get; set; }
    }
}
