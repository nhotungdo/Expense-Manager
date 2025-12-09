using Microsoft.EntityFrameworkCore;
using MoneyTrackerApp.DTOs;
using MoneyTrackerApp.Models;
using System.Text.Json;

namespace MoneyTrackerApp.Services
{
    public class ServicePackageService : IServicePackageService
    {
        private readonly ExpenseManagerContext _context;

        public ServicePackageService(ExpenseManagerContext context)
        {
            _context = context;
        }

        public async Task<List<ServicePackageDto>> GetAllPackagesAsync()
        {
            var packages = await _context.ServicePackages
                .OrderBy(p => p.DisplayOrder)
                .ToListAsync();

            return packages.Select(MapToDto).ToList();
        }

        public async Task<List<ServicePackageDto>> GetActivePackagesAsync()
        {
            var packages = await _context.ServicePackages
                .Where(p => p.IsActive)
                .OrderBy(p => p.DisplayOrder)
                .ToListAsync();

            return packages.Select(MapToDto).ToList();
        }

        public async Task<ServicePackageDto?> GetPackageByIdAsync(int id)
        {
            var package = await _context.ServicePackages.FindAsync(id);
            return package != null ? MapToDto(package) : null;
        }

        public async Task<ServicePackageDto> CreatePackageAsync(CreateServicePackageDto dto)
        {
            var package = new ServicePackage
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                OriginalPrice = dto.OriginalPrice,
                DurationDays = dto.DurationDays,
                Features = JsonSerializer.Serialize(dto.Features),
                IsPopular = dto.IsPopular,
                BadgeText = dto.BadgeText,
                BadgeColor = dto.BadgeColor,
                DisplayOrder = dto.DisplayOrder,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.ServicePackages.Add(package);
            await _context.SaveChangesAsync();

            return MapToDto(package);
        }

        public async Task<ServicePackageDto?> UpdatePackageAsync(int id, UpdateServicePackageDto dto)
        {
            var package = await _context.ServicePackages.FindAsync(id);
            if (package == null) return null;

            package.Name = dto.Name;
            package.Description = dto.Description;
            package.Price = dto.Price;
            package.OriginalPrice = dto.OriginalPrice;
            package.DurationDays = dto.DurationDays;
            package.Features = JsonSerializer.Serialize(dto.Features);
            package.IsPopular = dto.IsPopular;
            package.BadgeText = dto.BadgeText;
            package.BadgeColor = dto.BadgeColor;
            package.DisplayOrder = dto.DisplayOrder;
            package.IsActive = dto.IsActive;
            package.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToDto(package);
        }

        public async Task<bool> DeletePackageAsync(int id)
        {
            var package = await _context.ServicePackages.FindAsync(id);
            if (package == null) return false;

            _context.ServicePackages.Remove(package);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> TogglePackageStatusAsync(int id)
        {
            var package = await _context.ServicePackages.FindAsync(id);
            if (package == null) return false;

            package.IsActive = !package.IsActive;
            package.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        private ServicePackageDto MapToDto(ServicePackage package)
        {
            var features = new List<string>();
            try
            {
                features = JsonSerializer.Deserialize<List<string>>(package.Features) ?? new List<string>();
            }
            catch { }

            var discountPercentage = 0;
            if (package.OriginalPrice.HasValue && package.OriginalPrice > package.Price)
            {
                discountPercentage = (int)Math.Round(((package.OriginalPrice.Value - package.Price) / package.OriginalPrice.Value) * 100);
            }

            return new ServicePackageDto
            {
                Id = package.Id,
                Name = package.Name,
                Description = package.Description,
                Price = package.Price,
                OriginalPrice = package.OriginalPrice,
                DurationDays = package.DurationDays,
                Features = features,
                IsPopular = package.IsPopular,
                BadgeText = package.BadgeText,
                BadgeColor = package.BadgeColor,
                DiscountPercentage = discountPercentage
            };
        }
    }
}
