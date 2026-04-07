namespace GoAir.Services.Core.Tests
{
    using Data;
    using Contracts;
    using Web.ViewModels.Common;

    using Microsoft.EntityFrameworkCore;

    internal static class ServiceTestHelpers
    {
        public static ApplicationDbContext CreateDbContext()
        {
            DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
            return new ApplicationDbContext(options);
        }
    }
    internal sealed class TestLookupService : ILookupService
    {
        public IEnumerable<LookupOptionViewModel> AircraftOptions { get; init; } = [];
        public IEnumerable<LookupOptionViewModel> AirportOptions { get; init; } = [];
        public IEnumerable<LookupOptionViewModel> FlightOptions { get; init; } = [];
        public IEnumerable<LookupOptionViewModel> UserOptions { get; init; } = [];
        public Task<IEnumerable<LookupOptionViewModel>> GetAircraftOptionsAsync()
        {
            return Task.FromResult(AircraftOptions);
        }

        public Task<IEnumerable<LookupOptionViewModel>> GetAirportOptionsAsync()
        {
            return Task.FromResult(AirportOptions);
        }

        public Task<IEnumerable<LookupOptionViewModel>> GetFlightOptionsAsync()
        {
            return Task.FromResult(FlightOptions);
        }

        public Task<IEnumerable<LookupOptionViewModel>> GetUserOptionsAsync()
        {
            return Task.FromResult(UserOptions);
        }
    }
}