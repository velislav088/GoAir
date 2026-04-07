namespace GoAir.Services.Core.Tests
{
    using Data;
    using Data.Models;
    using Services;

    [TestFixture]
    public class LookupServiceTests
    {
        [Test]
        public async Task GetAircraftOptionsAsync_ShouldReturnOrderedAircraft()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            dbContext.Aircraft.AddRange(
            new Aircraft { Id = Guid.NewGuid(), Manufacturer = "Boeing", Model = "737", Capacity = 189 },
            new Aircraft { Id = Guid.NewGuid(), Manufacturer = "Airbus", Model = "A220", Capacity = 150 });
            await dbContext.SaveChangesAsync();
            LookupService service = new(dbContext);
            List<string> options = (await service.GetAircraftOptionsAsync()).Select(x => x.Text).ToList();
            Assert.That(options, Is.EqualTo(new[] { "Airbus A220", "Boeing 737" }));
        }
        [Test]
        public async Task GetAirportOptionsAsync_ShouldReturnOrderedOptions()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            dbContext.Airports.AddRange(
            new Airport { Id = Guid.NewGuid(), Name = "Heathrow", City = "London", IATACode = "LHR" },
            new Airport { Id = Guid.NewGuid(), Name = "Sofia Airport", City = "Sofia", IATACode = "SOF" },
            new Airport { Id = Guid.NewGuid(), Name = "Charles de Gaulle", City = "Paris", IATACode = "CDG" });
            await dbContext.SaveChangesAsync();
            LookupService service = new(dbContext);
            List<string> options = (await service.GetAirportOptionsAsync()).Select(x => x.Text).ToList();
            Assert.That(options, Is.EqualTo(new[] { "London (LHR)", "Paris (CDG)", "Sofia (SOF)" }));
        }
        [Test]
        public async Task GetFlightOptionsAsync_ShouldReturnFlightNumbersOrderedAscending()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            dbContext.Flights.AddRange(
            new Flight { Id = Guid.NewGuid(), FlightNumber = "GA300", DepartureTime = DateTime.UtcNow, ArrivalTime = DateTime.UtcNow.AddHours(2), Status = FlightStatus.OnTime },
            new Flight { Id = Guid.NewGuid(), FlightNumber = "GA100", DepartureTime = DateTime.UtcNow, ArrivalTime = DateTime.UtcNow.AddHours(1), Status = FlightStatus.Delayed });
            await dbContext.SaveChangesAsync();
            LookupService service = new(dbContext);
            List<string> options = (await service.GetFlightOptionsAsync()).Select(x => x.Text).ToList();
            Assert.That(options, Is.EqualTo(new[] { "GA100", "GA300" }));
        }
        [Test]
        public async Task GetUserOptionsAsync_ShouldPreferUsernameAndFallbackToEmail()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            dbContext.ApplicationUsers.AddRange(
            new ApplicationUser { Id = "1", UserName = "alpha", Email = "alpha@goair.local" },
            new ApplicationUser { Id = "2", Email = "beta@goair.local" });
            await dbContext.SaveChangesAsync();
            LookupService service = new(dbContext);
            List<string> options = (await service.GetUserOptionsAsync()).Select(x => x.Text).ToList();
            Assert.That(options, Does.Contain("alpha"));
            Assert.That(options, Does.Contain("beta@goair.local"));
        }
        [Test]
        public async Task GetUserOptionsAsync_ShouldFallbackToId_WhenUsernameAndEmailAreMissing()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            dbContext.ApplicationUsers.Add(new ApplicationUser { Id = "3" });
            await dbContext.SaveChangesAsync();
            LookupService service = new(dbContext);
            List<string> options = (await service.GetUserOptionsAsync()).Select(x => x.Text).ToList();
            Assert.That(options, Does.Contain("3"));
        }
    }
}