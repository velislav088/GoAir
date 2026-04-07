namespace GoAir.Web.ViewModels.Review
{
    using System.ComponentModel.DataAnnotations;
    
    using Data.Common;
    using Common;

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
        public DateTime? CreatedOn { get; set; }
        [Display(Name = "Flight")]
        public Guid FlightId { get; set; }

        public IEnumerable<LookupOptionViewModel> Flights { get; set; } = [];
    }
}