namespace GoAir.Services.Core.Services
{
    using Common;
    using Contracts;
    using Data;
    using Data.Models;
    using Web.ViewModels.Aircraft;

    using Microsoft.EntityFrameworkCore;

    public class AircraftService(ApplicationDbContext context) : IAircraftService
    {
        public async Task<AircraftIndexViewModel> GetAllAsync(string? searchTerm, int page, bool isAdmin)
        {
            const int PageSize = 6;
            IQueryable<Aircraft> query = context.Aircraft
            .AsNoTracking()
            .AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string normalizedSearch = searchTerm.Trim();
                query = query.Where(a =>
                a.Manufacturer.Contains(normalizedSearch) ||
                a.Model.Contains(normalizedSearch));
            }
            query = query
            .OrderBy(a => a.Manufacturer)
            .ThenBy(a => a.Model);
            int totalAircraft = await query.CountAsync();
            int totalPages = Math.Max(1, (int)Math.Ceiling(totalAircraft / (double)PageSize));
            int currentPage = Math.Clamp(page, 1, totalPages);
            List<AircraftViewModel> aircraft = await query
            .Skip((currentPage - 1) * PageSize)
            .Take(PageSize)
            .Select(a => new AircraftViewModel
            {
                Id = a.Id,
                Model = a.Model,
                Manufacturer = a.Manufacturer,
                Capacity = a.Capacity,
            })
            .ToListAsync();
            return new AircraftIndexViewModel
            {
                SearchTerm = searchTerm?.Trim() ?? string.Empty,
                CurrentPage = currentPage,
                TotalPages = totalPages,
                IsAdmin = isAdmin,
                Aircraft = aircraft,
            };
        }

        public async Task<AircraftViewModel?> GetByIdAsync(Guid id)
        {
            Aircraft? aircraft = await context.Aircraft
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);

            return aircraft == null ? null : Map(aircraft);
        }

        public async Task<ServiceResult> CreateAsync(AircraftViewModel model)
        {
            ServiceResult validation = await ValidateAsync(model);
            if (!validation.Succeeded)
                return validation;

            Aircraft aircraft = new()
            {
                Id = Guid.NewGuid(),
                Model = model.Model,
                Manufacturer = model.Manufacturer,
                Capacity = model.Capacity,
            };

            context.Aircraft.Add(aircraft);
            await context.SaveChangesAsync();
            return ServiceResult.Success();
        }

        public async Task<AircraftViewModel?> GetForEditAsync(Guid id)
        {
            Aircraft? aircraft = await context.Aircraft.FindAsync(id);
            return aircraft == null ? null : Map(aircraft);
        }

        public async Task<ServiceResult> UpdateAsync(AircraftViewModel model)
        {
            bool exists = await context.Aircraft.AnyAsync(a => a.Id == model.Id);
            if (!exists)
                return ServiceResult.Missing();

            ServiceResult validation = await ValidateAsync(model);
            if (!validation.Succeeded)
                return validation;

            Aircraft aircraft = new()
            {
                Id = model.Id,
                Model = model.Model,
                Manufacturer = model.Manufacturer,
                Capacity = model.Capacity,
            };

            context.Aircraft.Update(aircraft);
            await context.SaveChangesAsync();
            return ServiceResult.Success();
        }

        public async Task<ServiceResult> DeleteAsync(Guid id)
        {
            Aircraft? aircraft = await context.Aircraft.FindAsync(id);
            if (aircraft == null)
                return ServiceResult.Missing();

            bool hasFlights = await context.Flights.AnyAsync(f => f.AircraftId == id);
            if (hasFlights)
                return ServiceResult.Failure(string.Empty, "Aircraft cannot be deleted while it is assigned to flights.");

            context.Aircraft.Remove(aircraft);
            await context.SaveChangesAsync();
            return ServiceResult.Success();
        }

        private static Task<ServiceResult> ValidateAsync(AircraftViewModel model)
        {
            ServiceResult result = ServiceResult.Success();

            if (string.IsNullOrWhiteSpace(model.Model))
                result.AddError(nameof(model.Model), "Aircraft model is required.");

            if (string.IsNullOrWhiteSpace(model.Manufacturer))
                result.AddError(nameof(model.Manufacturer), "Aircraft manufacturer is required.");

            if (model.Capacity <= 0)
                result.AddError(nameof(model.Capacity), "Aircraft capacity must be greater than zero.");

            return Task.FromResult(result);
        }

        private static AircraftViewModel Map(Aircraft aircraft)
        {
            return new AircraftViewModel
            {
                Id = aircraft.Id,
                Model = aircraft.Model,
                Manufacturer = aircraft.Manufacturer,
                Capacity = aircraft.Capacity,
            };
        }
    }
}