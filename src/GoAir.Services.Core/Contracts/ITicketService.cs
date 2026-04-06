namespace GoAir.Services.Core.Contracts
{
    using Common;
    using Web.ViewModels.Ticket;

    public interface ITicketService
    {
        Task<IEnumerable<TicketViewModel>> GetAllAsync();

        Task<TicketViewModel?> GetByIdAsync(Guid id);

        Task<TicketFormViewModel> GetCreateModelAsync();

        Task<ServiceResult> CreateAsync(TicketFormViewModel model);

        Task<TicketFormViewModel?> GetForEditAsync(Guid id);

        Task<ServiceResult> UpdateAsync(TicketFormViewModel model);

        Task<TicketViewModel?> GetForDeleteAsync(Guid id);

        Task<ServiceResult> DeleteAsync(Guid id);

        Task PopulateFormOptionsAsync(TicketFormViewModel model);
    }
}