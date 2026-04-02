using System.ComponentModel.DataAnnotations;

using GoAir.Data.Common;

namespace GoAir.Data.Models
{
    public class Flight
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(EntityValidation.Flight.FlightNumberMaxLength, MinimumLength = EntityValidation.Flight.FlightNumberMinLength)]
        public string FlightNumber { get; set; } = null!;

        public DateTime DepartureTime { get; set; }

        public DateTime ArrivalTime { get; set; }

        public FlightStatus Status { get; set; }

        public Guid DepartureAirportId { get; set; }

        public Airport DepartureAirport { get; set; } = null!;

        public Guid ArrivalAirportId { get; set; }

        public Airport ArrivalAirport { get; set; } = null!;

        public Guid AircraftId { get; set; }

        public Aircraft Aircraft { get; set; } = null!;

        public ICollection<Ticket> Tickets { get; set; } = new HashSet<Ticket>();

        public ICollection<Review> Reviews { get; set; } = new HashSet<Review>();
    }
}