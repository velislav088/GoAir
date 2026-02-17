using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using GoAir.Data;
using GoAir.Models;
using GoAir.ViewModels;

namespace GoAir.Controllers
{
    public class FlightsController(ApplicationDbContext context) : Controller
    {
        private readonly ApplicationDbContext _context = context;

        // GET: Flights
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Flights.Include(f => f.ArrivalAirport).Include(f => f.DepartureAirport);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Flights/Details
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
                return NotFound();

            var flight = await _context.Flights
                .Include(f => f.ArrivalAirport)
                .Include(f => f.DepartureAirport)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (flight == null)
                return NotFound();

            return View(flight);
        }

        // GET: Flights/Create
        public IActionResult Create()
        {
            ViewData["ArrivalAirportId"] = new SelectList(_context.Airports, "Id", "City");
            ViewData["DepartureAirportId"] = new SelectList(_context.Airports, "Id", "City");
            ViewData["AircraftId"] = new SelectList(_context.Aircrafts, "Id", "Model");
            return View();
        }

        // POST: Flights/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateFlightViewModel model)
        {
            if (ModelState.IsValid)
            {
                var flight = new Flight
                {
                    Id = Guid.NewGuid(),
                    FlightNumber = model.FlightNumber,
                    Price = model.Price,
                    DepartureTime = model.DepartureTime,
                    Status = model.Status,
                    DepartureAirportId = model.DepartureAirportId,
                    ArrivalAirportId = model.ArrivalAirportId,
                    AircraftId = model.AircraftId
                };

                _context.Add(flight);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["ArrivalAirportId"] = new SelectList(_context.Airports, "Id", "City", model.ArrivalAirportId);
            ViewData["DepartureAirportId"] = new SelectList(_context.Airports, "Id", "City", model.DepartureAirportId);
            ViewData["AircraftId"] = new SelectList(_context.Aircrafts, "Id", "Model", model.AircraftId);
            return View(model);
        }

        // GET: Flights/Edit
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
                return NotFound();

            var flight = await _context.Flights.FindAsync(id);

            if (flight == null)
                return NotFound();

            var model = new EditFlightViewModel
            {
                Id = flight.Id,
                FlightNumber = flight.FlightNumber,
                Price = flight.Price,
                DepartureTime = flight.DepartureTime,
                Status = flight.Status,
                DepartureAirportId = flight.DepartureAirportId,
                ArrivalAirportId = flight.ArrivalAirportId,
                AircraftId = flight.AircraftId.GetValueOrDefault()
            };

            ViewData["ArrivalAirportId"] = new SelectList(_context.Airports, "Id", "City", model.ArrivalAirportId);
            ViewData["DepartureAirportId"] = new SelectList(_context.Airports, "Id", "City", model.DepartureAirportId);
            ViewData["AircraftId"] = new SelectList(_context.Aircrafts, "Id", "Model", model.AircraftId);
            return View(model);
        }


        // POST: Flights/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, EditFlightViewModel model)
        {
            if (id != model.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                var flight = await _context.Flights.FindAsync(id);

                if (flight == null)
                    return NotFound();

                flight.FlightNumber = model.FlightNumber;
                flight.Price = model.Price;
                flight.DepartureTime = model.DepartureTime;
                flight.Status = model.Status;
                flight.DepartureAirportId = model.DepartureAirportId;
                flight.ArrivalAirportId = model.ArrivalAirportId;
                flight.AircraftId = model.AircraftId;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["ArrivalAirportId"] = new SelectList(_context.Airports, "Id", "City", model.ArrivalAirportId);
            ViewData["DepartureAirportId"] = new SelectList(_context.Airports, "Id", "City", model.DepartureAirportId);
            ViewData["AircraftId"] = new SelectList(_context.Aircrafts, "Id", "Model", model.AircraftId);
            return View(model);
        }


        // GET: Flights/Delete
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
                return NotFound();

            var flight = await _context.Flights
                .Include(f => f.ArrivalAirport)
                .Include(f => f.DepartureAirport)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (flight == null)
                return NotFound();

            return View(flight);
        }

        // POST: Flights/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var flight = await _context.Flights.FindAsync(id);
            if (flight != null)
            {
                _context.Flights.Remove(flight);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool FlightExists(Guid id)
        {
            return _context.Flights.Any(e => e.Id == id);
        }
    }
}
