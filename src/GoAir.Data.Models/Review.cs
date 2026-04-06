using System.ComponentModel.DataAnnotations;

using GoAir.Data.Common;

namespace GoAir.Data.Models
{
    public class Review
    {
        public Guid Id { get; set; }

        [Range(EntityValidation.Review.MinRating, EntityValidation.Review.MaxRating)]
        public int Rating { get; set; }

        [Required]
        [StringLength(EntityValidation.Review.CommentMaxLength)]
        public string Comment { get; set; } = null!;

        public DateTime CreatedOn { get; set; }

        public string UserId { get; set; } = null!;

        public ApplicationUser User { get; set; } = null!;

        public Guid FlightId { get; set; }

        public Flight Flight { get; set; } = null!;
    }
}