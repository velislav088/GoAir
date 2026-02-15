using System.ComponentModel.DataAnnotations;

using GoAir.Models;
using static GoAir.Common.EntityValidation.Flight;

namespace GoAir.ViewModels
{
    public class EditFlightViewModel
    {
        public Guid Id { get; set; }

        [Required]
        public string FlightNumber { get; set; } = null!;

        [Range(MinPrice, MaxPrice)]
        public decimal Price { get; set; }

        [Required]
        public DateTime DepartureTime { get; set; }

        public FlightStatus Status { get; set; }

        [Required]
        public Guid DepartureAirportId { get; set; }

        [Required]
        public Guid ArrivalAirportId { get; set; }

        [Required]
        public Guid AircraftId { get; set; }

    }

}
