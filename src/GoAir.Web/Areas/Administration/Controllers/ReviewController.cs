namespace GoAir.Web.Areas.Administration.Controllers
{
    using System.Security.Claims;

    using GCommon;
    using Services.Common;
    using Services.Core.Contracts;
    using ViewModels.Review;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Area("Administration")]
    [Authorize(Roles = ApplicationRoles.Administrator)]
    public class ReviewController(IReviewService reviewService) : Controller
    {
        public async Task<IActionResult> Index(string? searchTerm, int page = 1)
        {
            return View("~/Views/Review/Index.cshtml", await reviewService.GetAllAsync(GetCurrentUserId(), true, searchTerm, page));
        }
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            return NotFound();
            ReviewViewModel? review = await reviewService.GetByIdAsync(id.Value, GetCurrentUserId(), true);
            return review == null ? NotFound() : View("~/Views/Review/Details.cshtml", review);
        }
        public async Task<IActionResult> Create()
        {
            return View("~/Views/Review/Create.cshtml", await reviewService.GetCreateModelAsync());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReviewFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await reviewService.PopulateFormOptionsAsync(model);
                return View("~/Views/Review/Create.cshtml", model);
            }
            ServiceResult result = await reviewService.CreateAsync(model, GetCurrentUserId());
            if (!result.Succeeded)
            {
                ApplyErrors(result);
                await reviewService.PopulateFormOptionsAsync(model);
                return View("~/Views/Review/Create.cshtml", model);
            }
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            return NotFound();
            ReviewFormViewModel? model = await reviewService.GetForEditAsync(id.Value, GetCurrentUserId(), true);
            return model == null ? NotFound() : View("~/Views/Review/Edit.cshtml", model);
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
                return View("~/Views/Review/Edit.cshtml", model);
            }
            ServiceResult result = await reviewService.UpdateAsync(model, GetCurrentUserId(), true);
            if (result.NotFound)
            return NotFound();
            if (!result.Succeeded)
            {
                ApplyErrors(result);
                await reviewService.PopulateFormOptionsAsync(model);
                return View("~/Views/Review/Edit.cshtml", model);
            }
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            return NotFound();
            ReviewViewModel? review = await reviewService.GetForDeleteAsync(id.Value, GetCurrentUserId(), true);
            return review == null ? NotFound() : View("~/Views/Review/Delete.cshtml", review);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            ServiceResult result = await reviewService.DeleteAsync(id, GetCurrentUserId(), true);
            if (result.NotFound)
            return NotFound();
            if (!result.Succeeded)
            {
                ApplyErrors(result);
                ReviewViewModel? review = await reviewService.GetForDeleteAsync(id, GetCurrentUserId(), true);
                return review == null ? NotFound() : View("~/Views/Review/Delete.cshtml", review);
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