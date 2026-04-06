namespace GoAir.Services.Core.Services
{
    using Data;
    using Data.Models;
    using Common;
    using Contracts;
    using Web.ViewModels.Ticket;

    using Microsoft.EntityFrameworkCore;

    public class TicketService(ApplicationDbContext context, ILookupService lookupService) : ITicketService
    {
        public async Task<IEnumerable<TicketViewModel>> GetAllAsync()
        {
            return await context.Tickets
                .AsNoTracking()
                .Include(t => t.Flight)
                .Include(t => t.User)
                .Select(t => new TicketViewModel
                {
                    Id = t.Id,
                    SeatNumber = t.SeatNumber,
                    Price = t.Price,
                    FareClass = t.FareClass.ToString(),
                    PurchasedOn = t.PurchasedOn,
                    User = t.User.UserName ?? t.User.Email ?? t.User.Id,
                    Flight = t.Flight.FlightNumber,
                })
                .ToListAsync();
        }

        public async Task<TicketViewModel?> GetByIdAsync(Guid id)
        {
            Ticket? ticket = await GetTicketEntityByIdAsync(id);
            return ticket == null ? null : MapTicketViewModel(ticket);
        }

        public async Task<TicketFormViewModel> GetCreateModelAsync()
        {
            TicketFormViewModel model = new();
            await PopulateFormOptionsAsync(model);
            return model;
        }

        public async Task<ServiceResult> CreateAsync(TicketFormViewModel model)
        {
            ServiceResult validation = await ValidateAsync(model);
            if (!validation.Succeeded)
                return validation;

            Ticket ticket = new()
            {
                Id = Guid.NewGuid(),
                SeatNumber = model.SeatNumber,
                Price = model.Price,
                FareClass = model.FareClass,
                PurchasedOn = model.PurchasedOn,
                UserId = model.UserId,
                FlightId = model.FlightId,
            };

            context.Tickets.Add(ticket);
            await context.SaveChangesAsync();
            return ServiceResult.Success();
        }

        public async Task<TicketFormViewModel?> GetForEditAsync(Guid id)
        {
            Ticket? ticket = await context.Tickets.FindAsync(id);
            if (ticket == null)
                return null;

            TicketFormViewModel model = new()
            {
                Id = ticket.Id,
                SeatNumber = ticket.SeatNumber,
                Price = ticket.Price,
                FareClass = ticket.FareClass,
                PurchasedOn = ticket.PurchasedOn,
                UserId = ticket.UserId,
                FlightId = ticket.FlightId,
            };

            await PopulateFormOptionsAsync(model);
            return model;
        }

        public async Task<ServiceResult> UpdateAsync(TicketFormViewModel model)
        {
            bool exists = await context.Tickets.AnyAsync(t => t.Id == model.Id);
            if (!exists)
                return ServiceResult.Missing();

            ServiceResult validation = await ValidateAsync(model);
            if (!validation.Succeeded)
                return validation;

            Ticket ticket = new()
            {
                Id = model.Id,
                SeatNumber = model.SeatNumber,
                Price = model.Price,
                FareClass = model.FareClass,
                PurchasedOn = model.PurchasedOn,
                UserId = model.UserId,
                FlightId = model.FlightId,
            };

            context.Tickets.Update(ticket);
            await context.SaveChangesAsync();
            return ServiceResult.Success();
        }

        public Task<TicketViewModel?> GetForDeleteAsync(Guid id)
        {
            return GetByIdAsync(id);
        }

        public async Task<ServiceResult> DeleteAsync(Guid id)
        {
            Ticket? ticket = await context.Tickets.FindAsync(id);
            if (ticket == null)
                return ServiceResult.Missing();

            context.Tickets.Remove(ticket);
            await context.SaveChangesAsync();
            return ServiceResult.Success();
        }

        public async Task PopulateFormOptionsAsync(TicketFormViewModel model)
        {
            model.Flights = await lookupService.GetFlightOptionsAsync();
            model.Users = await lookupService.GetUserOptionsAsync();
        }

        private async Task<Ticket?> GetTicketEntityByIdAsync(Guid id)
        {
            return await context.Tickets
                .AsNoTracking()
                .Include(t => t.Flight)
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        private async Task<ServiceResult> ValidateAsync(TicketFormViewModel model)
        {
            ServiceResult result = ServiceResult.Success();

            if (string.IsNullOrWhiteSpace(model.SeatNumber))
                result.AddError(nameof(model.SeatNumber), "Seat number is required.");
            else
                model.SeatNumber = model.SeatNumber.Trim().ToUpperInvariant();

            if (model.FlightId == Guid.Empty || !await context.Flights.AnyAsync(f => f.Id == model.FlightId))
                result.AddError(nameof(model.FlightId), "A valid flight is required.");

            if (string.IsNullOrWhiteSpace(model.UserId) || !await context.ApplicationUsers.AnyAsync(u => u.Id == model.UserId))
                result.AddError(nameof(model.UserId), "A valid user is required.");

            bool duplicateSeat = await context.Tickets.AnyAsync(t =>
                t.FlightId == model.FlightId &&
                t.SeatNumber == model.SeatNumber &&
                t.Id != model.Id);
            if (duplicateSeat)
                result.AddError(nameof(model.SeatNumber), "This seat is already taken for the selected flight.");

            if (model.PurchasedOn == default)
                model.PurchasedOn = DateTime.UtcNow;
            else if (model.PurchasedOn > DateTime.UtcNow.AddMinutes(1))
                result.AddError(nameof(model.PurchasedOn), "Purchase date cannot be in the future.");

            return result;
        }

        private static TicketViewModel MapTicketViewModel(Ticket ticket)
        {
            return new TicketViewModel
            {
                Id = ticket.Id,
                SeatNumber = ticket.SeatNumber,
                Price = ticket.Price,
                FareClass = ticket.FareClass.ToString(),
                PurchasedOn = ticket.PurchasedOn,
                User = ticket.User.UserName ?? ticket.User.Email ?? ticket.User.Id,
                Flight = ticket.Flight.FlightNumber,
            };
        }
    }
}