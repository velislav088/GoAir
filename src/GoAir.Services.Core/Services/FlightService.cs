namespace GoAir.Services.Core.Services
{
    using Data;
    using Data.Models;
    using Common;
    using Contracts;
    using Web.ViewModels.Flight;

    using Microsoft.EntityFrameworkCore;
    public class FlightService(ApplicationDbContext context, ILookupService lookupService) : IFlightService
    {
        public async Task<IEnumerable<FlightViewModel>> GetAllAsync()
        {
            return await context.Flights
                .AsNoTracking()
                .Include(f => f.Aircraft)
                .Include(f => f.ArrivalAirport)
                .Include(f => f.DepartureAirport)
                .Select(f => new FlightViewModel
                {
                    Id = f.Id,
                    FlightNumber = f.FlightNumber,
                    DepartureTime = f.DepartureTime,
                    ArrivalTime = f.ArrivalTime,
                    Status = f.Status.ToString(),
                    DepartureAirport = f.DepartureAirport.City,
                    ArrivalAirport = f.ArrivalAirport.City,
                    Aircraft = $"{f.Aircraft.Manufacturer} {f.Aircraft.Model}",
                })
                .ToListAsync();
        }

        public async Task<FlightViewModel?> GetByIdAsync(Guid id)
        {
            Flight? flight = await GetFlightEntityByIdAsync(id);
            return flight == null ? null : MapFlightViewModel(flight);
        }

        public async Task<FlightFormViewModel> GetCreateModelAsync()
        {
            DateTime defaultDeparture = DateTime.Now.AddHours(1);
            DateTime defaultArrival = defaultDeparture.AddHours(2);

            FlightFormViewModel model = new()
            {
                DepartureTime = defaultDeparture,
                ArrivalTime = defaultArrival,
                Status = FlightStatus.OnTime,
            };
            await PopulateFormOptionsAsync(model);
            return model;
        }

        public async Task<ServiceResult> CreateAsync(FlightFormViewModel model)
        {
            ServiceResult validation = await ValidateAsync(model);
            if (!validation.Succeeded)
                return validation;

            Flight flight = new()
            {
                Id = Guid.NewGuid(),
                FlightNumber = model.FlightNumber,
                DepartureTime = model.DepartureTime,
                ArrivalTime = model.ArrivalTime,
                Status = model.Status,
                DepartureAirportId = model.DepartureAirportId,
                ArrivalAirportId = model.ArrivalAirportId,
                AircraftId = model.AircraftId,
            };

            context.Flights.Add(flight);
            await context.SaveChangesAsync();
            return ServiceResult.Success();
        }

        public async Task<FlightFormViewModel?> GetForEditAsync(Guid id)
        {
            Flight? flight = await context.Flights.FindAsync(id);
            if (flight == null)
                return null;

            FlightFormViewModel model = new()
            {
                Id = flight.Id,
                FlightNumber = flight.FlightNumber,
                DepartureTime = flight.DepartureTime,
                ArrivalTime = flight.ArrivalTime,
                Status = flight.Status,
                DepartureAirportId = flight.DepartureAirportId,
                ArrivalAirportId = flight.ArrivalAirportId,
                AircraftId = flight.AircraftId,
            };

            await PopulateFormOptionsAsync(model);
            return model;
        }

        public async Task<ServiceResult> UpdateAsync(FlightFormViewModel model)
        {
            bool exists = await context.Flights.AnyAsync(f => f.Id == model.Id);
            if (!exists)
                return ServiceResult.Missing();

            ServiceResult validation = await ValidateAsync(model);
            if (!validation.Succeeded)
                return validation;

            Flight flight = new()
            {
                Id = model.Id,
                FlightNumber = model.FlightNumber,
                DepartureTime = model.DepartureTime,
                ArrivalTime = model.ArrivalTime,
                Status = model.Status,
                DepartureAirportId = model.DepartureAirportId,
                ArrivalAirportId = model.ArrivalAirportId,
                AircraftId = model.AircraftId,
            };

            context.Flights.Update(flight);
            await context.SaveChangesAsync();
            return ServiceResult.Success();
        }

        public Task<FlightViewModel?> GetForDeleteAsync(Guid id)
        {
            return GetByIdAsync(id);
        }

        public async Task<ServiceResult> DeleteAsync(Guid id)
        {
            Flight? flight = await context.Flights.FindAsync(id);
            if (flight == null)
                return ServiceResult.Missing();

            bool hasTickets = await context.Tickets.AnyAsync(t => t.FlightId == id);
            bool hasReviews = await context.Reviews.AnyAsync(r => r.FlightId == id);
            if (hasTickets || hasReviews)
                return ServiceResult.Failure(string.Empty, "Flight cannot be deleted while tickets or reviews exist for it.");

            context.Flights.Remove(flight);
            await context.SaveChangesAsync();
            return ServiceResult.Success();
        }

        public async Task PopulateFormOptionsAsync(FlightFormViewModel model)
        {
            IEnumerable<GoAir.Web.ViewModels.Common.LookupOptionViewModel> airports = await lookupService.GetAirportOptionsAsync();
            model.DepartureAirports = airports;
            model.ArrivalAirports = airports;
            model.AircraftOptions = await lookupService.GetAircraftOptionsAsync();
        }

        private async Task<Flight?> GetFlightEntityByIdAsync(Guid id)
        {
            return await context.Flights
                .AsNoTracking()
                .Include(f => f.Aircraft)
                .Include(f => f.ArrivalAirport)
                .Include(f => f.DepartureAirport)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        private async Task<ServiceResult> ValidateAsync(FlightFormViewModel model)
        {
            ServiceResult result = ServiceResult.Success();

            if (string.IsNullOrWhiteSpace(model.FlightNumber))
                result.AddError(nameof(model.FlightNumber), "Flight number is required.");
            else
            {
                model.FlightNumber = model.FlightNumber.Trim().ToUpperInvariant();

                bool duplicateFlightNumber = await context.Flights.AnyAsync(f => f.FlightNumber == model.FlightNumber && f.Id != model.Id);
                if (duplicateFlightNumber)
                {
                    result.AddError(nameof(model.FlightNumber), "This flight number is already in use.");
                }
            }

            if (model.DepartureAirportId == Guid.Empty || !await context.Airports.AnyAsync(a => a.Id == model.DepartureAirportId))
                result.AddError(nameof(model.DepartureAirportId), "A valid departure airport is required.");

            if (model.ArrivalAirportId == Guid.Empty || !await context.Airports.AnyAsync(a => a.Id == model.ArrivalAirportId))
                result.AddError(nameof(model.ArrivalAirportId), "A valid arrival airport is required.");

            if (model.DepartureAirportId != Guid.Empty && model.DepartureAirportId == model.ArrivalAirportId)
                result.AddError(nameof(model.ArrivalAirportId), "Arrival airport must be different from departure airport.");

            if (model.AircraftId == Guid.Empty || !await context.Aircraft.AnyAsync(a => a.Id == model.AircraftId))
                result.AddError(nameof(model.AircraftId), "A valid aircraft is required.");
            else
            {
                bool overlappingAircraftFlight = await context.Flights.AnyAsync(f =>
                    f.AircraftId == model.AircraftId &&
                    f.Id != model.Id &&
                    f.Status != FlightStatus.Cancelled &&
                    model.DepartureTime < f.ArrivalTime &&
                    model.ArrivalTime > f.DepartureTime);

                if (overlappingAircraftFlight)
                    result.AddError(nameof(model.AircraftId), "This aircraft is already assigned to another flight in the selected time range.");
            }

            if (model.ArrivalTime <= model.DepartureTime)
                result.AddError(nameof(model.ArrivalTime), "Arrival time must be after departure time.");

            return result;
        }

        private static FlightViewModel MapFlightViewModel(Flight flight)
        {
            return new FlightViewModel
            {
                Id = flight.Id,
                FlightNumber = flight.FlightNumber,
                DepartureTime = flight.DepartureTime,
                ArrivalTime = flight.ArrivalTime,
                Status = flight.Status.ToString(),
                DepartureAirport = $"{flight.DepartureAirport.City} ({flight.DepartureAirport.IATACode})",
                ArrivalAirport = $"{flight.ArrivalAirport.City} ({flight.ArrivalAirport.IATACode})",
                Aircraft = $"{flight.Aircraft.Manufacturer} {flight.Aircraft.Model}",
            };
        }
    }
}