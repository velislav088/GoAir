using System.ComponentModel.DataAnnotations;

using static GoAir.Common.EntityValidation.Flight;

namespace GoAir.Models
{
    public class Flight
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(FlightNumberMaxLength)]
        public string FlightNumber { get; set; } = null!;

        [Range(MinPrice, MaxPrice)]
        public decimal Price { get; set; }

        [Required]
        public DateTime DepartureTime { get; set; }

        public FlightStatus Status { get; set; }

        [Required]
        public Guid DepartureAirportId { get; set; }

        public Airport DepartureAirport { get; set; } = null!;

        [Required]
        public Guid ArrivalAirportId { get; set; }

        public Airport ArrivalAirport { get; set; } = null!;

        public Guid? AircraftId { get; set; }

        public Aircraft? Aircraft { get; set; }

    }
}