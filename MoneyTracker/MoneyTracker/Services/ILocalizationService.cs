namespace MoneyTracker.Services
{
    public interface ILocalizationService
    {
        string GetString(string key, string language = "vi");
        Dictionary<string, string> GetLocalizedStrings(string language = "vi");
        List<LanguageDto> GetSupportedLanguages();
        Task SetUserLanguageAsync(long userId, string language);
        string GetUserLanguage(long userId);
    }

    public class LanguageDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string NativeName { get; set; } = string.Empty;
        public string Flag { get; set; } = string.Empty;
    }
}
