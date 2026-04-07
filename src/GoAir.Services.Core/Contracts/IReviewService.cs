namespace GoAir.Services.Core.Contracts
{
    using Common;
    using Web.ViewModels.Review;

    public interface IReviewService
    {
        Task<ReviewIndexViewModel> GetAllAsync(string userId, bool isAdmin, string? searchTerm, int page);

        Task<ReviewViewModel?> GetByIdAsync(Guid id, string userId, bool isAdmin);

        Task<IEnumerable<ReviewViewModel>> GetReviewsByFlightAsync(Guid flightId);

        Task<ReviewFormViewModel> GetCreateModelAsync();

        Task<ServiceResult> CreateAsync(ReviewFormViewModel model, string userId);

        Task<ReviewFormViewModel?> GetForEditAsync(Guid id, string userId, bool isAdmin);

        Task<ServiceResult> UpdateAsync(ReviewFormViewModel model, string userId, bool isAdmin);

        Task<ReviewViewModel?> GetForDeleteAsync(Guid id, string userId, bool isAdmin);

        Task<ServiceResult> DeleteAsync(Guid id, string userId, bool isAdmin);

        Task PopulateFormOptionsAsync(ReviewFormViewModel model);
    }
}