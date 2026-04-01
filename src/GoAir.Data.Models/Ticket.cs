using System.ComponentModel.DataAnnotations;

namespace GoAir.Data.Models
{
    public class Ticket
    {
        public Guid Id { get; set; }

        [Required]
        public string SeatNumber { get; set; } = null!;

        public decimal Price { get; set; }

        public FareClass FareClass { get; set; }

        public DateTime PurchasedOn { get; set; }

        [Required]
        public Guid FlightId { get; set; }

        public Flight Flight { get; set; } = null!;
    }
}