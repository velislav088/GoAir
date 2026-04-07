namespace GoAir.Services.Core.Tests
{
    using Common;
    using Data;
    using Data.Models;
    using Services;
    using Web.ViewModels.Aircraft;

    using Microsoft.EntityFrameworkCore;

    [TestFixture]
    public class AircraftServiceTests
    {
        [Test]
        public async Task GetAllAsync_ShouldReturnMappedAircraft()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            dbContext.Aircraft.AddRange(
            new Aircraft { Id = Guid.NewGuid(), Manufacturer = "Airbus", Model = "A320", Capacity = 180 },
            new Aircraft { Id = Guid.NewGuid(), Manufacturer = "Boeing", Model = "737", Capacity = 189 });
            await dbContext.SaveChangesAsync();
            AircraftService service = new(dbContext);
            AircraftIndexViewModel result = await service.GetAllAsync(null, 1, false);
            Assert.That(result.Aircraft.Count(), Is.EqualTo(2));
            Assert.That(result.Aircraft.Select(a => a.Model), Is.EquivalentTo(new[] { "A320", "737" }));
            Assert.That(result.IsAdmin, Is.False);
        }
        [Test]
        public async Task GetByIdAsync_ShouldReturnMappedAircraft()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            Aircraft aircraft = new() { Id = Guid.NewGuid(), Manufacturer = "Airbus", Model = "A321", Capacity = 220 };
            dbContext.Aircraft.Add(aircraft);
            await dbContext.SaveChangesAsync();
            AircraftService service = new(dbContext);
            AircraftViewModel? result = await service.GetByIdAsync(aircraft.Id);
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Capacity, Is.EqualTo(220));
        }
        [Test]
        public async Task CreateAsync_ShouldRejectMissingFields()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            AircraftService service = new(dbContext);
            ServiceResult result = await service.CreateAsync(new AircraftViewModel { Capacity = 0 });
            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Errors[nameof(AircraftViewModel.Model)], Does.Contain("Aircraft model is required."));
            Assert.That(result.Errors[nameof(AircraftViewModel.Manufacturer)], Does.Contain("Aircraft manufacturer is required."));
            Assert.That(result.Errors[nameof(AircraftViewModel.Capacity)], Does.Contain("Aircraft capacity must be greater than zero."));
        }
        [Test]
        public async Task CreateAsync_ShouldPersistAircraft_WhenValid()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            AircraftService service = new(dbContext);
            ServiceResult result = await service.CreateAsync(new AircraftViewModel
            {
                Manufacturer = "Embraer",
                Model = "E195-E2",
                Capacity = 146,
            });
            Aircraft storedAircraft = await dbContext.Aircraft.SingleAsync();
            Assert.That(result.Succeeded, Is.True);
            Assert.That(storedAircraft.Model, Is.EqualTo("E195-E2"));
        }
        [Test]
        public async Task GetForEditAsync_ShouldReturnMappedAircraft()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            Aircraft aircraft = new() { Id = Guid.NewGuid(), Manufacturer = "Airbus", Model = "A220", Capacity = 150 };
            dbContext.Aircraft.Add(aircraft);
            await dbContext.SaveChangesAsync();
            AircraftService service = new(dbContext);
            AircraftViewModel? result = await service.GetForEditAsync(aircraft.Id);
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Manufacturer, Is.EqualTo("Airbus"));
        }
        [Test]
        public async Task UpdateAsync_ShouldReturnMissing_WhenAircraftDoesNotExist()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            AircraftService service = new(dbContext);
            ServiceResult result = await service.UpdateAsync(new AircraftViewModel
            {
                Id = Guid.NewGuid(),
                Manufacturer = "Airbus",
                Model = "A321",
                Capacity = 200,
            });
            Assert.That(result.NotFound, Is.True);
        }
        [Test]
        public async Task UpdateAsync_ShouldPersistChanges_WhenAircraftExists()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            Aircraft aircraft = new() { Id = Guid.NewGuid(), Manufacturer = "Airbus", Model = "A319", Capacity = 144 };
            dbContext.Aircraft.Add(aircraft);
            await dbContext.SaveChangesAsync();
            dbContext.ChangeTracker.Clear();
            AircraftService service = new(dbContext);
            ServiceResult result = await service.UpdateAsync(new AircraftViewModel
            {
                Id = aircraft.Id,
                Manufacturer = "Boeing",
                Model = "737 MAX 8",
                Capacity = 178,
            });
            Aircraft storedAircraft = await dbContext.Aircraft.SingleAsync();
            Assert.That(result.Succeeded, Is.True);
            Assert.That(storedAircraft.Manufacturer, Is.EqualTo("Boeing"));
            Assert.That(storedAircraft.Capacity, Is.EqualTo(178));
        }
        [Test]
        public async Task DeleteAsync_ShouldBlockDeletion_WhenAircraftHasFlights()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            Airport departure = new() { Id = Guid.NewGuid(), Name = "Sofia Airport", City = "Sofia", IATACode = "SOF" };
            Airport arrival = new() { Id = Guid.NewGuid(), Name = "Heathrow", City = "London", IATACode = "LHR" };
            Aircraft aircraft = new() { Id = Guid.NewGuid(), Manufacturer = "Airbus", Model = "A320", Capacity = 180 };
            dbContext.Airports.AddRange(departure, arrival);
            dbContext.Aircraft.Add(aircraft);
            dbContext.Flights.Add(new Flight
            {
                Id = Guid.NewGuid(),
                FlightNumber = "GA404",
                DepartureTime = DateTime.UtcNow.AddDays(1),
                ArrivalTime = DateTime.UtcNow.AddDays(1).AddHours(2),
                Status = FlightStatus.OnTime,
                DepartureAirportId = departure.Id,
                ArrivalAirportId = arrival.Id,
                AircraftId = aircraft.Id,
            });
            await dbContext.SaveChangesAsync();
            AircraftService service = new(dbContext);
            ServiceResult result = await service.DeleteAsync(aircraft.Id);
            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Errors[string.Empty], Does.Contain("Aircraft cannot be deleted while it is assigned to flights."));
        }
        [Test]
        public async Task DeleteAsync_ShouldRemoveAircraft_WhenUnused()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            Aircraft aircraft = new() { Id = Guid.NewGuid(), Manufacturer = "Airbus", Model = "A220", Capacity = 150 };
            dbContext.Aircraft.Add(aircraft);
            await dbContext.SaveChangesAsync();
            AircraftService service = new(dbContext);
            ServiceResult result = await service.DeleteAsync(aircraft.Id);
            Assert.That(result.Succeeded, Is.True);
            Assert.That(await dbContext.Aircraft.CountAsync(), Is.EqualTo(0));
        }
    }
}