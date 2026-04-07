namespace GoAir.IntegrationTests
{
    using Data;
    using Data.Models;

    using Microsoft.EntityFrameworkCore;

    [TestFixture]
    public class ApplicationDbContextIntegrationTests
    {
        [Test]
        public async Task Context_ShouldPersistRelatedFlightData()
        {
            DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
            await using ApplicationDbContext dbContext = new(options);
            Airport departure = new() { Id = Guid.NewGuid(), Name = "Sofia Airport", City = "Sofia", IATACode = "SOF" };
            Airport arrival = new() { Id = Guid.NewGuid(), Name = "Heathrow", City = "London", IATACode = "LHR" };
            Aircraft aircraft = new() { Id = Guid.NewGuid(), Manufacturer = "Airbus", Model = "A320", Capacity = 180 };
            dbContext.Airports.AddRange(departure, arrival);
            dbContext.Aircraft.Add(aircraft);
            dbContext.Flights.Add(new Flight
            {
                Id = Guid.NewGuid(),
                FlightNumber = "GA515",
                DepartureTime = DateTime.UtcNow.AddDays(1),
                ArrivalTime = DateTime.UtcNow.AddDays(1).AddHours(3),
                Status = FlightStatus.OnTime,
                DepartureAirportId = departure.Id,
                ArrivalAirportId = arrival.Id,
                AircraftId = aircraft.Id,
            });
            await dbContext.SaveChangesAsync();
            Flight? storedFlight = await dbContext.Flights
            .Include(f => f.DepartureAirport)
            .Include(f => f.ArrivalAirport)
            .Include(f => f.Aircraft)
            .SingleAsync();
            Assert.That(storedFlight.DepartureAirport.IATACode, Is.EqualTo("SOF"));
            Assert.That(storedFlight.ArrivalAirport.IATACode, Is.EqualTo("LHR"));
            Assert.That(storedFlight.Aircraft.Model, Is.EqualTo("A320"));
        }
    }
}
