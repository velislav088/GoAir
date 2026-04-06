namespace GoAir.Web.Controllers
{
    using Services.Common;
    using Services.Core.Contracts;
    using ViewModels.Airport;

    using Microsoft.AspNetCore.Mvc;

    public class AirportController(IAirportService airportService) : Controller
    {
        public async Task<IActionResult> Index()
        {
            return View(await airportService.GetAllAsync());
        }

        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
                return NotFound();

            AirportViewModel? airport = await airportService.GetByIdAsync(id.Value);
            return airport == null ? NotFound() : View(airport);
        }

        public IActionResult Create()
        {
            return View(new AirportViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AirportViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            ServiceResult result = await airportService.CreateAsync(model);
            if (!result.Succeeded)
            {
                ApplyErrors(result);
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
                return NotFound();

            AirportViewModel? airport = await airportService.GetForEditAsync(id.Value);
            return airport == null ? NotFound() : View(airport);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, AirportViewModel model)
        {
            if (id != model.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(model);

            ServiceResult result = await airportService.UpdateAsync(model);
            if (result.NotFound)
                return NotFound();

            if (!result.Succeeded)
            {
                ApplyErrors(result);
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
                return NotFound();

            AirportViewModel? airport = await airportService.GetByIdAsync(id.Value);
            return airport == null ? NotFound() : View(airport);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            ServiceResult result = await airportService.DeleteAsync(id);
            if (result.NotFound)
                return NotFound();

            if (!result.Succeeded)
            {
                ApplyErrors(result);
                AirportViewModel? airport = await airportService.GetByIdAsync(id);
                return airport == null ? NotFound() : View("Delete", airport);
            }

            return RedirectToAction(nameof(Index));
        }

        private void ApplyErrors(ServiceResult result)
        {
            foreach ((string key, IReadOnlyCollection<string> messages) in result.Errors)
            {
                foreach (string message in messages)
                    ModelState.AddModelError(key, message);
            }
        }
    }
}