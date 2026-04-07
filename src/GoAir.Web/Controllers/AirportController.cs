namespace GoAir.Web.Controllers
{
    using GCommon;
    using Services.Core.Contracts;
    using Web.ViewModels.Airport;

    using Microsoft.AspNetCore.Mvc;

    public class AirportController(IAirportService airportService) : Controller
    {
        public async Task<IActionResult> Index(string? searchTerm, int page = 1)
        {
            return View(await airportService.GetAllAsync(searchTerm, page, User.IsInRole(ApplicationRoles.Administrator)));
        }

        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            return NotFound();

            AirportViewModel? airport = await airportService.GetByIdAsync(id.Value);
            return airport == null ? NotFound() : View(airport);
        }
    }
}