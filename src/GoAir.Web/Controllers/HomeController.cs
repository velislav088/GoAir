namespace GoAir.Web.Controllers
{
    using System.Diagnostics;

    using Data;
    using ViewModels;
    using ViewModels.Home;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    public class HomeController(ApplicationDbContext dbContext) : Controller
    {
        public async Task<IActionResult> Index()
        {
            DateTime now = DateTime.UtcNow;
            HomeLandingViewModel model = new()
            {
                AirportCount = await dbContext.Airports.CountAsync(),
                AircraftCount = await dbContext.Aircraft.CountAsync(),
                ScheduledFlightCount = await dbContext.Flights.CountAsync(),
                TicketCount = await dbContext.Tickets.CountAsync(),
                ReviewCount = await dbContext.Reviews.CountAsync(),
                UpcomingFlights = await dbContext.Flights
                .AsNoTracking()
                .Include(f => f.Aircraft)
                .Include(f => f.ArrivalAirport)
                .Include(f => f.DepartureAirport)
                .Where(f => f.DepartureTime >= now)
                .OrderBy(f => f.DepartureTime)
                .Take(4)
                .Select(f => new ViewModels.Flight.FlightViewModel
                {
                    Id = f.Id,
                    FlightNumber = f.FlightNumber,
                    DepartureTime = f.DepartureTime,
                    ArrivalTime = f.ArrivalTime,
                    Status = f.Status.ToString(),
                    DepartureAirport = $"{f.DepartureAirport.City} ({f.DepartureAirport.IATACode})",
                    ArrivalAirport = $"{f.ArrivalAirport.City} ({f.ArrivalAirport.IATACode})",
                    Aircraft = $"{f.Aircraft.Manufacturer} {f.Aircraft.Model}",
                })
                .ToListAsync(),
            };
            return View(model);
        }

        public IActionResult Privacy() => View();
        [ActionName("NotFoundPage")]
        public IActionResult NotFoundPage() => View("NotFound");
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}