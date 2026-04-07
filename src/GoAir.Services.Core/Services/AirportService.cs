namespace GoAir.Services.Core.Services
{
    using Data;
    using Data.Models;
    using Common;
    using Contracts;
    using Web.ViewModels.Airport;

    using Microsoft.EntityFrameworkCore;

    public class AirportService(ApplicationDbContext context) : IAirportService
    {
        public async Task<AirportIndexViewModel> GetAllAsync(string? searchTerm, int page, bool isAdmin)
        {
            const int PageSize = 6;
            IQueryable<Airport> query = context.Airports
            .AsNoTracking()
            .AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string normalizedSearch = searchTerm.Trim();
                query = query.Where(a =>
                a.Name.Contains(normalizedSearch) ||
                a.City.Contains(normalizedSearch) ||
                a.IATACode.Contains(normalizedSearch));
            }
            query = query
            .OrderBy(a => a.City)
            .ThenBy(a => a.Name);
            int totalAirports = await query.CountAsync();
            int totalPages = Math.Max(1, (int)Math.Ceiling(totalAirports / (double)PageSize));
            int currentPage = Math.Clamp(page, 1, totalPages);
            List<AirportViewModel> airports = await query
            .Skip((currentPage - 1) * PageSize)
            .Take(PageSize)
            .Select(a => new AirportViewModel
            {
                Id = a.Id,
                Name = a.Name,
                IATACode = a.IATACode,
                City = a.City,
            })
            .ToListAsync();
            return new AirportIndexViewModel
            {
                SearchTerm = searchTerm?.Trim() ?? string.Empty,
                CurrentPage = currentPage,
                TotalPages = totalPages,
                IsAdmin = isAdmin,
                Airports = airports,
            };
        }

        public async Task<AirportViewModel?> GetByIdAsync(Guid id)
        {
            Airport? airport = await context.Airports
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);

            return airport == null ? null : Map(airport);
        }

        public async Task<ServiceResult> CreateAsync(AirportViewModel model)
        {
            ServiceResult validation = await ValidateAsync(model);
            if (!validation.Succeeded)
            return validation;

            Airport airport = new()
            {
                Id = Guid.NewGuid(),
                Name = model.Name,
                IATACode = model.IATACode,
                City = model.City,
            };

            context.Airports.Add(airport);
            await context.SaveChangesAsync();
            return ServiceResult.Success();
        }

        public async Task<AirportViewModel?> GetForEditAsync(Guid id)
        {
            Airport? airport = await context.Airports.FindAsync(id);
            return airport == null ? null : Map(airport);
        }

        public async Task<ServiceResult> UpdateAsync(AirportViewModel model)
        {
            bool exists = await context.Airports.AnyAsync(a => a.Id == model.Id);
            if (!exists)
            return ServiceResult.Missing();

            ServiceResult validation = await ValidateAsync(model);
            if (!validation.Succeeded)
            return validation;

            Airport airport = new()
            {
                Id = model.Id,
                Name = model.Name,
                IATACode = model.IATACode,
                City = model.City,
            };

            context.Airports.Update(airport);
            await context.SaveChangesAsync();
            return ServiceResult.Success();
        }

        public async Task<ServiceResult> DeleteAsync(Guid id)
        {
            Airport? airport = await context.Airports.FindAsync(id);
            if (airport == null)
            return ServiceResult.Missing();

            bool hasFlights = await context.Flights.AnyAsync(f => f.DepartureAirportId == id || f.ArrivalAirportId == id);
            if (hasFlights)
            return ServiceResult.Failure(string.Empty, "Airport cannot be deleted while it is used by flights.");

            context.Airports.Remove(airport);
            await context.SaveChangesAsync();
            return ServiceResult.Success();
        }

        private async Task<ServiceResult> ValidateAsync(AirportViewModel model)
        {
            ServiceResult result = ServiceResult.Success();

            if (string.IsNullOrWhiteSpace(model.Name))
            result.AddError(nameof(model.Name), "Airport name is required.");

            if (string.IsNullOrWhiteSpace(model.City))
            result.AddError(nameof(model.City), "Airport city is required.");

            string normalizedCode = model.IATACode.Trim().ToUpperInvariant();
            if (normalizedCode.Length != 3)
            result.AddError(nameof(model.IATACode), "IATA code must be exactly 3 characters.");
            else
            {
                bool duplicateCode = await context.Airports.AnyAsync(a => a.IATACode == normalizedCode && a.Id != model.Id);
                if (duplicateCode)
                result.AddError(nameof(model.IATACode), "This IATA code is already in use.");
            }

            model.IATACode = normalizedCode;
            return result;
        }

        private static AirportViewModel Map(Airport airport)
        {
            return new AirportViewModel
            {
                Id = airport.Id,
                Name = airport.Name,
                IATACode = airport.IATACode,
                City = airport.City,
            };
        }
    }
}