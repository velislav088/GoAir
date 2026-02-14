using System.ComponentModel.DataAnnotations;

using static GoAir.Common.EntityValidation.Flight;

namespace GoAir.Models
{
    public class Flight
    {
        public int Id { get; set; }

        [Required]
        [StringLength(FlightNumberMaxLength)]
        public string FlightNumber { get; set; } = null!;

        [Range(MinPrice, MaxPrice)]
        public decimal Price { get; set; }

        [Required]
        public DateTime DepartureTime { get; set; }

        public FlightStatus Status { get; set; }

        [Required]
        public int DepartureAirportId { get; set; }

        public Airport DepartureAirport { get; set; } = null!;

        [Required]
        public int ArrivalAirportId { get; set; }

        [Required]
        public Airport ArrivalAirport { get; set; } = null!;
    }
}