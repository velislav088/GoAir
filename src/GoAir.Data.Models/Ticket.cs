using System.ComponentModel.DataAnnotations;

using GoAir.Data.Common;

namespace GoAir.Data.Models
{
    public class Ticket
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(EntityValidation.Ticket.SeatNumberMaxLength)]
        public string SeatNumber { get; set; } = null!;

        [Range(EntityValidation.Ticket.MinPrice, EntityValidation.Ticket.MaxPrice)]
        public decimal Price { get; set; }

        public FareClass FareClass { get; set; }

        public DateTime PurchasedOn { get; set; }

        public Guid FlightId { get; set; }

        public Flight Flight { get; set; } = null!;
    }
}