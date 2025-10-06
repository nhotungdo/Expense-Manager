using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MoneyTracker.Services;

namespace MoneyTracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocalizationController : ControllerBase
    {
        private readonly ILocalizationService _localizationService;
        private readonly ILogger<LocalizationController> _logger;

        public LocalizationController(ILocalizationService localizationService, ILogger<LocalizationController> logger)
        {
            _localizationService = localizationService;
            _logger = logger;
        }

        [HttpGet("languages")]
        public IActionResult GetSupportedLanguages()
        {
            try
            {
                var languages = _localizationService.GetSupportedLanguages();
                return Ok(languages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting supported languages");
                return StatusCode(500, "Error getting supported languages");
            }
        }

        [HttpGet("strings/{language}")]
        public IActionResult GetLocalizedStrings(string language = "vi")
        {
            try
            {
                var strings = _localizationService.GetLocalizedStrings(language);
                return Ok(strings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting localized strings for language {Language}", language);
                return StatusCode(500, "Error getting localized strings");
            }
        }

        [HttpGet("string/{key}")]
        public IActionResult GetString(string key, [FromQuery] string language = "vi")
        {
            try
            {
                var value = _localizationService.GetString(key, language);
                return Ok(new { key, value, language });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting string for key {Key} and language {Language}", key, language);
                return StatusCode(500, "Error getting string");
            }
        }

        [HttpPost("set-language")]
        [Authorize]
        public async Task<IActionResult> SetUserLanguage([FromBody] SetLanguageDto languageDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                await _localizationService.SetUserLanguageAsync(userId.Value, languageDto.Language);
                return Ok(new { message = "Language updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting user language");
                return StatusCode(500, "Error setting user language");
            }
        }

        [HttpGet("user-language")]
        [Authorize]
        public IActionResult GetUserLanguage()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var language = _localizationService.GetUserLanguage(userId.Value);
                return Ok(new { language });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user language");
                return StatusCode(500, "Error getting user language");
            }
        }

        private long? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return long.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }

    public class SetLanguageDto
    {
        public string Language { get; set; } = string.Empty;
    }
}
