using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace MoneyTrackerApp.Services;

public class FileUploadService : IFileUploadService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<FileUploadService> _logger;

    // File size limits (in bytes)
    private const long MaxImageSize = 10 * 1024 * 1024; // 10MB
    private const long MaxVideoSize = 50 * 1024 * 1024; // 50MB
    private const long MaxFileSize = 25 * 1024 * 1024; // 25MB

    // Allowed file extensions
    private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };
    private static readonly string[] AllowedVideoExtensions = { ".mp4", ".webm", ".mov", ".avi" };
    private static readonly string[] AllowedAudioExtensions = { ".mp3", ".wav", ".ogg", ".m4a" };
    private static readonly string[] AllowedDocumentExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".zip", ".rar" };

    public FileUploadService(IWebHostEnvironment environment, ILogger<FileUploadService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async Task<(bool Success, string? FilePath, string? ThumbnailPath, string? ErrorMessage)> UploadFileAsync(
        IFormFile file,
        string uploadType,
        long userId)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return (false, null, null, "No file provided");
            }

            // Validate file type
            if (!IsValidFileType(file.FileName, uploadType))
            {
                return (false, null, null, "Invalid file type");
            }

            // Validate file size
            if (!IsValidFileSize(file.Length, uploadType))
            {
                return (false, null, null, "File size exceeds limit");
            }

            // Generate unique filename
            var extension = GetFileExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{extension}";
            var uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", uploadType, userId.ToString());

            // Create directory if it doesn't exist
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            var filePath = Path.Combine(uploadFolder, fileName);
            var relativeFilePath = $"/uploads/{uploadType}/{userId}/{fileName}";

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            _logger.LogInformation("File uploaded successfully: {FilePath}", relativeFilePath);

            // Generate thumbnail for images
            string? thumbnailPath = null;
            if (uploadType == "images" && IsImageFile(extension))
            {
                var (success, thumbPath) = await GenerateThumbnailAsync(filePath);
                if (success && thumbPath != null)
                {
                    thumbnailPath = thumbPath.Replace(_environment.WebRootPath, "").Replace("\\", "/");
                }
            }

            return (true, relativeFilePath, thumbnailPath, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            return (false, null, null, "Error uploading file");
        }
    }

    public async Task<bool> DeleteFileAsync(string filePath)
    {
        try
        {
            var fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));
            
            if (File.Exists(fullPath))
            {
                await Task.Run(() => File.Delete(fullPath));
                _logger.LogInformation("File deleted: {FilePath}", filePath);
                
                // Delete thumbnail if exists
                var thumbnailPath = fullPath.Replace(".", "_thumb.");
                if (File.Exists(thumbnailPath))
                {
                    await Task.Run(() => File.Delete(thumbnailPath));
                }
                
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
            return false;
        }
    }

    public async Task<(bool Success, string? ThumbnailPath)> GenerateThumbnailAsync(string imagePath)
    {
        try
        {
            var thumbnailPath = imagePath.Replace(".", "_thumb.");
            
            using (var image = await Image.LoadAsync(imagePath))
            {
                // Resize to max 300x300 while maintaining aspect ratio
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(300, 300),
                    Mode = ResizeMode.Max
                }));

                await image.SaveAsJpegAsync(thumbnailPath, new JpegEncoder { Quality = 80 });
            }

            _logger.LogInformation("Thumbnail generated: {ThumbnailPath}", thumbnailPath);
            return (true, thumbnailPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating thumbnail for: {ImagePath}", imagePath);
            return (false, null);
        }
    }

    public bool IsValidFileType(string fileName, string uploadType)
    {
        var extension = GetFileExtension(fileName).ToLowerInvariant();

        return uploadType.ToLower() switch
        {
            "images" => AllowedImageExtensions.Contains(extension),
            "videos" => AllowedVideoExtensions.Contains(extension),
            "audio" => AllowedAudioExtensions.Contains(extension),
            "files" => AllowedDocumentExtensions.Contains(extension) ||
                      AllowedImageExtensions.Contains(extension) ||
                      AllowedVideoExtensions.Contains(extension) ||
                      AllowedAudioExtensions.Contains(extension),
            _ => false
        };
    }

    public bool IsValidFileSize(long fileSize, string uploadType)
    {
        return uploadType.ToLower() switch
        {
            "images" => fileSize <= MaxImageSize,
            "videos" => fileSize <= MaxVideoSize,
            "audio" => fileSize <= MaxFileSize,
            "files" => fileSize <= MaxFileSize,
            _ => false
        };
    }

    public string GetFileExtension(string fileName)
    {
        return Path.GetExtension(fileName).ToLowerInvariant();
    }

    public string GetMimeType(string fileName)
    {
        var extension = GetFileExtension(fileName);

        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".bmp" => "image/bmp",
            ".mp4" => "video/mp4",
            ".webm" => "video/webm",
            ".mov" => "video/quicktime",
            ".avi" => "video/x-msvideo",
            ".mp3" => "audio/mpeg",
            ".wav" => "audio/wav",
            ".ogg" => "audio/ogg",
            ".m4a" => "audio/mp4",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".ppt" => "application/vnd.ms-powerpoint",
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            ".txt" => "text/plain",
            ".zip" => "application/zip",
            ".rar" => "application/x-rar-compressed",
            _ => "application/octet-stream"
        };
    }

    private bool IsImageFile(string extension)
    {
        return AllowedImageExtensions.Contains(extension.ToLowerInvariant());
    }
}
