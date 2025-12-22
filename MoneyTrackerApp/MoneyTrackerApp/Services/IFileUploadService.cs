namespace MoneyTrackerApp.Services;

public interface IFileUploadService
{
    Task<(bool Success, string? FilePath, string? ThumbnailPath, string? ErrorMessage)> UploadFileAsync(
        IFormFile file, 
        string uploadType, 
        long userId);
    
    Task<bool> DeleteFileAsync(string filePath);
    
    Task<(bool Success, string? ThumbnailPath)> GenerateThumbnailAsync(string imagePath);
    
    bool IsValidFileType(string fileName, string uploadType);
    
    bool IsValidFileSize(long fileSize, string uploadType);
    
    string GetFileExtension(string fileName);
    
    string GetMimeType(string fileName);
}
