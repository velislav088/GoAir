namespace GoAir.Web.Controllers
{
    using Services.Common;
    using Services.Core.Contracts;
    using ViewModels.Review;

    using Microsoft.AspNetCore.Mvc;

    public class ReviewController(IReviewService reviewService) : Controller
    {
        public async Task<IActionResult> Index() => View(await reviewService.GetAllAsync());

        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
                return NotFound();

            ReviewViewModel? review = await reviewService.GetByIdAsync(id.Value);
            return review == null ? NotFound() : View(review);
        }

        public async Task<IActionResult> Create() => View(await reviewService.GetCreateModelAsync());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReviewFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await reviewService.PopulateFormOptionsAsync(model);
                return View(model);
            }

            ServiceResult result = await reviewService.CreateAsync(model);
            if (!result.Succeeded)
            {
                ApplyErrors(result);
                await reviewService.PopulateFormOptionsAsync(model);
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
                return NotFound();

            ReviewFormViewModel? model = await reviewService.GetForEditAsync(id.Value);
            return model == null ? NotFound() : View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ReviewFormViewModel model)
        {
            if (id != model.Id)
                return NotFound();

            if (!ModelState.IsValid)
            {
                await reviewService.PopulateFormOptionsAsync(model);
                return View(model);
            }

            ServiceResult result = await reviewService.UpdateAsync(model);
            if (result.NotFound)
                return NotFound();

            if (!result.Succeeded)
            {
                ApplyErrors(result);
                await reviewService.PopulateFormOptionsAsync(model);
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
                return NotFound();

            ReviewViewModel? review = await reviewService.GetForDeleteAsync(id.Value);
            return review == null ? NotFound() : View(review);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            ServiceResult result = await reviewService.DeleteAsync(id);
            if (result.NotFound)
                return NotFound();

            if (!result.Succeeded)
            {
                ApplyErrors(result);
                ReviewViewModel? review = await reviewService.GetForDeleteAsync(id);
                return review == null ? NotFound() : View("Delete", review);
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