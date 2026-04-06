namespace GoAir.Services.Core.Contracts
{
    using Common;
    using Web.ViewModels.Airport;

    public interface IAirportService
    {
        Task<IEnumerable<AirportViewModel>> GetAllAsync();

        Task<AirportViewModel?> GetByIdAsync(Guid id);

        Task<ServiceResult> CreateAsync(AirportViewModel model);

        Task<AirportViewModel?> GetForEditAsync(Guid id);

        Task<ServiceResult> UpdateAsync(AirportViewModel model);

        Task<ServiceResult> DeleteAsync(Guid id);
    }
}