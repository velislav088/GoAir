namespace GoAir.Web.Areas.Administration.Controllers
{
    using GCommon;
    using Services.Common;
    using Services.Core.Contracts;
    using ViewModels.Aircraft;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Area("Administration")]
    [Authorize(Roles = ApplicationRoles.Administrator)]
    public class AircraftController(IAircraftService aircraftService) : Controller
    {
        public async Task<IActionResult> Index(string? searchTerm, int page = 1)
        {
            return View("~/Views/Aircraft/Index.cshtml", await aircraftService.GetAllAsync(searchTerm, page, true));
        }
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            return NotFound();
            AircraftViewModel? aircraft = await aircraftService.GetByIdAsync(id.Value);
            return aircraft == null ? NotFound() : View("~/Views/Aircraft/Details.cshtml", aircraft);
        }
        public IActionResult Create()
        {
            return View("~/Views/Aircraft/Create.cshtml", new AircraftViewModel());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AircraftViewModel aircraftViewModel)
        {
            if (!ModelState.IsValid)
            return View("~/Views/Aircraft/Create.cshtml", aircraftViewModel);
            ServiceResult result = await aircraftService.CreateAsync(aircraftViewModel);
            if (!result.Succeeded)
            {
                ApplyErrors(result);
                return View("~/Views/Aircraft/Create.cshtml", aircraftViewModel);
            }
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            return NotFound();
            AircraftViewModel? aircraft = await aircraftService.GetForEditAsync(id.Value);
            return aircraft == null ? NotFound() : View("~/Views/Aircraft/Edit.cshtml", aircraft);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, AircraftViewModel aircraftViewModel)
        {
            if (id != aircraftViewModel.Id)
            return NotFound();
            if (!ModelState.IsValid)
            return View("~/Views/Aircraft/Edit.cshtml", aircraftViewModel);
            ServiceResult result = await aircraftService.UpdateAsync(aircraftViewModel);
            if (result.NotFound)
            return NotFound();
            if (!result.Succeeded)
            {
                ApplyErrors(result);
                return View("~/Views/Aircraft/Edit.cshtml", aircraftViewModel);
            }
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            return NotFound();
            AircraftViewModel? aircraft = await aircraftService.GetByIdAsync(id.Value);
            return aircraft == null ? NotFound() : View("~/Views/Aircraft/Delete.cshtml", aircraft);
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
                return aircraft == null ? NotFound() : View("~/Views/Aircraft/Delete.cshtml", aircraft);
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