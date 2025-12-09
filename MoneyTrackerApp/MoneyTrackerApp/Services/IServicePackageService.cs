using MoneyTrackerApp.DTOs;

namespace MoneyTrackerApp.Services
{
    public interface IServicePackageService
    {
        Task<List<ServicePackageDto>> GetAllPackagesAsync();
        Task<List<ServicePackageDto>> GetActivePackagesAsync();
        Task<ServicePackageDto?> GetPackageByIdAsync(int id);
        Task<ServicePackageDto> CreatePackageAsync(CreateServicePackageDto dto);
        Task<ServicePackageDto?> UpdatePackageAsync(int id, UpdateServicePackageDto dto);
        Task<bool> DeletePackageAsync(int id);
        Task<bool> TogglePackageStatusAsync(int id);
    }
}
