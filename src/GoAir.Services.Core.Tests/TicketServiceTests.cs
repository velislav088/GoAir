namespace GoAir.Services.Core.Tests
{
    using Data;
    using Data.Models;
    using Common;
    using Services;
    using Web.ViewModels.Ticket;

    using Microsoft.EntityFrameworkCore;

    [TestFixture]
    public class TicketServiceTests
    {
        [Test]
        public async Task GetAllAsync_ShouldReturnAllTickets_ForAdmin()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            ApplicationUser owner = new() { Id = "owner-id", UserName = "owner@goair.local", Email = "owner@goair.local" };
            ApplicationUser other = new() { Id = "other-id", UserName = "other@goair.local", Email = "other@goair.local" };
            Flight flight = new()
            {
                Id = Guid.NewGuid(),
                FlightNumber = "GA900",
                DepartureTime = DateTime.UtcNow.AddDays(1),
                ArrivalTime = DateTime.UtcNow.AddDays(1).AddHours(2),
                Status = FlightStatus.OnTime,
            };
            dbContext.ApplicationUsers.AddRange(owner, other);
            dbContext.Flights.Add(flight);
            dbContext.Tickets.AddRange(
            new Ticket { Id = Guid.NewGuid(), SeatNumber = "1A", Price = 150, FareClass = FareClass.Business, PurchasedOn = DateTime.UtcNow, UserId = owner.Id, FlightId = flight.Id },
            new Ticket { Id = Guid.NewGuid(), SeatNumber = "2B", Price = 99, FareClass = FareClass.Economy, PurchasedOn = DateTime.UtcNow.AddMinutes(-5), UserId = other.Id, FlightId = flight.Id });
            await dbContext.SaveChangesAsync();
            TicketService service = new(dbContext, new TestLookupService());
            TicketIndexViewModel result = await service.GetAllAsync(owner.Id, true, null, 1);
            Assert.That(result.Tickets.Count(), Is.EqualTo(2));
            Assert.That(result.Tickets.Select(t => t.UserId), Is.EquivalentTo(new[] { owner.Id, other.Id }));
            Assert.That(result.IsAdmin, Is.True);
        }
        [Test]
        public async Task GetByIdAsync_ShouldReturnNull_WhenTicketBelongsToAnotherUser()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            ApplicationUser owner = new() { Id = "owner-id", UserName = "owner@goair.local", Email = "owner@goair.local" };
            ApplicationUser intruder = new() { Id = "intruder-id", UserName = "intruder@goair.local", Email = "intruder@goair.local" };
            Flight flight = new()
            {
                Id = Guid.NewGuid(),
                FlightNumber = "GA908",
                DepartureTime = DateTime.UtcNow.AddDays(1),
                ArrivalTime = DateTime.UtcNow.AddDays(1).AddHours(2),
                Status = FlightStatus.OnTime,
            };
            Ticket ticket = new()
            {
                Id = Guid.NewGuid(),
                SeatNumber = "12A",
                Price = 129,
                FareClass = FareClass.Economy,
                PurchasedOn = DateTime.UtcNow,
                UserId = owner.Id,
                FlightId = flight.Id,
            };
            dbContext.ApplicationUsers.AddRange(owner, intruder);
            dbContext.Flights.Add(flight);
            dbContext.Tickets.Add(ticket);
            await dbContext.SaveChangesAsync();
            TicketService service = new(dbContext, new TestLookupService());
            TicketViewModel? model = await service.GetByIdAsync(ticket.Id, intruder.Id, false);
            Assert.That(model, Is.Null);
        }
        [Test]
        public async Task GetForEditAsync_ShouldReturnNull_WhenTicketBelongsToAnotherUser()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            ApplicationUser owner = new() { Id = "owner-id", UserName = "owner@goair.local", Email = "owner@goair.local" };
            ApplicationUser intruder = new() { Id = "intruder-id", UserName = "intruder@goair.local", Email = "intruder@goair.local" };
            Flight flight = new()
            {
                Id = Guid.NewGuid(),
                FlightNumber = "GA909",
                DepartureTime = DateTime.UtcNow.AddDays(1),
                ArrivalTime = DateTime.UtcNow.AddDays(1).AddHours(2),
                Status = FlightStatus.OnTime,
            };
            dbContext.ApplicationUsers.AddRange(owner, intruder);
            dbContext.Flights.Add(flight);
            dbContext.Tickets.Add(new Ticket
            {
                Id = Guid.NewGuid(),
                SeatNumber = "12A",
                Price = 129,
                FareClass = FareClass.Economy,
                PurchasedOn = DateTime.UtcNow,
                UserId = owner.Id,
                FlightId = flight.Id,
            });
            await dbContext.SaveChangesAsync();
            TicketService service = new(dbContext, new TestLookupService());
            Ticket storedTicket = await dbContext.Tickets.AsNoTracking().FirstAsync();
            TicketFormViewModel? model = await service.GetForEditAsync(storedTicket.Id, intruder.Id, false);
            Assert.That(model, Is.Null);
        }
        [Test]
        public async Task GetCreateModelAsync_ShouldPopulateFlightOptions()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            TestLookupService lookupService = new()
            {
                FlightOptions =
                [
                new() { Value = Guid.NewGuid().ToString(), Text = "GA100" }
                ],
            };
            TicketService service = new(dbContext, lookupService);
            TicketFormViewModel model = await service.GetCreateModelAsync();
            Assert.That(model.Flights.Select(f => f.Text), Is.EqualTo(new[] { "GA100" }));
        }
        [Test]
        public async Task CreateAsync_ShouldNormalizeSeatNumberAndSetPurchaseDate()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            ApplicationUser user = new() { Id = "user-id", UserName = "user@goair.local", Email = "user@goair.local" };
            Flight flight = new()
            {
                Id = Guid.NewGuid(),
                FlightNumber = "GA100",
                DepartureTime = DateTime.UtcNow.AddDays(1),
                ArrivalTime = DateTime.UtcNow.AddDays(1).AddHours(2),
                Status = FlightStatus.OnTime,
            };
            dbContext.ApplicationUsers.Add(user);
            dbContext.Flights.Add(flight);
            await dbContext.SaveChangesAsync();
            TicketService service = new(dbContext, new TestLookupService());
            ServiceResult result = await service.CreateAsync(new TicketFormViewModel
            {
                SeatNumber = " 12a ",
                Price = 99,
                FareClass = FareClass.Economy,
                FlightId = flight.Id,
            }, user.Id);
            Ticket storedTicket = await dbContext.Tickets.SingleAsync();
            Assert.That(result.Succeeded, Is.True);
            Assert.That(storedTicket.SeatNumber, Is.EqualTo("12A"));
            Assert.That(storedTicket.PurchasedOn, Is.Not.EqualTo(default(DateTime)));
        }
        [Test]
        public async Task CreateAsync_ShouldRejectDuplicateSeatAndFuturePurchaseDate()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            ApplicationUser user = new() { Id = "user-id", UserName = "user@goair.local", Email = "user@goair.local" };
            Flight flight = new()
            {
                Id = Guid.NewGuid(),
                FlightNumber = "GA101",
                DepartureTime = DateTime.UtcNow.AddDays(1),
                ArrivalTime = DateTime.UtcNow.AddDays(1).AddHours(2),
                Status = FlightStatus.OnTime,
            };
            dbContext.ApplicationUsers.Add(user);
            dbContext.Flights.Add(flight);
            dbContext.Tickets.Add(new Ticket
            {
                Id = Guid.NewGuid(),
                SeatNumber = "12A",
                Price = 150,
                FareClass = FareClass.Business,
                PurchasedOn = DateTime.UtcNow,
                UserId = user.Id,
                FlightId = flight.Id,
            });
            await dbContext.SaveChangesAsync();
            TicketService service = new(dbContext, new TestLookupService());
            ServiceResult result = await service.CreateAsync(new TicketFormViewModel
            {
                SeatNumber = "12a",
                Price = 100,
                FareClass = FareClass.Economy,
                PurchasedOn = DateTime.UtcNow.AddHours(2),
                FlightId = flight.Id,
            }, user.Id);
            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Errors[nameof(TicketFormViewModel.SeatNumber)], Does.Contain("This seat is already taken for the selected flight."));
            Assert.That(result.Errors[nameof(TicketFormViewModel.PurchasedOn)], Does.Contain("Purchase date cannot be in the future."));
        }
        [Test]
        public async Task UpdateAsync_ShouldUpdateOwnedTicket()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            ApplicationUser user = new() { Id = "user-id", UserName = "user@goair.local", Email = "user@goair.local" };
            Flight originalFlight = new() { Id = Guid.NewGuid(), FlightNumber = "GA200", DepartureTime = DateTime.UtcNow, ArrivalTime = DateTime.UtcNow.AddHours(2), Status = FlightStatus.OnTime };
            Flight newFlight = new() { Id = Guid.NewGuid(), FlightNumber = "GA201", DepartureTime = DateTime.UtcNow.AddDays(1), ArrivalTime = DateTime.UtcNow.AddDays(1).AddHours(2), Status = FlightStatus.Delayed };
            Ticket ticket = new()
            {
                Id = Guid.NewGuid(),
                SeatNumber = "14C",
                Price = 80,
                FareClass = FareClass.Economy,
                PurchasedOn = DateTime.UtcNow.AddDays(-1),
                UserId = user.Id,
                FlightId = originalFlight.Id,
            };
            dbContext.ApplicationUsers.Add(user);
            dbContext.Flights.AddRange(originalFlight, newFlight);
            dbContext.Tickets.Add(ticket);
            await dbContext.SaveChangesAsync();
            TicketService service = new(dbContext, new TestLookupService());
            ServiceResult result = await service.UpdateAsync(new TicketFormViewModel
            {
                Id = ticket.Id,
                SeatNumber = " 15d ",
                Price = 110,
                FareClass = FareClass.Business,
                PurchasedOn = DateTime.UtcNow,
                FlightId = newFlight.Id,
            }, user.Id, false);
            Ticket storedTicket = await dbContext.Tickets.SingleAsync();
            Assert.That(result.Succeeded, Is.True);
            Assert.That(storedTicket.SeatNumber, Is.EqualTo("15D"));
            Assert.That(storedTicket.Price, Is.EqualTo(110));
            Assert.That(storedTicket.FareClass, Is.EqualTo(FareClass.Business));
            Assert.That(storedTicket.FlightId, Is.EqualTo(newFlight.Id));
        }
        [Test]
        public async Task UpdateAsync_ShouldReturnMissing_WhenTicketIsNotEditable()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            ApplicationUser owner = new() { Id = "owner-id", UserName = "owner@goair.local", Email = "owner@goair.local" };
            ApplicationUser intruder = new() { Id = "intruder-id", UserName = "intruder@goair.local", Email = "intruder@goair.local" };
            Flight flight = new() { Id = Guid.NewGuid(), FlightNumber = "GA202", DepartureTime = DateTime.UtcNow, ArrivalTime = DateTime.UtcNow.AddHours(2), Status = FlightStatus.OnTime };
            Ticket ticket = new()
            {
                Id = Guid.NewGuid(),
                SeatNumber = "10A",
                Price = 120,
                FareClass = FareClass.Economy,
                PurchasedOn = DateTime.UtcNow,
                UserId = owner.Id,
                FlightId = flight.Id,
            };
            dbContext.ApplicationUsers.AddRange(owner, intruder);
            dbContext.Flights.Add(flight);
            dbContext.Tickets.Add(ticket);
            await dbContext.SaveChangesAsync();
            TicketService service = new(dbContext, new TestLookupService());
            ServiceResult result = await service.UpdateAsync(new TicketFormViewModel
            {
                Id = ticket.Id,
                SeatNumber = "11A",
                Price = 130,
                FareClass = FareClass.Business,
                PurchasedOn = DateTime.UtcNow,
                FlightId = flight.Id,
            }, intruder.Id, false);
            Assert.That(result.NotFound, Is.True);
        }
        [Test]
        public async Task DeleteAsync_ShouldRemoveOwnedTicket()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            ApplicationUser user = new() { Id = "user-id", UserName = "user@goair.local", Email = "user@goair.local" };
            Flight flight = new() { Id = Guid.NewGuid(), FlightNumber = "GA203", DepartureTime = DateTime.UtcNow, ArrivalTime = DateTime.UtcNow.AddHours(2), Status = FlightStatus.OnTime };
            Ticket ticket = new()
            {
                Id = Guid.NewGuid(),
                SeatNumber = "20B",
                Price = 95,
                FareClass = FareClass.Economy,
                PurchasedOn = DateTime.UtcNow,
                UserId = user.Id,
                FlightId = flight.Id,
            };
            dbContext.ApplicationUsers.Add(user);
            dbContext.Flights.Add(flight);
            dbContext.Tickets.Add(ticket);
            await dbContext.SaveChangesAsync();
            TicketService service = new(dbContext, new TestLookupService());
            ServiceResult result = await service.DeleteAsync(ticket.Id, user.Id, false);
            Assert.That(result.Succeeded, Is.True);
            Assert.That(await dbContext.Tickets.CountAsync(), Is.EqualTo(0));
        }
    }
}