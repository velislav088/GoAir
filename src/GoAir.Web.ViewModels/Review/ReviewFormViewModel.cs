namespace GoAir.Web.ViewModels.Review
{
    using Data.Common;
    using Common;

    using System.ComponentModel.DataAnnotations;

    public class ReviewFormViewModel
    {
        public Guid Id { get; set; }

        [Display(Name = "Rating")]
        [Range(EntityValidation.Review.MinRating, EntityValidation.Review.MaxRating)]
        public int Rating { get; set; }

        [Required]
        [Display(Name = "Comment")]
        [StringLength(EntityValidation.Review.CommentMaxLength)]
        public string Comment { get; set; } = string.Empty;

        [Display(Name = "Created On")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedOn { get; set; }

        [Display(Name = "User")]
        public string UserId { get; set; } = string.Empty;

        [Display(Name = "Flight")]
        public Guid FlightId { get; set; }

        public IEnumerable<LookupOptionViewModel> Users { get; set; } = [];

        public IEnumerable<LookupOptionViewModel> Flights { get; set; } = [];
    }
}