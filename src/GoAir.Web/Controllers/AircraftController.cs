namespace GoAir.Web.Controllers
{
    using Services.Common;
    using Services.Core.Contracts;
    using ViewModels.Aircraft;

    using Microsoft.AspNetCore.Mvc;

    public class AircraftController(IAircraftService aircraftService) : Controller
    {
        public async Task<IActionResult> Index()
        {
            return View(await aircraftService.GetAllAsync());
        }

        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
                return NotFound();

            AircraftViewModel? aircraft = await aircraftService.GetByIdAsync(id.Value);
            return aircraft == null ? NotFound() : View(aircraft);
        }

        public IActionResult Create()
        {
            return View(new AircraftViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AircraftViewModel aircraftViewModel)
        {
            if (!ModelState.IsValid)
                return View(aircraftViewModel);

            ServiceResult result = await aircraftService.CreateAsync(aircraftViewModel);
            if (!result.Succeeded)
            {
                ApplyErrors(result);
                return View(aircraftViewModel);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
                return NotFound();

            AircraftViewModel? aircraft = await aircraftService.GetForEditAsync(id.Value);
            return aircraft == null ? NotFound() : View(aircraft);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, AircraftViewModel aircraftViewModel)
        {
            if (id != aircraftViewModel.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(aircraftViewModel);

            ServiceResult result = await aircraftService.UpdateAsync(aircraftViewModel);
            if (result.NotFound)
                return NotFound();

            if (!result.Succeeded)
            {
                ApplyErrors(result);
                return View(aircraftViewModel);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
                return NotFound();

            AircraftViewModel? aircraft = await aircraftService.GetByIdAsync(id.Value);
            return aircraft == null ? NotFound() : View(aircraft);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            ServiceResult result = await aircraftService.DeleteAsync(id);
            if (result.NotFound)
                return NotFound();

            if (!result.Succeeded)
            {
                ApplyErrors(result);
                AircraftViewModel? aircraft = await aircraftService.GetByIdAsync(id);
                return aircraft == null ? NotFound() : View("Delete", aircraft);
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