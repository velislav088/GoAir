namespace GoAir.Services.Core.Services
{
    using Data;
    using Data.Models;
    using Common;
    using Contracts;
    using Web.ViewModels.Review;

    using Microsoft.EntityFrameworkCore;

    public class ReviewService(ApplicationDbContext context, ILookupService lookupService) : IReviewService
    {
        public async Task<IEnumerable<ReviewViewModel>> GetAllAsync()
        {
            return await context.Reviews
                .AsNoTracking()
                .Include(r => r.Flight)
                .Include(r => r.User)
                .Select(r => new ReviewViewModel
                {
                    Id = r.Id,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedOn = r.CreatedOn,
                    User = r.User.UserName ?? r.User.Email ?? r.User.Id,
                    Flight = r.Flight.FlightNumber,
                })
                .ToListAsync();
        }

        public async Task<ReviewViewModel?> GetByIdAsync(Guid id)
        {
            Review? review = await GetReviewEntityByIdAsync(id);
            return review == null ? null : MapReviewViewModel(review);
        }

        public async Task<ReviewFormViewModel> GetCreateModelAsync()
        {
            ReviewFormViewModel model = new();
            await PopulateFormOptionsAsync(model);
            return model;
        }

        public async Task<ServiceResult> CreateAsync(ReviewFormViewModel model)
        {
            ServiceResult validation = await ValidateAsync(model);
            if (!validation.Succeeded)
                return validation;

            Review review = new()
            {
                Id = Guid.NewGuid(),
                Rating = model.Rating,
                Comment = model.Comment,
                CreatedOn = model.CreatedOn,
                UserId = model.UserId,
                FlightId = model.FlightId,
            };

            context.Reviews.Add(review);
            await context.SaveChangesAsync();
            return ServiceResult.Success();
        }

        public async Task<ReviewFormViewModel?> GetForEditAsync(Guid id)
        {
            Review? review = await context.Reviews.FindAsync(id);
            if (review == null)
                return null;

            ReviewFormViewModel model = new()
            {
                Id = review.Id,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedOn = review.CreatedOn,
                UserId = review.UserId,
                FlightId = review.FlightId,
            };

            await PopulateFormOptionsAsync(model);
            return model;
        }

        public async Task<ServiceResult> UpdateAsync(ReviewFormViewModel model)
        {
            bool exists = await context.Reviews.AnyAsync(r => r.Id == model.Id);
            if (!exists)
                return ServiceResult.Missing();

            ServiceResult validation = await ValidateAsync(model);
            if (!validation.Succeeded)
                return validation;

            Review review = new()
            {
                Id = model.Id,
                Rating = model.Rating,
                Comment = model.Comment,
                CreatedOn = model.CreatedOn,
                UserId = model.UserId,
                FlightId = model.FlightId,
            };

            context.Reviews.Update(review);
            await context.SaveChangesAsync();
            return ServiceResult.Success();
        }

        public Task<ReviewViewModel?> GetForDeleteAsync(Guid id)
        {
            return GetByIdAsync(id);
        }

        public async Task<ServiceResult> DeleteAsync(Guid id)
        {
            Review? review = await context.Reviews.FindAsync(id);
            if (review == null)
                return ServiceResult.Missing();

            context.Reviews.Remove(review);
            await context.SaveChangesAsync();
            return ServiceResult.Success();
        }

        public async Task PopulateFormOptionsAsync(ReviewFormViewModel model)
        {
            model.Flights = await lookupService.GetFlightOptionsAsync();
            model.Users = await lookupService.GetUserOptionsAsync();
        }

        private async Task<Review?> GetReviewEntityByIdAsync(Guid id)
        {
            return await context.Reviews
                .AsNoTracking()
                .Include(r => r.Flight)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        private async Task<ServiceResult> ValidateAsync(ReviewFormViewModel model)
        {
            ServiceResult result = ServiceResult.Success();

            if (model.FlightId == Guid.Empty || !await context.Flights.AnyAsync(f => f.Id == model.FlightId))
                result.AddError(nameof(model.FlightId), "A valid flight is required.");

            if (string.IsNullOrWhiteSpace(model.UserId) || !await context.ApplicationUsers.AnyAsync(u => u.Id == model.UserId))
                result.AddError(nameof(model.UserId), "A valid user is required.");

            if (string.IsNullOrWhiteSpace(model.Comment))
                result.AddError(nameof(model.Comment), "Review comment is required.");

            if (model.CreatedOn == default)
                model.CreatedOn = DateTime.UtcNow;
            else if (model.CreatedOn > DateTime.UtcNow.AddMinutes(1))
                result.AddError(nameof(model.CreatedOn), "Review date cannot be in the future.");

            return result;
        }

        private static ReviewViewModel MapReviewViewModel(Review review)
        {
            return new ReviewViewModel
            {
                Id = review.Id,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedOn = review.CreatedOn,
                User = review.User.UserName ?? review.User.Email ?? review.User.Id,
                Flight = review.Flight.FlightNumber,
            };
        }
    }
}