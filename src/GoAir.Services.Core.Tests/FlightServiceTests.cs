namespace GoAir.Services.Core.Tests
{
    using Data;
    using Data.Models;
    using Common;
    using Services;
    using Web.ViewModels.Flight;

    using Microsoft.EntityFrameworkCore;

    [TestFixture]
    public class FlightServiceTests
    {
        [Test]
        public async Task GetCreateModelAsync_ShouldPopulateLookupOptions()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            TestLookupService lookupService = new()
            {
                AirportOptions =
                [
                new() { Value = Guid.NewGuid().ToString(), Text = "Sofia (SOF)" }
                ],
                AircraftOptions =
                [
                new() { Value = Guid.NewGuid().ToString(), Text = "Airbus A320" }
                ],
            };
            FlightService service = new(dbContext, lookupService);
            FlightFormViewModel model = await service.GetCreateModelAsync();
            Assert.That(model.DepartureAirports.Select(a => a.Text), Is.EqualTo(new[] { "Sofia (SOF)" }));
            Assert.That(model.ArrivalAirports.Select(a => a.Text), Is.EqualTo(new[] { "Sofia (SOF)" }));
            Assert.That(model.AircraftOptions.Select(a => a.Text), Is.EqualTo(new[] { "Airbus A320" }));
        }
        [Test]
        public async Task CreateAsync_ShouldRejectOverlappingAircraftAssignments()
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
                FlightNumber = "GA111",
                DepartureTime = new DateTime(2026, 4, 10, 8, 0, 0, DateTimeKind.Utc),
                ArrivalTime = new DateTime(2026, 4, 10, 10, 0, 0, DateTimeKind.Utc),
                Status = FlightStatus.OnTime,
                DepartureAirportId = departure.Id,
                ArrivalAirportId = arrival.Id,
                AircraftId = aircraft.Id,
            });
            await dbContext.SaveChangesAsync();
            FlightService service = new(dbContext, new TestLookupService());
            ServiceResult result = await service.CreateAsync(new FlightFormViewModel
            {
                FlightNumber = "GA222",
                DepartureTime = new DateTime(2026, 4, 10, 9, 0, 0, DateTimeKind.Utc),
                ArrivalTime = new DateTime(2026, 4, 10, 11, 0, 0, DateTimeKind.Utc),
                Status = FlightStatus.OnTime,
                DepartureAirportId = departure.Id,
                ArrivalAirportId = arrival.Id,
                AircraftId = aircraft.Id,
            });
            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Errors[nameof(FlightFormViewModel.AircraftId)], Does.Contain("This aircraft is already assigned to another flight in the selected time range."));
        }
        [Test]
        public async Task CreateAsync_ShouldRejectInvalidRouteAndSchedule()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            Airport airport = new() { Id = Guid.NewGuid(), Name = "Sofia Airport", City = "Sofia", IATACode = "SOF" };
            Aircraft aircraft = new() { Id = Guid.NewGuid(), Manufacturer = "Airbus", Model = "A320", Capacity = 180 };
            dbContext.Airports.Add(airport);
            dbContext.Aircraft.Add(aircraft);
            await dbContext.SaveChangesAsync();
            FlightService service = new(dbContext, new TestLookupService());
            ServiceResult result = await service.CreateAsync(new FlightFormViewModel
            {
                FlightNumber = "ga500",
                DepartureTime = new DateTime(2026, 4, 10, 11, 0, 0, DateTimeKind.Utc),
                ArrivalTime = new DateTime(2026, 4, 10, 10, 0, 0, DateTimeKind.Utc),
                Status = FlightStatus.OnTime,
                DepartureAirportId = airport.Id,
                ArrivalAirportId = airport.Id,
                AircraftId = aircraft.Id,
            });
            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Errors[nameof(FlightFormViewModel.ArrivalAirportId)], Does.Contain("Arrival airport must be different from departure airport."));
            Assert.That(result.Errors[nameof(FlightFormViewModel.ArrivalTime)], Does.Contain("Arrival time must be after departure time."));
        }
        [Test]
        public async Task GetAllAsync_ShouldFilterSortAndPaginateFlights()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            Airport sofia = new() { Id = Guid.NewGuid(), Name = "Sofia Airport", City = "Sofia", IATACode = "SOF" };
            Airport varna = new() { Id = Guid.NewGuid(), Name = "Varna Airport", City = "Varna", IATACode = "VAR" };
            Airport london = new() { Id = Guid.NewGuid(), Name = "Heathrow", City = "London", IATACode = "LHR" };
            Aircraft airbus = new() { Id = Guid.NewGuid(), Manufacturer = "Airbus", Model = "A220", Capacity = 150 };
            Aircraft boeing = new() { Id = Guid.NewGuid(), Manufacturer = "Boeing", Model = "737", Capacity = 180 };
            dbContext.Airports.AddRange(sofia, varna, london);
            dbContext.Aircraft.AddRange(airbus, boeing);
            for (int i = 0; i < 8; i++)
            {
                dbContext.Flights.Add(new Flight
                {
                    Id = Guid.NewGuid(),
                    FlightNumber = $"GA10{i}",
                    DepartureTime = new DateTime(2026, 4, 10, 8, 0, 0, DateTimeKind.Utc).AddHours(i),
                    ArrivalTime = new DateTime(2026, 4, 10, 10, 0, 0, DateTimeKind.Utc).AddHours(i),
                    Status = FlightStatus.OnTime,
                    DepartureAirportId = i % 2 == 0 ? sofia.Id : varna.Id,
                    ArrivalAirportId = london.Id,
                    AircraftId = i % 2 == 0 ? airbus.Id : boeing.Id,
                });
            }
            await dbContext.SaveChangesAsync();
            FlightService service = new(dbContext, new TestLookupService());
            FlightIndexViewModel result = await service.GetAllAsync("Sofia", FlightSorting.FlightNumber, 2, true);
            Assert.That(result.CurrentPage, Is.EqualTo(1));
            Assert.That(result.TotalPages, Is.EqualTo(1));
            Assert.That(result.IsAdmin, Is.True);
            Assert.That(result.Flights.Select(f => f.FlightNumber), Is.EqualTo(new[] { "GA100", "GA102", "GA104", "GA106" }));
        }
        [Test]
        public async Task CreateAsync_ShouldNormalizeFlightNumberAndPersistFlight()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            Airport departure = new() { Id = Guid.NewGuid(), Name = "Sofia Airport", City = "Sofia", IATACode = "SOF" };
            Airport arrival = new() { Id = Guid.NewGuid(), Name = "Heathrow", City = "London", IATACode = "LHR" };
            Aircraft aircraft = new() { Id = Guid.NewGuid(), Manufacturer = "Airbus", Model = "A320", Capacity = 180 };
            dbContext.Airports.AddRange(departure, arrival);
            dbContext.Aircraft.Add(aircraft);
            await dbContext.SaveChangesAsync();
            FlightService service = new(dbContext, new TestLookupService());
            ServiceResult result = await service.CreateAsync(new FlightFormViewModel
            {
                FlightNumber = " ga321 ",
                DepartureTime = new DateTime(2026, 4, 10, 9, 0, 0, DateTimeKind.Utc),
                ArrivalTime = new DateTime(2026, 4, 10, 11, 0, 0, DateTimeKind.Utc),
                Status = FlightStatus.Delayed,
                DepartureAirportId = departure.Id,
                ArrivalAirportId = arrival.Id,
                AircraftId = aircraft.Id,
            });
            Flight storedFlight = await dbContext.Flights.SingleAsync();
            Assert.That(result.Succeeded, Is.True);
            Assert.That(storedFlight.FlightNumber, Is.EqualTo("GA321"));
            Assert.That(storedFlight.Status, Is.EqualTo(FlightStatus.Delayed));
        }
        [Test]
        public async Task GetByIdAsync_ShouldReturnMappedFlight()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            Airport departure = new() { Id = Guid.NewGuid(), Name = "Sofia Airport", City = "Sofia", IATACode = "SOF" };
            Airport arrival = new() { Id = Guid.NewGuid(), Name = "Heathrow", City = "London", IATACode = "LHR" };
            Aircraft aircraft = new() { Id = Guid.NewGuid(), Manufacturer = "Airbus", Model = "A320", Capacity = 180 };
            Flight flight = new()
            {
                Id = Guid.NewGuid(),
                FlightNumber = "GA322",
                DepartureTime = DateTime.UtcNow.AddDays(1),
                ArrivalTime = DateTime.UtcNow.AddDays(1).AddHours(3),
                Status = FlightStatus.OnTime,
                DepartureAirportId = departure.Id,
                ArrivalAirportId = arrival.Id,
                AircraftId = aircraft.Id,
            };
            dbContext.Airports.AddRange(departure, arrival);
            dbContext.Aircraft.Add(aircraft);
            dbContext.Flights.Add(flight);
            await dbContext.SaveChangesAsync();
            FlightService service = new(dbContext, new TestLookupService());
            FlightViewModel? result = await service.GetByIdAsync(flight.Id);
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.FlightNumber, Is.EqualTo("GA322"));
            Assert.That(result.DepartureAirport, Does.Contain("SOF"));
            Assert.That(result.Aircraft, Is.EqualTo("Airbus A320"));
        }
        [Test]
        public async Task GetForEditAsync_ShouldReturnMappedFlightAndPopulateOptions()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            Airport departure = new() { Id = Guid.NewGuid(), Name = "Sofia Airport", City = "Sofia", IATACode = "SOF" };
            Airport arrival = new() { Id = Guid.NewGuid(), Name = "Heathrow", City = "London", IATACode = "LHR" };
            Aircraft aircraft = new() { Id = Guid.NewGuid(), Manufacturer = "Airbus", Model = "A320", Capacity = 180 };
            Flight flight = new()
            {
                Id = Guid.NewGuid(),
                FlightNumber = "GA323",
                DepartureTime = DateTime.UtcNow.AddDays(1),
                ArrivalTime = DateTime.UtcNow.AddDays(1).AddHours(3),
                Status = FlightStatus.Delayed,
                DepartureAirportId = departure.Id,
                ArrivalAirportId = arrival.Id,
                AircraftId = aircraft.Id,
            };
            dbContext.Airports.AddRange(departure, arrival);
            dbContext.Aircraft.Add(aircraft);
            dbContext.Flights.Add(flight);
            await dbContext.SaveChangesAsync();
            TestLookupService lookupService = new()
            {
                AirportOptions =
                [
                new() { Value = departure.Id.ToString(), Text = "Sofia (SOF)" },
                    new() { Value = arrival.Id.ToString(), Text = "London (LHR)" }
                ],
                AircraftOptions =
                [
                new() { Value = aircraft.Id.ToString(), Text = "Airbus A320" }
                ],
            };
            FlightService service = new(dbContext, lookupService);
            FlightFormViewModel? result = await service.GetForEditAsync(flight.Id);
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.FlightNumber, Is.EqualTo("GA323"));
            Assert.That(result.DepartureAirports.Count(), Is.EqualTo(2));
            Assert.That(result.AircraftOptions.Count(), Is.EqualTo(1));
        }
        [Test]
        public async Task UpdateAsync_ShouldReturnMissing_WhenFlightDoesNotExist()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            FlightService service = new(dbContext, new TestLookupService());
            ServiceResult result = await service.UpdateAsync(new FlightFormViewModel
            {
                Id = Guid.NewGuid(),
                FlightNumber = "GA324",
                DepartureTime = DateTime.UtcNow.AddDays(1),
                ArrivalTime = DateTime.UtcNow.AddDays(1).AddHours(2),
                Status = FlightStatus.OnTime,
                DepartureAirportId = Guid.NewGuid(),
                ArrivalAirportId = Guid.NewGuid(),
                AircraftId = Guid.NewGuid(),
            });
            Assert.That(result.NotFound, Is.True);
        }
        [Test]
        public async Task UpdateAsync_ShouldPersistFlightChanges()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            Airport departure = new() { Id = Guid.NewGuid(), Name = "Sofia Airport", City = "Sofia", IATACode = "SOF" };
            Airport arrival = new() { Id = Guid.NewGuid(), Name = "Heathrow", City = "London", IATACode = "LHR" };
            Airport newArrival = new() { Id = Guid.NewGuid(), Name = "Frankfurt", City = "Frankfurt", IATACode = "FRA" };
            Aircraft aircraft = new() { Id = Guid.NewGuid(), Manufacturer = "Airbus", Model = "A320", Capacity = 180 };
            Aircraft newAircraft = new() { Id = Guid.NewGuid(), Manufacturer = "Boeing", Model = "737", Capacity = 189 };
            Flight flight = new()
            {
                Id = Guid.NewGuid(),
                FlightNumber = "GA325",
                DepartureTime = new DateTime(2026, 4, 10, 9, 0, 0, DateTimeKind.Utc),
                ArrivalTime = new DateTime(2026, 4, 10, 11, 0, 0, DateTimeKind.Utc),
                Status = FlightStatus.OnTime,
                DepartureAirportId = departure.Id,
                ArrivalAirportId = arrival.Id,
                AircraftId = aircraft.Id,
            };
            dbContext.Airports.AddRange(departure, arrival, newArrival);
            dbContext.Aircraft.AddRange(aircraft, newAircraft);
            dbContext.Flights.Add(flight);
            await dbContext.SaveChangesAsync();
            dbContext.ChangeTracker.Clear();
            FlightService service = new(dbContext, new TestLookupService());
            ServiceResult result = await service.UpdateAsync(new FlightFormViewModel
            {
                Id = flight.Id,
                FlightNumber = " ga999 ",
                DepartureTime = new DateTime(2026, 4, 10, 12, 0, 0, DateTimeKind.Utc),
                ArrivalTime = new DateTime(2026, 4, 10, 14, 0, 0, DateTimeKind.Utc),
                Status = FlightStatus.Cancelled,
                DepartureAirportId = departure.Id,
                ArrivalAirportId = newArrival.Id,
                AircraftId = newAircraft.Id,
            });
            Flight storedFlight = await dbContext.Flights.SingleAsync();
            Assert.That(result.Succeeded, Is.True);
            Assert.That(storedFlight.FlightNumber, Is.EqualTo("GA999"));
            Assert.That(storedFlight.ArrivalAirportId, Is.EqualTo(newArrival.Id));
            Assert.That(storedFlight.AircraftId, Is.EqualTo(newAircraft.Id));
            Assert.That(storedFlight.Status, Is.EqualTo(FlightStatus.Cancelled));
        }
        [Test]
        public async Task DeleteAsync_ShouldFail_WhenTicketsExist()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            ApplicationUser user = new() { Id = "user-1", UserName = "user@goair.local", Email = "user@goair.local" };
            Airport departure = new() { Id = Guid.NewGuid(), Name = "Sofia Airport", City = "Sofia", IATACode = "SOF" };
            Airport arrival = new() { Id = Guid.NewGuid(), Name = "Heathrow", City = "London", IATACode = "LHR" };
            Aircraft aircraft = new() { Id = Guid.NewGuid(), Manufacturer = "Airbus", Model = "A320", Capacity = 180 };
            Flight flight = new()
            {
                Id = Guid.NewGuid(),
                FlightNumber = "GA777",
                DepartureTime = new DateTime(2026, 4, 12, 8, 0, 0, DateTimeKind.Utc),
                ArrivalTime = new DateTime(2026, 4, 12, 10, 0, 0, DateTimeKind.Utc),
                Status = FlightStatus.OnTime,
                DepartureAirportId = departure.Id,
                ArrivalAirportId = arrival.Id,
                AircraftId = aircraft.Id,
            };
            dbContext.ApplicationUsers.Add(user);
            dbContext.Airports.AddRange(departure, arrival);
            dbContext.Aircraft.Add(aircraft);
            dbContext.Flights.Add(flight);
            dbContext.Tickets.Add(new Ticket
            {
                Id = Guid.NewGuid(),
                SeatNumber = "12A",
                Price = 100,
                FareClass = FareClass.Economy,
                PurchasedOn = DateTime.UtcNow,
                FlightId = flight.Id,
                UserId = user.Id,
            });
            await dbContext.SaveChangesAsync();
            FlightService service = new(dbContext, new TestLookupService());
            ServiceResult result = await service.DeleteAsync(flight.Id);
            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Errors[string.Empty], Does.Contain("Flight cannot be deleted while tickets or reviews exist for it."));
        }
        [Test]
        public async Task DeleteAsync_ShouldReturnMissing_WhenFlightDoesNotExist()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            FlightService service = new(dbContext, new TestLookupService());
            ServiceResult result = await service.DeleteAsync(Guid.NewGuid());
            Assert.That(result.NotFound, Is.True);
        }
        [Test]
        public async Task DeleteAsync_ShouldRemoveFlight_WhenNoDependenciesExist()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            Airport departure = new() { Id = Guid.NewGuid(), Name = "Sofia Airport", City = "Sofia", IATACode = "SOF" };
            Airport arrival = new() { Id = Guid.NewGuid(), Name = "Heathrow", City = "London", IATACode = "LHR" };
            Aircraft aircraft = new() { Id = Guid.NewGuid(), Manufacturer = "Airbus", Model = "A320", Capacity = 180 };
            Flight flight = new()
            {
                Id = Guid.NewGuid(),
                FlightNumber = "GA326",
                DepartureTime = DateTime.UtcNow.AddDays(1),
                ArrivalTime = DateTime.UtcNow.AddDays(1).AddHours(3),
                Status = FlightStatus.OnTime,
                DepartureAirportId = departure.Id,
                ArrivalAirportId = arrival.Id,
                AircraftId = aircraft.Id,
            };
            dbContext.Airports.AddRange(departure, arrival);
            dbContext.Aircraft.Add(aircraft);
            dbContext.Flights.Add(flight);
            await dbContext.SaveChangesAsync();
            FlightService service = new(dbContext, new TestLookupService());
            ServiceResult result = await service.DeleteAsync(flight.Id);
            Assert.That(result.Succeeded, Is.True);
            Assert.That(await dbContext.Flights.CountAsync(), Is.EqualTo(0));
        }
    }
}