namespace GoAir.Web.Controllers
{
    using GCommon;
    using Services.Core.Contracts;
    using Web.ViewModels.Flight;
    using Web.ViewModels.Review;

    using Microsoft.AspNetCore.Mvc;

    public class FlightController(IFlightService flightService, IReviewService reviewService) : Controller
    {
        public async Task<IActionResult> Index(string? searchTerm, string? sortOrder, int page = 1)
        {
            return View(await flightService.GetAllAsync(searchTerm, sortOrder, page, User.IsInRole(ApplicationRoles.Administrator)));
        }

        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            return NotFound();

            FlightViewModel? flight = await flightService.GetByIdAsync(id.Value);
            return flight == null ? NotFound() : View(flight);
        }
        public async Task<IActionResult> ReviewsPartial(Guid id)
        {
            IEnumerable<ReviewViewModel> reviews = await reviewService.GetReviewsByFlightAsync(id);
            return PartialView("_FlightReviewsPartial", reviews);
        }
    }
}