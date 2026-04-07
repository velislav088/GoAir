namespace GoAir.Services.Core.Contracts
{
    using Common;
    using Web.ViewModels.Aircraft;

    public interface IAircraftService
    {
        Task<AircraftIndexViewModel> GetAllAsync(string? searchTerm, int page, bool isAdmin);

        Task<AircraftViewModel?> GetByIdAsync(Guid id);

        Task<ServiceResult> CreateAsync(AircraftViewModel model);

        Task<AircraftViewModel?> GetForEditAsync(Guid id);

        Task<ServiceResult> UpdateAsync(AircraftViewModel model);

        Task<ServiceResult> DeleteAsync(Guid id);
    }
}