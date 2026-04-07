namespace GoAir.Services.Core.Services
{
    using Common;
    using Contracts;
    using Data;
    using Data.Models;
    using Web.ViewModels.Ticket;

    using Microsoft.EntityFrameworkCore;

    public class TicketService(ApplicationDbContext context, ILookupService lookupService) : ITicketService
    {
        public async Task<TicketIndexViewModel> GetAllAsync(string userId, bool isAdmin, string? searchTerm, int page)
        {
            const int PageSize = 6;
            IQueryable<Ticket> query = context.Tickets
            .AsNoTracking()
            .Include(t => t.Flight)
            .Include(t => t.User);
            if (!isAdmin)
            {
                query = query.Where(t => t.UserId == userId);
            }
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string normalizedSearch = searchTerm.Trim();
                query = query.Where(t =>
                t.SeatNumber.Contains(normalizedSearch) ||
                t.Flight.FlightNumber.Contains(normalizedSearch) ||
                (t.User.UserName ?? string.Empty).Contains(normalizedSearch) ||
                (t.User.Email ?? string.Empty).Contains(normalizedSearch) ||
                t.FareClass.ToString().Contains(normalizedSearch));
            }
            query = query.OrderByDescending(t => t.PurchasedOn);
            int totalTickets = await query.CountAsync();
            int totalPages = Math.Max(1, (int)Math.Ceiling(totalTickets / (double)PageSize));
            int currentPage = Math.Clamp(page, 1, totalPages);
            List<TicketViewModel> tickets = await query
            .Skip((currentPage - 1) * PageSize)
            .Take(PageSize)
            .Select(t => new TicketViewModel
            {
                Id = t.Id,
                SeatNumber = t.SeatNumber,
                Price = t.Price,
                FareClass = t.FareClass.ToString(),
                PurchasedOn = t.PurchasedOn,
                User = t.User.UserName ?? t.User.Email ?? t.User.Id,
                UserId = t.UserId,
                Flight = t.Flight.FlightNumber,
            })
            .ToListAsync();
            return new TicketIndexViewModel
            {
                SearchTerm = searchTerm?.Trim() ?? string.Empty,
                CurrentPage = currentPage,
                TotalPages = totalPages,
                IsAdmin = isAdmin,
                CurrentUserId = userId,
                Tickets = tickets,
            };
        }
        public async Task<TicketViewModel?> GetByIdAsync(Guid id, string userId, bool isAdmin)
        {
            Ticket? ticket = await GetTicketEntityByIdAsync(id, userId, isAdmin);
            return ticket == null ? null : MapTicketViewModel(ticket);
        }

        public async Task<TicketFormViewModel> GetCreateModelAsync()
        {
            TicketFormViewModel model = new();
            await PopulateFormOptionsAsync(model);
            return model;
        }
        public async Task<ServiceResult> CreateAsync(TicketFormViewModel model, string userId)
        {
            ServiceResult validation = await ValidateAsync(model, userId);
            if (!validation.Succeeded)
                return validation;

            Ticket ticket = new()
            {
                Id = Guid.NewGuid(),
                SeatNumber = model.SeatNumber,
                Price = model.Price,
                FareClass = model.FareClass,
                PurchasedOn = model.PurchasedOn ?? DateTime.UtcNow,
                UserId = userId,
                FlightId = model.FlightId,
            };

            context.Tickets.Add(ticket);
            await context.SaveChangesAsync();
            return ServiceResult.Success();
        }
        public async Task<TicketFormViewModel?> GetForEditAsync(Guid id, string userId, bool isAdmin)
        {
            Ticket? ticket = await GetEditableTicketAsync(id, userId, isAdmin);
            if (ticket == null)
                return null;

            TicketFormViewModel model = new()
            {
                Id = ticket.Id,
                SeatNumber = ticket.SeatNumber,
                Price = ticket.Price,
                FareClass = ticket.FareClass,
                PurchasedOn = ticket.PurchasedOn,
                FlightId = ticket.FlightId,
            };

            await PopulateFormOptionsAsync(model);
            return model;
        }
        public async Task<ServiceResult> UpdateAsync(TicketFormViewModel model, string userId, bool isAdmin)
        {
            Ticket? ticket = await GetEditableTicketAsync(model.Id, userId, isAdmin);
            if (ticket == null)
                return ServiceResult.Missing();
            ServiceResult validation = await ValidateAsync(model, userId);
            if (!validation.Succeeded)
                return validation;
            ticket.SeatNumber = model.SeatNumber;
            ticket.Price = model.Price;
            ticket.FareClass = model.FareClass;
            ticket.PurchasedOn = model.PurchasedOn ?? ticket.PurchasedOn;
            ticket.FlightId = model.FlightId;
            await context.SaveChangesAsync();
            return ServiceResult.Success();
        }
        public Task<TicketViewModel?> GetForDeleteAsync(Guid id, string userId, bool isAdmin)
        {
            return GetByIdAsync(id, userId, isAdmin);
        }
        public async Task<ServiceResult> DeleteAsync(Guid id, string userId, bool isAdmin)
        {
            Ticket? ticket = await GetEditableTicketAsync(id, userId, isAdmin);
            if (ticket == null)
                return ServiceResult.Missing();

            context.Tickets.Remove(ticket);
            await context.SaveChangesAsync();
            return ServiceResult.Success();
        }

        public async Task PopulateFormOptionsAsync(TicketFormViewModel model)
        {
            model.Flights = await lookupService.GetFlightOptionsAsync();
        }
        private async Task<Ticket?> GetTicketEntityByIdAsync(Guid id, string userId, bool isAdmin)
        {
            IQueryable<Ticket> query = context.Tickets
            .AsNoTracking()
            .Include(t => t.Flight)
            .Include(t => t.User)
            .Where(t => t.Id == id);
            if (!isAdmin)
            {
                query = query.Where(t => t.UserId == userId);
            }
            return await query.FirstOrDefaultAsync();
        }
        private async Task<Ticket?> GetEditableTicketAsync(Guid id, string userId, bool isAdmin)
        {
            IQueryable<Ticket> query = context.Tickets.Where(t => t.Id == id);
            if (!isAdmin)
            {
                query = query.Where(t => t.UserId == userId);
            }
            return await query.FirstOrDefaultAsync();
        }
        private async Task<ServiceResult> ValidateAsync(TicketFormViewModel model, string userId)
        {
            ServiceResult result = ServiceResult.Success();

            if (string.IsNullOrWhiteSpace(model.SeatNumber))
                result.AddError(nameof(model.SeatNumber), "Seat number is required.");
            else
                model.SeatNumber = model.SeatNumber.Trim().ToUpperInvariant();

            if (model.FlightId == Guid.Empty || !await context.Flights.AnyAsync(f => f.Id == model.FlightId))
                result.AddError(nameof(model.FlightId), "A valid flight is required.");

            bool duplicateSeat = await context.Tickets.AnyAsync(t =>
            t.FlightId == model.FlightId &&
            t.SeatNumber == model.SeatNumber &&
            t.Id != model.Id);
            if (duplicateSeat)
                result.AddError(nameof(model.SeatNumber), "This seat is already taken for the selected flight.");
            bool validUser = await context.ApplicationUsers.AnyAsync(u => u.Id == userId);
            if (!validUser)
                result.AddError(string.Empty, "You must be logged in with a valid account to manage tickets.");
            if (model.PurchasedOn.HasValue && model.PurchasedOn.Value > DateTime.UtcNow.AddMinutes(1))
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
                UserId = ticket.UserId,
                Flight = ticket.Flight.FlightNumber,
            };
        }
    }
}