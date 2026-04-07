namespace GoAir.Web.Areas.Administration.Controllers
{
    using GCommon;
    using Services.Common;
    using Services.Core.Contracts;
    using ViewModels.Airport;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Area("Administration")]
    [Authorize(Roles = ApplicationRoles.Administrator)]
    public class AirportController(IAirportService airportService) : Controller
    {
        public async Task<IActionResult> Index(string? searchTerm, int page = 1)
        {
            return View("~/Views/Airport/Index.cshtml", await airportService.GetAllAsync(searchTerm, page, true));
        }
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            return NotFound();
            AirportViewModel? airport = await airportService.GetByIdAsync(id.Value);
            return airport == null ? NotFound() : View("~/Views/Airport/Details.cshtml", airport);
        }
        public IActionResult Create()
        {
            return View("~/Views/Airport/Create.cshtml", new AirportViewModel());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AirportViewModel model)
        {
            if (!ModelState.IsValid)
            return View("~/Views/Airport/Create.cshtml", model);
            ServiceResult result = await airportService.CreateAsync(model);
            if (!result.Succeeded)
            {
                ApplyErrors(result);
                return View("~/Views/Airport/Create.cshtml", model);
            }
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            return NotFound();
            AirportViewModel? airport = await airportService.GetForEditAsync(id.Value);
            return airport == null ? NotFound() : View("~/Views/Airport/Edit.cshtml", airport);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, AirportViewModel model)
        {
            if (id != model.Id)
            return NotFound();
            if (!ModelState.IsValid)
            return View("~/Views/Airport/Edit.cshtml", model);
            ServiceResult result = await airportService.UpdateAsync(model);
            if (result.NotFound)
            return NotFound();
            if (!result.Succeeded)
            {
                ApplyErrors(result);
                return View("~/Views/Airport/Edit.cshtml", model);
            }
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            return NotFound();
            AirportViewModel? airport = await airportService.GetByIdAsync(id.Value);
            return airport == null ? NotFound() : View("~/Views/Airport/Delete.cshtml", airport);
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
                return airport == null ? NotFound() : View("~/Views/Airport/Delete.cshtml", airport);
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