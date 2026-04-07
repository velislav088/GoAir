namespace GoAir.Services.Core.Services
{
    using Data;
    using Contracts;
    using Web.ViewModels.Common;

    using Microsoft.EntityFrameworkCore;

    public class LookupService(ApplicationDbContext context) : ILookupService
    {
        public async Task<IEnumerable<LookupOptionViewModel>> GetAircraftOptionsAsync()
        {
            return await context.Aircraft
            .AsNoTracking()
            .OrderBy(a => a.Manufacturer)
            .ThenBy(a => a.Model)
            .Select(a => new LookupOptionViewModel
            {
                Value = a.Id.ToString(),
                Text = $"{a.Manufacturer} {a.Model}",
            })
            .ToListAsync();
        }

        public async Task<IEnumerable<LookupOptionViewModel>> GetAirportOptionsAsync()
        {
            return await context.Airports
            .AsNoTracking()
            .OrderBy(a => a.City)
            .ThenBy(a => a.Name)
            .Select(a => new LookupOptionViewModel
            {
                Value = a.Id.ToString(),
                Text = $"{a.City} ({a.IATACode})",
            })
            .ToListAsync();
        }

        public async Task<IEnumerable<LookupOptionViewModel>> GetFlightOptionsAsync()
        {
            return await context.Flights
            .AsNoTracking()
            .OrderBy(f => f.FlightNumber)
            .Select(f => new LookupOptionViewModel
            {
                Value = f.Id.ToString(),
                Text = f.FlightNumber,
            })
            .ToListAsync();
        }

        public async Task<IEnumerable<LookupOptionViewModel>> GetUserOptionsAsync()
        {
            return await context.ApplicationUsers
            .AsNoTracking()
            .OrderBy(u => u.UserName)
            .ThenBy(u => u.Email)
            .Select(u => new LookupOptionViewModel
            {
                Value = u.Id,
                Text = u.UserName ?? u.Email ?? u.Id,
            })
            .ToListAsync();
        }
    }
}