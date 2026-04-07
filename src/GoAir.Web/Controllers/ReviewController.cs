namespace GoAir.Web.Controllers
{
    using System.Security.Claims;

    using GCommon;
    using Services.Common;
    using Services.Core.Contracts;
    using ViewModels.Review;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Authorize]
    public class ReviewController(IReviewService reviewService) : Controller
    {
        public async Task<IActionResult> Index(string? searchTerm, int page = 1) => View(await reviewService.GetAllAsync(GetCurrentUserId(), User.IsInRole(ApplicationRoles.Administrator), searchTerm, page));
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            return NotFound();
            ReviewViewModel? review = await reviewService.GetByIdAsync(id.Value, GetCurrentUserId(), User.IsInRole(ApplicationRoles.Administrator));
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
            ServiceResult result = await reviewService.CreateAsync(model, GetCurrentUserId());
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
            ReviewFormViewModel? model = await reviewService.GetForEditAsync(id.Value, GetCurrentUserId(), User.IsInRole(ApplicationRoles.Administrator));
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
            ServiceResult result = await reviewService.UpdateAsync(model, GetCurrentUserId(), User.IsInRole(ApplicationRoles.Administrator));
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
            ReviewViewModel? review = await reviewService.GetForDeleteAsync(id.Value, GetCurrentUserId(), User.IsInRole(ApplicationRoles.Administrator));
            return review == null ? NotFound() : View(review);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            ServiceResult result = await reviewService.DeleteAsync(id, GetCurrentUserId(), User.IsInRole(ApplicationRoles.Administrator));
            if (result.NotFound)
            return NotFound();

            if (!result.Succeeded)
            {
                ApplyErrors(result);
                ReviewViewModel? review = await reviewService.GetForDeleteAsync(id, GetCurrentUserId(), User.IsInRole(ApplicationRoles.Administrator));
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
        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }
    }
}