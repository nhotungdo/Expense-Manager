using System.Threading.Tasks;
using System.Collections.Generic;
using MoneyTrackerApp.Models;

namespace MoneyTrackerApp.Services
{
    public interface IAiAdvisorService
    {
        Task<AiSuggestion> GenerateAdviceAsync(long userId);
        Task<IEnumerable<AiSuggestion>> GetHistoryAsync(long userId);
    }
}
