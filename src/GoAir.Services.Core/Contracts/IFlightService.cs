namespace GoAir.Services.Core.Contracts
{
    using Common;
    using Web.ViewModels.Flight;

    public interface IFlightService
    {
        Task<IEnumerable<FlightViewModel>> GetAllAsync();

        Task<FlightViewModel?> GetByIdAsync(Guid id);

        Task<FlightFormViewModel> GetCreateModelAsync();

        Task<ServiceResult> CreateAsync(FlightFormViewModel model);

        Task<FlightFormViewModel?> GetForEditAsync(Guid id);

        Task<ServiceResult> UpdateAsync(FlightFormViewModel model);

        Task<FlightViewModel?> GetForDeleteAsync(Guid id);

        Task<ServiceResult> DeleteAsync(Guid id);

        Task PopulateFormOptionsAsync(FlightFormViewModel model);
    }
}