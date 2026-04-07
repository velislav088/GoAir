namespace GoAir.Services.Core.Tests
{
    using Data;
    using Data.Models;
    using Common;
    using Services;
    using Web.ViewModels.Review;

    using Microsoft.EntityFrameworkCore;

    [TestFixture]
    public class ReviewServiceTests
    {
        [Test]
        public async Task GetCreateModelAsync_ShouldPopulateFlightOptions()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            TestLookupService lookupService = new()
            {
                FlightOptions =
                [
                new() { Value = Guid.NewGuid().ToString(), Text = "GA500" }
                ],
            };
            ReviewService service = new(dbContext, lookupService);
            ReviewFormViewModel model = await service.GetCreateModelAsync();
            Assert.That(model.Flights.Select(f => f.Text), Is.EqualTo(new[] { "GA500" }));
        }
        [Test]
        public async Task CreateAsync_ShouldRejectFutureReviewDateAndInvalidUser()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            Flight flight = new()
            {
                Id = Guid.NewGuid(),
                FlightNumber = "GA555",
                DepartureTime = DateTime.UtcNow.AddDays(1),
                ArrivalTime = DateTime.UtcNow.AddDays(1).AddHours(2),
                Status = FlightStatus.OnTime,
            };
            dbContext.Flights.Add(flight);
            await dbContext.SaveChangesAsync();
            ReviewService service = new(dbContext, new TestLookupService());
            ServiceResult result = await service.CreateAsync(new ReviewFormViewModel
            {
                FlightId = flight.Id,
                Rating = 5,
                Comment = "Great flight",
                CreatedOn = DateTime.UtcNow.AddDays(1),
            }, "missing-user");
            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Errors[string.Empty], Does.Contain("You must be logged in with a valid account to manage reviews."));
            Assert.That(result.Errors[nameof(ReviewFormViewModel.CreatedOn)], Does.Contain("Review date cannot be in the future."));
        }
        [Test]
        public async Task CreateAsync_ShouldSetCreatedOnAndPersistReview()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            ApplicationUser user = new() { Id = "user-id", UserName = "user@goair.local", Email = "user@goair.local" };
            Flight flight = new()
            {
                Id = Guid.NewGuid(),
                FlightNumber = "GA556",
                DepartureTime = DateTime.UtcNow.AddDays(1),
                ArrivalTime = DateTime.UtcNow.AddDays(1).AddHours(2),
                Status = FlightStatus.OnTime,
            };
            dbContext.ApplicationUsers.Add(user);
            dbContext.Flights.Add(flight);
            await dbContext.SaveChangesAsync();
            ReviewService service = new(dbContext, new TestLookupService());
            ServiceResult result = await service.CreateAsync(new ReviewFormViewModel
            {
                FlightId = flight.Id,
                Rating = 4,
                Comment = "Very good",
            }, user.Id);
            Review storedReview = await dbContext.Reviews.SingleAsync();
            Assert.That(result.Succeeded, Is.True);
            Assert.That(storedReview.Comment, Is.EqualTo("Very good"));
            Assert.That(storedReview.CreatedOn, Is.Not.EqualTo(default(DateTime)));
        }
        [Test]
        public async Task GetAllAsync_ShouldLimitRegularUsersToOwnReviews()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            ApplicationUser owner = new() { Id = "owner", UserName = "owner@goair.local", Email = "owner@goair.local" };
            ApplicationUser other = new() { Id = "other", UserName = "other@goair.local", Email = "other@goair.local" };
            Flight flight = new() { Id = Guid.NewGuid(), FlightNumber = "GA123", DepartureTime = DateTime.UtcNow, ArrivalTime = DateTime.UtcNow.AddHours(2), Status = FlightStatus.OnTime };
            dbContext.ApplicationUsers.AddRange(owner, other);
            dbContext.Flights.Add(flight);
            dbContext.Reviews.AddRange(
            new Review { Id = Guid.NewGuid(), Rating = 5, Comment = "Mine", CreatedOn = DateTime.UtcNow, FlightId = flight.Id, UserId = owner.Id },
            new Review { Id = Guid.NewGuid(), Rating = 4, Comment = "Not mine", CreatedOn = DateTime.UtcNow.AddMinutes(-1), FlightId = flight.Id, UserId = other.Id });
            await dbContext.SaveChangesAsync();
            ReviewService service = new(dbContext, new TestLookupService());
            ReviewIndexViewModel result = await service.GetAllAsync(owner.Id, false, null, 1);
            List<string> comments = result.Reviews.Select(r => r.Comment).ToList();
            Assert.That(comments, Is.EqualTo(new[] { "Mine" }));
            Assert.That(result.CurrentUserId, Is.EqualTo(owner.Id));
        }
        [Test]
        public async Task GetByIdAsync_ShouldReturnNull_WhenReviewBelongsToAnotherUser()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            ApplicationUser owner = new() { Id = "owner", UserName = "owner@goair.local", Email = "owner@goair.local" };
            ApplicationUser intruder = new() { Id = "intruder", UserName = "intruder@goair.local", Email = "intruder@goair.local" };
            Flight flight = new() { Id = Guid.NewGuid(), FlightNumber = "GA124", DepartureTime = DateTime.UtcNow, ArrivalTime = DateTime.UtcNow.AddHours(2), Status = FlightStatus.OnTime };
            Review review = new() { Id = Guid.NewGuid(), Rating = 5, Comment = "Mine", CreatedOn = DateTime.UtcNow, FlightId = flight.Id, UserId = owner.Id };
            dbContext.ApplicationUsers.AddRange(owner, intruder);
            dbContext.Flights.Add(flight);
            dbContext.Reviews.Add(review);
            await dbContext.SaveChangesAsync();
            ReviewService service = new(dbContext, new TestLookupService());
            ReviewViewModel? result = await service.GetByIdAsync(review.Id, intruder.Id, false);
            Assert.That(result, Is.Null);
        }
        [Test]
        public async Task GetForEditAsync_ShouldReturnMappedModel_ForOwner()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            ApplicationUser owner = new() { Id = "owner", UserName = "owner@goair.local", Email = "owner@goair.local" };
            Flight flight = new() { Id = Guid.NewGuid(), FlightNumber = "GA125", DepartureTime = DateTime.UtcNow, ArrivalTime = DateTime.UtcNow.AddHours(2), Status = FlightStatus.OnTime };
            Review review = new() { Id = Guid.NewGuid(), Rating = 3, Comment = "Average", CreatedOn = DateTime.UtcNow, FlightId = flight.Id, UserId = owner.Id };
            dbContext.ApplicationUsers.Add(owner);
            dbContext.Flights.Add(flight);
            dbContext.Reviews.Add(review);
            await dbContext.SaveChangesAsync();
            ReviewService service = new(dbContext, new TestLookupService());
            ReviewFormViewModel? result = await service.GetForEditAsync(review.Id, owner.Id, false);
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Comment, Is.EqualTo("Average"));
            Assert.That(result.FlightId, Is.EqualTo(flight.Id));
        }
        [Test]
        public async Task UpdateAsync_ShouldUpdateOwnedReview()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            ApplicationUser user = new() { Id = "user", UserName = "user@goair.local", Email = "user@goair.local" };
            Flight flight = new() { Id = Guid.NewGuid(), FlightNumber = "GA126", DepartureTime = DateTime.UtcNow, ArrivalTime = DateTime.UtcNow.AddHours(2), Status = FlightStatus.OnTime };
            Review review = new() { Id = Guid.NewGuid(), Rating = 3, Comment = "Average", CreatedOn = DateTime.UtcNow.AddDays(-1), FlightId = flight.Id, UserId = user.Id };
            dbContext.ApplicationUsers.Add(user);
            dbContext.Flights.Add(flight);
            dbContext.Reviews.Add(review);
            await dbContext.SaveChangesAsync();
            ReviewService service = new(dbContext, new TestLookupService());
            ServiceResult result = await service.UpdateAsync(new ReviewFormViewModel
            {
                Id = review.Id,
                FlightId = flight.Id,
                Rating = 5,
                Comment = "Excellent",
                CreatedOn = DateTime.UtcNow,
            }, user.Id, false);
            Review storedReview = await dbContext.Reviews.SingleAsync();
            Assert.That(result.Succeeded, Is.True);
            Assert.That(storedReview.Rating, Is.EqualTo(5));
            Assert.That(storedReview.Comment, Is.EqualTo("Excellent"));
        }
        [Test]
        public async Task UpdateAsync_ShouldReturnMissing_WhenReviewIsNotEditable()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            ApplicationUser owner = new() { Id = "owner", UserName = "owner@goair.local", Email = "owner@goair.local" };
            ApplicationUser intruder = new() { Id = "intruder", UserName = "intruder@goair.local", Email = "intruder@goair.local" };
            Flight flight = new() { Id = Guid.NewGuid(), FlightNumber = "GA127", DepartureTime = DateTime.UtcNow, ArrivalTime = DateTime.UtcNow.AddHours(2), Status = FlightStatus.OnTime };
            Review review = new() { Id = Guid.NewGuid(), Rating = 5, Comment = "Mine", CreatedOn = DateTime.UtcNow, FlightId = flight.Id, UserId = owner.Id };
            dbContext.ApplicationUsers.AddRange(owner, intruder);
            dbContext.Flights.Add(flight);
            dbContext.Reviews.Add(review);
            await dbContext.SaveChangesAsync();
            ReviewService service = new(dbContext, new TestLookupService());
            ServiceResult result = await service.UpdateAsync(new ReviewFormViewModel
            {
                Id = review.Id,
                FlightId = flight.Id,
                Rating = 1,
                Comment = "Changed",
                CreatedOn = DateTime.UtcNow,
            }, intruder.Id, false);
            Assert.That(result.NotFound, Is.True);
        }
        [Test]
        public async Task DeleteAsync_ShouldReturnMissing_ForAnotherUsersReview()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            ApplicationUser owner = new() { Id = "owner", UserName = "owner@goair.local", Email = "owner@goair.local" };
            ApplicationUser intruder = new() { Id = "intruder", UserName = "intruder@goair.local", Email = "intruder@goair.local" };
            Flight flight = new() { Id = Guid.NewGuid(), FlightNumber = "GA123", DepartureTime = DateTime.UtcNow, ArrivalTime = DateTime.UtcNow.AddHours(2), Status = FlightStatus.OnTime };
            Review review = new() { Id = Guid.NewGuid(), Rating = 5, Comment = "Mine", CreatedOn = DateTime.UtcNow, FlightId = flight.Id, UserId = owner.Id };
            dbContext.ApplicationUsers.AddRange(owner, intruder);
            dbContext.Flights.Add(flight);
            dbContext.Reviews.Add(review);
            await dbContext.SaveChangesAsync();
            ReviewService service = new(dbContext, new TestLookupService());
            ServiceResult result = await service.DeleteAsync(review.Id, intruder.Id, false);
            Assert.That(result.NotFound, Is.True);
        }
        [Test]
        public async Task DeleteAsync_ShouldRemoveOwnedReview()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            ApplicationUser owner = new() { Id = "owner", UserName = "owner@goair.local", Email = "owner@goair.local" };
            Flight flight = new() { Id = Guid.NewGuid(), FlightNumber = "GA128", DepartureTime = DateTime.UtcNow, ArrivalTime = DateTime.UtcNow.AddHours(2), Status = FlightStatus.OnTime };
            Review review = new() { Id = Guid.NewGuid(), Rating = 5, Comment = "Mine", CreatedOn = DateTime.UtcNow, FlightId = flight.Id, UserId = owner.Id };
            dbContext.ApplicationUsers.Add(owner);
            dbContext.Flights.Add(flight);
            dbContext.Reviews.Add(review);
            await dbContext.SaveChangesAsync();
            ReviewService service = new(dbContext, new TestLookupService());
            ServiceResult result = await service.DeleteAsync(review.Id, owner.Id, false);
            Assert.That(result.Succeeded, Is.True);
            Assert.That(await dbContext.Reviews.CountAsync(), Is.EqualTo(0));
        }
        [Test]
        public async Task GetReviewsByFlightAsync_ShouldReturnReviewsOrderedNewestFirst()
        {
            await using ApplicationDbContext dbContext = ServiceTestHelpers.CreateDbContext();
            ApplicationUser user = new() { Id = "user", UserName = "user@goair.local", Email = "user@goair.local" };
            Flight targetFlight = new() { Id = Guid.NewGuid(), FlightNumber = "GA129", DepartureTime = DateTime.UtcNow, ArrivalTime = DateTime.UtcNow.AddHours(2), Status = FlightStatus.OnTime };
            Flight otherFlight = new() { Id = Guid.NewGuid(), FlightNumber = "GA130", DepartureTime = DateTime.UtcNow, ArrivalTime = DateTime.UtcNow.AddHours(3), Status = FlightStatus.Delayed };
            dbContext.ApplicationUsers.Add(user);
            dbContext.Flights.AddRange(targetFlight, otherFlight);
            dbContext.Reviews.AddRange(
            new Review { Id = Guid.NewGuid(), Rating = 4, Comment = "Older", CreatedOn = DateTime.UtcNow.AddDays(-2), FlightId = targetFlight.Id, UserId = user.Id },
            new Review { Id = Guid.NewGuid(), Rating = 5, Comment = "Newest", CreatedOn = DateTime.UtcNow.AddDays(-1), FlightId = targetFlight.Id, UserId = user.Id },
            new Review { Id = Guid.NewGuid(), Rating = 3, Comment = "Other flight", CreatedOn = DateTime.UtcNow, FlightId = otherFlight.Id, UserId = user.Id });
            await dbContext.SaveChangesAsync();
            ReviewService service = new(dbContext, new TestLookupService());
            List<ReviewViewModel> result = (await service.GetReviewsByFlightAsync(targetFlight.Id)).ToList();
            Assert.That(result.Select(r => r.Comment), Is.EqualTo(new[] { "Newest", "Older" }));
        }
    }
}