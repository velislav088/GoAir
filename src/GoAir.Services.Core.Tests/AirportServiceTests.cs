namespace GoAir.Services.Core.Tests
{
    using Common;
    using Data;
    using Data.Models;
    using Services;
    using Web.ViewModels.Airport;

    using Microsoft.EntityFrameworkCore;

    [TestFixture]
    public class AirportServiceTests
    {
        [Test]
        public async Task GetAllAsync_ShouldReturnMappedAirports()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            dbContext.Airports.AddRange(
            new Airport { Id = Guid.NewGuid(), Name = "Sofia Airport", City = "Sofia", IATACode = "SOF" },
            new Airport { Id = Guid.NewGuid(), Name = "Heathrow", City = "London", IATACode = "LHR" });
            await dbContext.SaveChangesAsync();
            AirportService service = new(dbContext);
            AirportIndexViewModel result = await service.GetAllAsync(null, 1, false);
            Assert.That(result.Airports.Count(), Is.EqualTo(2));
            Assert.That(result.Airports.Select(a => a.IATACode), Is.EquivalentTo(new[] { "SOF", "LHR" }));
            Assert.That(result.IsAdmin, Is.False);
        }
        [Test]
        public async Task GetByIdAsync_ShouldReturnMappedAirport()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            Airport airport = new() { Id = Guid.NewGuid(), Name = "Sofia Airport", City = "Sofia", IATACode = "SOF" };
            dbContext.Airports.Add(airport);
            await dbContext.SaveChangesAsync();
            AirportService service = new(dbContext);
            AirportViewModel? result = await service.GetByIdAsync(airport.Id);
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.City, Is.EqualTo("Sofia"));
        }
        [Test]
        public async Task CreateAsync_ShouldNormalizeCodeAndPersistAirport()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            AirportService service = new(dbContext);
            AirportViewModel model = new()
            {
                Name = "Burgas Airport",
                City = "Burgas",
                IATACode = " bos ",
            };
            ServiceResult result = await service.CreateAsync(model);
            Airport storedAirport = await dbContext.Airports.SingleAsync();
            Assert.That(result.Succeeded, Is.True);
            Assert.That(storedAirport.IATACode, Is.EqualTo("BOS"));
            Assert.That(model.IATACode, Is.EqualTo("BOS"));
        }
        [Test]
        public async Task CreateAsync_ShouldRejectDuplicateIataCode()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            dbContext.Airports.Add(new Airport { Id = Guid.NewGuid(), Name = "Sofia Airport", City = "Sofia", IATACode = "SOF" });
            await dbContext.SaveChangesAsync();
            AirportService service = new(dbContext);
            ServiceResult result = await service.CreateAsync(new AirportViewModel
            {
                Name = "Secondary Sofia Airport",
                City = "Sofia",
                IATACode = "sof",
            });
            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Errors[nameof(AirportViewModel.IATACode)], Does.Contain("This IATA code is already in use."));
        }
        [Test]
        public async Task GetForEditAsync_ShouldReturnMappedAirport()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            Airport airport = new() { Id = Guid.NewGuid(), Name = "Varna Airport", City = "Varna", IATACode = "VAR" };
            dbContext.Airports.Add(airport);
            await dbContext.SaveChangesAsync();
            AirportService service = new(dbContext);
            AirportViewModel? result = await service.GetForEditAsync(airport.Id);
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Name, Is.EqualTo("Varna Airport"));
        }
        [Test]
        public async Task UpdateAsync_ShouldPersistChanges()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            Airport airport = new() { Id = Guid.NewGuid(), Name = "Old Name", City = "Old City", IATACode = "OLD" };
            dbContext.Airports.Add(airport);
            await dbContext.SaveChangesAsync();
            dbContext.ChangeTracker.Clear();
            AirportService service = new(dbContext);
            ServiceResult result = await service.UpdateAsync(new AirportViewModel
            {
                Id = airport.Id,
                Name = "New Name",
                City = "New City",
                IATACode = " new ",
            });
            Airport storedAirport = await dbContext.Airports.SingleAsync();
            Assert.That(result.Succeeded, Is.True);
            Assert.That(storedAirport.Name, Is.EqualTo("New Name"));
            Assert.That(storedAirport.City, Is.EqualTo("New City"));
            Assert.That(storedAirport.IATACode, Is.EqualTo("NEW"));
        }
        [Test]
        public async Task DeleteAsync_ShouldRemoveAirport_WhenUnused()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            Airport airport = new() { Id = Guid.NewGuid(), Name = "Burgas Airport", City = "Burgas", IATACode = "BOJ" };
            dbContext.Airports.Add(airport);
            await dbContext.SaveChangesAsync();
            AirportService service = new(dbContext);
            ServiceResult result = await service.DeleteAsync(airport.Id);
            Assert.That(result.Succeeded, Is.True);
            Assert.That(await dbContext.Airports.CountAsync(), Is.EqualTo(0));
        }
        [Test]
        public async Task DeleteAsync_ShouldBlockDeletion_WhenAirportHasFlights()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            Airport airport = new() { Id = Guid.NewGuid(), Name = "Sofia Airport", City = "Sofia", IATACode = "SOF" };
            Airport arrival = new() { Id = Guid.NewGuid(), Name = "Heathrow", City = "London", IATACode = "LHR" };
            Aircraft aircraft = new() { Id = Guid.NewGuid(), Manufacturer = "Airbus", Model = "A320", Capacity = 180 };
            dbContext.Airports.AddRange(airport, arrival);
            dbContext.Aircraft.Add(aircraft);
            dbContext.Flights.Add(new Flight
            {
                Id = Guid.NewGuid(),
                FlightNumber = "GA808",
                DepartureTime = DateTime.UtcNow.AddDays(1),
                ArrivalTime = DateTime.UtcNow.AddDays(1).AddHours(2),
                Status = FlightStatus.OnTime,
                DepartureAirportId = airport.Id,
                ArrivalAirportId = arrival.Id,
                AircraftId = aircraft.Id,
            });
            await dbContext.SaveChangesAsync();
            AirportService service = new(dbContext);
            ServiceResult result = await service.DeleteAsync(airport.Id);
            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Errors[string.Empty], Does.Contain("Airport cannot be deleted while it is used by flights."));
        }
        [Test]
        public async Task DeleteAsync_ShouldReturnMissing_WhenAirportDoesNotExist()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            AirportService service = new(dbContext);
            ServiceResult result = await service.DeleteAsync(Guid.NewGuid());
            Assert.That(result.NotFound, Is.True);
        }
    }
}