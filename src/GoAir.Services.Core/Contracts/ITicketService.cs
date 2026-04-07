namespace GoAir.Services.Core.Contracts
{
    using Common;
    using Web.ViewModels.Ticket;

    public interface ITicketService
    {
        Task<TicketIndexViewModel> GetAllAsync(string userId, bool isAdmin, string? searchTerm, int page);

        Task<TicketViewModel?> GetByIdAsync(Guid id, string userId, bool isAdmin);

        Task<TicketFormViewModel> GetCreateModelAsync();

        Task<ServiceResult> CreateAsync(TicketFormViewModel model, string userId);

        Task<TicketFormViewModel?> GetForEditAsync(Guid id, string userId, bool isAdmin);

        Task<ServiceResult> UpdateAsync(TicketFormViewModel model, string userId, bool isAdmin);

        Task<TicketViewModel?> GetForDeleteAsync(Guid id, string userId, bool isAdmin);

        Task<ServiceResult> DeleteAsync(Guid id, string userId, bool isAdmin);

        Task PopulateFormOptionsAsync(TicketFormViewModel model);
    }
}