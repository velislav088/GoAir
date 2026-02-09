using System.ComponentModel.DataAnnotations;

namespace GoAir.Models
{
    public class Flight
    {
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string FlightNumber { get; set; } = null!;

        [Range(0, 100000)]
        public decimal Price { get; set; }

        [Required]
        public DateTime DepartureTime { get; set; }

        public FlightStatus Status { get; set; }

        [Required]
        public int DepartureAirportId { get; set; }

        [Required]
        public Airport DepartureAirport { get; set; } = null!;

        [Required]
        public int ArrivalAirportId { get; set; }

        [Required]
        public Airport ArrivalAirport { get; set; } = null!;
    }
}