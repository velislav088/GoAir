namespace GoAir.Web.Areas.Administration.Controllers
{
    using GCommon;
    using Services.Common;
    using Services.Core.Contracts;
    using ViewModels.Flight;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Area("Administration")]
    [Authorize(Roles = ApplicationRoles.Administrator)]
    public class FlightController(IFlightService flightService) : Controller
    {
        public async Task<IActionResult> Index(string? searchTerm, string? sortOrder, int page = 1)
        {
            return View("~/Views/Flight/Index.cshtml", await flightService.GetAllAsync(searchTerm, sortOrder, page, true));
        }
        public async Task<IActionResult> Create()
        {
            return View("~/Views/Flight/Create.cshtml", await flightService.GetCreateModelAsync());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FlightFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await flightService.PopulateFormOptionsAsync(model);
                return View("~/Views/Flight/Create.cshtml", model);
            }
            ServiceResult result = await flightService.CreateAsync(model);
            if (!result.Succeeded)
            {
                ApplyErrors(result);
                await flightService.PopulateFormOptionsAsync(model);
                return View("~/Views/Flight/Create.cshtml", model);
            }
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            return NotFound();
            FlightFormViewModel? model = await flightService.GetForEditAsync(id.Value);
            return model == null ? NotFound() : View("~/Views/Flight/Edit.cshtml", model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, FlightFormViewModel model)
        {
            if (id != model.Id)
            return NotFound();
            if (!ModelState.IsValid)
            {
                await flightService.PopulateFormOptionsAsync(model);
                return View("~/Views/Flight/Edit.cshtml", model);
            }
            ServiceResult result = await flightService.UpdateAsync(model);
            if (result.NotFound)
            return NotFound();
            if (!result.Succeeded)
            {
                ApplyErrors(result);
                await flightService.PopulateFormOptionsAsync(model);
                return View("~/Views/Flight/Edit.cshtml", model);
            }
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            return NotFound();
            FlightViewModel? flight = await flightService.GetForDeleteAsync(id.Value);
            return flight == null ? NotFound() : View("~/Views/Flight/Delete.cshtml", flight);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            ServiceResult result = await flightService.DeleteAsync(id);
            if (result.NotFound)
            return NotFound();
            if (!result.Succeeded)
            {
                ApplyErrors(result);
                FlightViewModel? flight = await flightService.GetForDeleteAsync(id);
                return flight == null ? NotFound() : View("~/Views/Flight/Delete.cshtml", flight);
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