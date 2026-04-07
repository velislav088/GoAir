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
        public async Task<ReviewIndexViewModel> GetAllAsync(string userId, bool isAdmin, string? searchTerm, int page)
        {
            const int PageSize = 6;
            IQueryable<Review> query = context.Reviews
            .AsNoTracking()
            .Include(r => r.Flight)
            .Include(r => r.User);
            if (!isAdmin)
            {
                query = query.Where(r => r.UserId == userId);
            }
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string normalizedSearch = searchTerm.Trim();
                query = query.Where(r =>
                r.Comment.Contains(normalizedSearch) ||
                r.Flight.FlightNumber.Contains(normalizedSearch) ||
                (r.User.UserName ?? string.Empty).Contains(normalizedSearch) ||
                (r.User.Email ?? string.Empty).Contains(normalizedSearch));
            }
            query = query.OrderByDescending(r => r.CreatedOn);
            int totalReviews = await query.CountAsync();
            int totalPages = Math.Max(1, (int)Math.Ceiling(totalReviews / (double)PageSize));
            int currentPage = Math.Clamp(page, 1, totalPages);
            List<ReviewViewModel> reviews = await query
            .Skip((currentPage - 1) * PageSize)
            .Take(PageSize)
            .Select(r => new ReviewViewModel
            {
                Id = r.Id,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedOn = r.CreatedOn,
                User = r.User.UserName ?? r.User.Email ?? r.User.Id,
                UserId = r.UserId,
                Flight = r.Flight.FlightNumber,
            })
            .ToListAsync();
            return new ReviewIndexViewModel
            {
                SearchTerm = searchTerm?.Trim() ?? string.Empty,
                CurrentPage = currentPage,
                TotalPages = totalPages,
                IsAdmin = isAdmin,
                CurrentUserId = userId,
                Reviews = reviews,
            };
        }
        public async Task<IEnumerable<ReviewViewModel>> GetReviewsByFlightAsync(Guid flightId)
        {
            return await context.Reviews
            .AsNoTracking()
            .Include(r => r.Flight)
            .Include(r => r.User)
            .Where(r => r.FlightId == flightId)
            .OrderByDescending(r => r.CreatedOn)
            .Select(r => new ReviewViewModel
            {
                Id = r.Id,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedOn = r.CreatedOn,
                User = r.User.UserName ?? r.User.Email ?? r.User.Id,
                UserId = r.UserId,
                Flight = r.Flight.FlightNumber,
            })
            .ToListAsync();
        }
        public async Task<ReviewViewModel?> GetByIdAsync(Guid id, string userId, bool isAdmin)
        {
            Review? review = await GetReviewEntityByIdAsync(id, userId, isAdmin);
            return review == null ? null : MapReviewViewModel(review);
        }

        public async Task<ReviewFormViewModel> GetCreateModelAsync()
        {
            ReviewFormViewModel model = new();
            await PopulateFormOptionsAsync(model);
            return model;
        }
        public async Task<ServiceResult> CreateAsync(ReviewFormViewModel model, string userId)
        {
            ServiceResult validation = await ValidateAsync(model, userId);
            if (!validation.Succeeded)
            return validation;

            Review review = new()
            {
                Id = Guid.NewGuid(),
                Rating = model.Rating,
                Comment = model.Comment,
                CreatedOn = model.CreatedOn ?? DateTime.UtcNow,
                UserId = userId,
                FlightId = model.FlightId,
            };

            context.Reviews.Add(review);
            await context.SaveChangesAsync();
            return ServiceResult.Success();
        }
        public async Task<ReviewFormViewModel?> GetForEditAsync(Guid id, string userId, bool isAdmin)
        {
            Review? review = await GetEditableReviewAsync(id, userId, isAdmin);
            if (review == null)
            return null;

            ReviewFormViewModel model = new()
            {
                Id = review.Id,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedOn = review.CreatedOn,
                FlightId = review.FlightId,
            };

            await PopulateFormOptionsAsync(model);
            return model;
        }
        public async Task<ServiceResult> UpdateAsync(ReviewFormViewModel model, string userId, bool isAdmin)
        {
            Review? review = await GetEditableReviewAsync(model.Id, userId, isAdmin);
            if (review == null)
            return ServiceResult.Missing();
            ServiceResult validation = await ValidateAsync(model, userId);
            if (!validation.Succeeded)
            return validation;
            review.Rating = model.Rating;
            review.Comment = model.Comment;
            review.CreatedOn = model.CreatedOn ?? review.CreatedOn;
            review.FlightId = model.FlightId;
            await context.SaveChangesAsync();
            return ServiceResult.Success();
        }
        public Task<ReviewViewModel?> GetForDeleteAsync(Guid id, string userId, bool isAdmin)
        {
            return GetByIdAsync(id, userId, isAdmin);
        }
        public async Task<ServiceResult> DeleteAsync(Guid id, string userId, bool isAdmin)
        {
            Review? review = await GetEditableReviewAsync(id, userId, isAdmin);
            if (review == null)
            return ServiceResult.Missing();

            context.Reviews.Remove(review);
            await context.SaveChangesAsync();
            return ServiceResult.Success();
        }

        public async Task PopulateFormOptionsAsync(ReviewFormViewModel model)
        {
            model.Flights = await lookupService.GetFlightOptionsAsync();
        }
        private async Task<Review?> GetReviewEntityByIdAsync(Guid id, string userId, bool isAdmin)
        {
            IQueryable<Review> query = context.Reviews
            .AsNoTracking()
            .Include(r => r.Flight)
            .Include(r => r.User)
            .Where(r => r.Id == id);
            if (!isAdmin)
            {
                query = query.Where(r => r.UserId == userId);
            }
            return await query.FirstOrDefaultAsync();
        }
        private async Task<Review?> GetEditableReviewAsync(Guid id, string userId, bool isAdmin)
        {
            IQueryable<Review> query = context.Reviews.Where(r => r.Id == id);
            if (!isAdmin)
            {
                query = query.Where(r => r.UserId == userId);
            }
            return await query.FirstOrDefaultAsync();
        }
        private async Task<ServiceResult> ValidateAsync(ReviewFormViewModel model, string userId)
        {
            ServiceResult result = ServiceResult.Success();

            if (model.FlightId == Guid.Empty || !await context.Flights.AnyAsync(f => f.Id == model.FlightId))
            result.AddError(nameof(model.FlightId), "A valid flight is required.");

            if (string.IsNullOrWhiteSpace(model.Comment))
            result.AddError(nameof(model.Comment), "Review comment is required.");
            bool validUser = await context.ApplicationUsers.AnyAsync(u => u.Id == userId);
            if (!validUser)
            result.AddError(string.Empty, "You must be logged in with a valid account to manage reviews.");
            if (model.CreatedOn.HasValue && model.CreatedOn.Value > DateTime.UtcNow.AddMinutes(1))
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
                UserId = review.UserId,
                Flight = review.Flight.FlightNumber,
            };
        }
    }
}