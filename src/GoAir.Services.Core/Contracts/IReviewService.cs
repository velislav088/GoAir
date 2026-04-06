namespace GoAir.Services.Core.Contracts
{
    using Common;
    using Web.ViewModels.Review;

    public interface IReviewService
    {
        Task<IEnumerable<ReviewViewModel>> GetAllAsync();

        Task<ReviewViewModel?> GetByIdAsync(Guid id);

        Task<ReviewFormViewModel> GetCreateModelAsync();

        Task<ServiceResult> CreateAsync(ReviewFormViewModel model);

        Task<ReviewFormViewModel?> GetForEditAsync(Guid id);

        Task<ServiceResult> UpdateAsync(ReviewFormViewModel model);

        Task<ReviewViewModel?> GetForDeleteAsync(Guid id);

        Task<ServiceResult> DeleteAsync(Guid id);

        Task PopulateFormOptionsAsync(ReviewFormViewModel model);
    }
}