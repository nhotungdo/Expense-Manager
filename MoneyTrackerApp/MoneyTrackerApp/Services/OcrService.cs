using MoneyTrackerApp.Models;
using MoneyTrackerApp.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace MoneyTrackerApp.Services;

/// <summary>
/// Service for OCR receipt scanning and text extraction
/// Extracts merchant name, amount, and date from receipt images
/// </summary>
public interface IOcrService
{
    Task<OcrResultDto> ProcessReceiptAsync(string imageBase64);
    Task<long> SaveOcrTextAsync(long transactionId, OcrResultDto ocrResult);
}

public class OcrService : IOcrService
{
    private readonly ExpenseManagerContext _context;

    public OcrService(ExpenseManagerContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Process receipt image and extract information
    /// Note: This is a simplified implementation. In production, use Azure Computer Vision, Google Cloud Vision, or Tesseract OCR
    /// </summary>
    public async Task<OcrResultDto> ProcessReceiptAsync(string imageBase64)
    {
        // TODO: Integrate with actual OCR service (Azure Computer Vision, Google Cloud Vision, etc.)
        // For now, return a mock result
        
        // In production, you would:
        // 1. Decode base64 image
        // 2. Send to OCR service
        // 3. Parse the OCR text
        // 4. Extract merchant, amount, date using regex patterns

        var mockRawText = @"
            SUPERMARKET ABC
            123 Main Street
            Date: 2025-11-27
            Time: 14:30
            
            Item 1          $10.50
            Item 2          $25.00
            Item 3          $5.99
            
            Subtotal:       $41.49
            Tax:            $3.32
            TOTAL:          $44.81
            
            Thank you!
        ";

        var result = new OcrResultDto
        {
            RawText = mockRawText,
            MerchantName = ExtractMerchantName(mockRawText),
            Amount = ExtractAmount(mockRawText),
            Date = ExtractDate(mockRawText),
            Confidence = 0.85m
        };

        return await Task.FromResult(result);
    }

    /// <summary>
    /// Save OCR text to database
    /// </summary>
    public async Task<long> SaveOcrTextAsync(long transactionId, OcrResultDto ocrResult)
    {
        var ocrText = new OcrText
        {
            TransactionId = transactionId,
            RawText = ocrResult.RawText,
            MerchantName = ocrResult.MerchantName,
            Amount = ocrResult.Amount,
            Date = ocrResult.Date,
            CreatedAt = DateTime.UtcNow
        };

        _context.OcrTexts.Add(ocrText);
        await _context.SaveChangesAsync();

        return ocrText.Id;
    }

    // Helper Methods for Text Extraction

    private string? ExtractMerchantName(string text)
    {
        // Look for merchant name in first few lines
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length > 0)
        {
            var firstLine = lines[0].Trim();
            if (!string.IsNullOrWhiteSpace(firstLine) && firstLine.Length > 3)
                return firstLine;
        }
        return null;
    }

    private decimal? ExtractAmount(string text)
    {
        // Look for TOTAL, Total, or similar patterns
        var patterns = new[]
        {
            @"TOTAL[:\s]*\$?([0-9,]+\.?[0-9]*)",
            @"Total[:\s]*\$?([0-9,]+\.?[0-9]*)",
            @"Amount[:\s]*\$?([0-9,]+\.?[0-9]*)",
            @"\$([0-9,]+\.[0-9]{2})"
        };

        foreach (var pattern in patterns)
        {
            var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
            if (match.Success && match.Groups.Count > 1)
            {
                var amountStr = match.Groups[1].Value.Replace(",", "");
                if (decimal.TryParse(amountStr, out var amount))
                    return amount;
            }
        }

        return null;
    }

    private DateTime? ExtractDate(string text)
    {
        // Look for date patterns
        var patterns = new[]
        {
            @"Date[:\s]*([0-9]{4}-[0-9]{2}-[0-9]{2})",
            @"Date[:\s]*([0-9]{2}/[0-9]{2}/[0-9]{4})",
            @"Date[:\s]*([0-9]{2}-[0-9]{2}-[0-9]{4})",
            @"([0-9]{4}-[0-9]{2}-[0-9]{2})",
            @"([0-9]{2}/[0-9]{2}/[0-9]{4})"
        };

        foreach (var pattern in patterns)
        {
            var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
            if (match.Success && match.Groups.Count > 1)
            {
                var dateStr = match.Groups[1].Value;
                if (DateTime.TryParse(dateStr, out var date))
                    return date;
            }
        }

        return null;
    }
}

/// <summary>
/// Extension methods for OCR integration
/// </summary>
public static class OcrExtensions
{
    /// <summary>
    /// Create transaction from OCR result
    /// </summary>
    public static CreateTransactionDto ToCreateTransactionDto(this OcrResultDto ocrResult, long accountId, long? categoryId = null)
    {
        return new CreateTransactionDto
        {
            AccountId = accountId,
            CategoryId = categoryId,
            TransactionType = 2, // Expense
            Amount = ocrResult.Amount ?? 0,
            Currency = "VND",
            Note = ocrResult.MerchantName,
            TransactionDate = ocrResult.Date ?? DateTime.UtcNow,
            OcrText = ocrResult.RawText
        };
    }
}
