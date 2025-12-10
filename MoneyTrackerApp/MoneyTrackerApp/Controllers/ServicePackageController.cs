using Microsoft.AspNetCore.Mvc;
using MoneyTrackerApp.DTOs;
using MoneyTrackerApp.Services;

namespace MoneyTrackerApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    public class ServicePackageController : ControllerBase
    {
        private readonly IServicePackageService _packageService;

        public ServicePackageController(IServicePackageService packageService)
        {
            _packageService = packageService;
        }

        [HttpGet]
        public async Task<ActionResult<List<ServicePackageDto>>> GetAllPackages([FromQuery] bool activeOnly = true)
        {
            var packages = activeOnly 
                ? await _packageService.GetActivePackagesAsync()
                : await _packageService.GetAllPackagesAsync();
            
            return Ok(packages);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ServicePackageDto>> GetPackage(int id)
        {
            var package = await _packageService.GetPackageByIdAsync(id);
            if (package == null)
                return NotFound(new { message = "Gói dịch vụ không tồn tại" });

            return Ok(package);
        }

        [HttpPost]
        public async Task<ActionResult<ServicePackageDto>> CreatePackage([FromBody] CreateServicePackageDto dto)
        {
            var package = await _packageService.CreatePackageAsync(dto);
            return CreatedAtAction(nameof(GetPackage), new { id = package.Id }, package);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ServicePackageDto>> UpdatePackage(int id, [FromBody] UpdateServicePackageDto dto)
        {
            var package = await _packageService.UpdatePackageAsync(id, dto);
            if (package == null)
                return NotFound(new { message = "Gói dịch vụ không tồn tại" });

            return Ok(package);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePackage(int id)
        {
            var result = await _packageService.DeletePackageAsync(id);
            if (!result)
                return NotFound(new { message = "Gói dịch vụ không tồn tại" });

            return NoContent();
        }

        [HttpPatch("{id}/toggle-status")]
        public async Task<ActionResult> ToggleStatus(int id)
        {
            var result = await _packageService.TogglePackageStatusAsync(id);
            if (!result)
                return NotFound(new { message = "Gói dịch vụ không tồn tại" });

            return Ok(new { message = "Cập nhật trạng thái thành công" });
        }
    }
}
