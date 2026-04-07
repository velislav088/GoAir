namespace GoAir.Web.Controllers
{
    using GCommon;
    using Services.Core.Contracts;
    using ViewModels.Aircraft;

    using Microsoft.AspNetCore.Mvc;

    public class AircraftController(IAircraftService aircraftService) : Controller
    {
        public async Task<IActionResult> Index(string? searchTerm, int page = 1)
        {
            return View(await aircraftService.GetAllAsync(searchTerm, page, User.IsInRole(ApplicationRoles.Administrator)));
        }

        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            return NotFound();

            AircraftViewModel? aircraft = await aircraftService.GetByIdAsync(id.Value);
            return aircraft == null ? NotFound() : View(aircraft);
        }
    }
}