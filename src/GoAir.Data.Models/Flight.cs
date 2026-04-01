using System.ComponentModel.DataAnnotations;

namespace GoAir.Data.Models
{
    public class Flight
    {
        public Guid Id { get; set; }

        [Required]
        public string FlightNumber { get; set; } = null!;

        [Required]
        public DateTime DepartureTime { get; set; }

        [Required]
        public DateTime ArrivalTime { get; set; }

        public FlightStatus Status { get; set; }

        [Required]
        public Guid DepartureAirportId { get; set; }

        public Airport DepartureAirport { get; set; } = null!;

        [Required]
        public Guid ArrivalAirportId { get; set; }

        public Airport ArrivalAirport { get; set; } = null!;

        [Required]
        public Guid AircraftId { get; set; }

        public Aircraft Aircraft { get; set; } = null!;

        public ICollection<Ticket> Tickets { get; set; } = new HashSet<Ticket>();

        public ICollection<Review> Reviews { get; set; } = new HashSet<Review>();
    }
}