namespace GoAir.Web.Controllers
{
    using System.Security.Claims;

    using GCommon;
    using Services.Common;
    using Services.Core.Contracts;
    using ViewModels.Ticket;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Authorize]
    public class TicketController(ITicketService ticketService) : Controller
    {
        public async Task<IActionResult> Index(string? searchTerm, int page = 1)
        {
            return View(await ticketService.GetAllAsync(GetCurrentUserId(), User.IsInRole(ApplicationRoles.Administrator), searchTerm, page));
        }

        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            return NotFound();
            TicketViewModel? ticket = await ticketService.GetByIdAsync(id.Value, GetCurrentUserId(), User.IsInRole(ApplicationRoles.Administrator));
            return ticket == null ? NotFound() : View(ticket);
        }

        public async Task<IActionResult> Create()
        {
            return View(await ticketService.GetCreateModelAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TicketFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await ticketService.PopulateFormOptionsAsync(model);
                return View(model);
            }
            ServiceResult result = await ticketService.CreateAsync(model, GetCurrentUserId());
            if (!result.Succeeded)
            {
                ApplyErrors(result);
                await ticketService.PopulateFormOptionsAsync(model);
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            return NotFound();
            TicketFormViewModel? model = await ticketService.GetForEditAsync(id.Value, GetCurrentUserId(), User.IsInRole(ApplicationRoles.Administrator));
            return model == null ? NotFound() : View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, TicketFormViewModel model)
        {
            if (id != model.Id)
            return NotFound();

            if (!ModelState.IsValid)
            {
                await ticketService.PopulateFormOptionsAsync(model);
                return View(model);
            }
            ServiceResult result = await ticketService.UpdateAsync(model, GetCurrentUserId(), User.IsInRole(ApplicationRoles.Administrator));
            if (result.NotFound)
            return NotFound();

            if (!result.Succeeded)
            {
                ApplyErrors(result);
                await ticketService.PopulateFormOptionsAsync(model);
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            return NotFound();
            TicketViewModel? ticket = await ticketService.GetForDeleteAsync(id.Value, GetCurrentUserId(), User.IsInRole(ApplicationRoles.Administrator));
            return ticket == null ? NotFound() : View(ticket);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            ServiceResult result = await ticketService.DeleteAsync(id, GetCurrentUserId(), User.IsInRole(ApplicationRoles.Administrator));
            if (result.NotFound)
            return NotFound();

            if (!result.Succeeded)
            {
                ApplyErrors(result);
                TicketViewModel? ticket = await ticketService.GetForDeleteAsync(id, GetCurrentUserId(), User.IsInRole(ApplicationRoles.Administrator));
                return ticket == null ? NotFound() : View("Delete", ticket);
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