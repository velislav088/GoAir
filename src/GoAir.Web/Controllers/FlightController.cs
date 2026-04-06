namespace GoAir.Web.Controllers
{
    using Services.Common;
    using Services.Core.Contracts;
    using ViewModels.Flight;

    using Microsoft.AspNetCore.Mvc;

    public class FlightController(IFlightService flightService) : Controller
    {
        public async Task<IActionResult> Index()
        {
            return View(await flightService.GetAllAsync());
        }

        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
                return NotFound();

            FlightViewModel? flight = await flightService.GetByIdAsync(id.Value);
            return flight == null ? NotFound() : View(flight);
        }

        public async Task<IActionResult> Create()
        {
            return View(await flightService.GetCreateModelAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FlightFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await flightService.PopulateFormOptionsAsync(model);
                return View(model);
            }

            ServiceResult result = await flightService.CreateAsync(model);
            if (!result.Succeeded)
            {
                ApplyErrors(result);
                await flightService.PopulateFormOptionsAsync(model);
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
                return NotFound();

            FlightFormViewModel? model = await flightService.GetForEditAsync(id.Value);
            return model == null ? NotFound() : View(model);
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
                return View(model);
            }

            ServiceResult result = await flightService.UpdateAsync(model);
            if (result.NotFound)
                return NotFound();

            if (!result.Succeeded)
            {
                ApplyErrors(result);
                await flightService.PopulateFormOptionsAsync(model);
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
                return NotFound();

            FlightViewModel? flight = await flightService.GetForDeleteAsync(id.Value);
            return flight == null ? NotFound() : View(flight);
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
                return flight == null ? NotFound() : View("Delete", flight);
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