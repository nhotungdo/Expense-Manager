using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoneyTracker.Services
{
    public sealed class AiTransactionInput
    {
        public DateTime TransactionDate => Date;
        public DateTime Date { get; set; }
        public long? CategoryId { get; set; }
        public decimal Amount { get; set; }
    }

    public interface IAiService
    {
        Task<List<string>> GetSuggestionsAsync(IEnumerable<AiTransactionInput> transactions);
    }
}

