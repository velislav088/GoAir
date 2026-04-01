using System.ComponentModel.DataAnnotations;

namespace GoAir.Data.Models
{
    public class Review
    {
        public Guid Id { get; set; }

        public int Rating { get; set; }

        [Required]
        public string Comment { get; set; } = null!;

        public DateTime CreatedOn { get; set; }

        [Required]
        public Guid FlightId { get; set; }

        public Flight Flight { get; set; } = null!;
    }
}