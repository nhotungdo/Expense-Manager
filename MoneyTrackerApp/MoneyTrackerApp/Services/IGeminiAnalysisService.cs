using System.Collections.Generic;
using System.Threading.Tasks;
using MoneyTrackerApp.DTOs;

namespace MoneyTrackerApp.Services
{
    public interface IGeminiAnalysisService
    {
        Task<string> AnalyzeTransactionsAsync(List<TransactionAnalysisDto> transactions);
    }
}
