using System.ComponentModel.DataAnnotations;

using static GoAir.Common.EntityValidation.Flight;

namespace GoAir.Models
{
    public class Flight
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(FlightNumberMaxLength)]
        [Display(Name = "Flight Number")]
        public string FlightNumber { get; set; } = null!;

        [Display(Name = "Price")]
        [Range(MinPrice, MaxPrice)]
        public decimal Price { get; set; }

        [Display(Name = "Departure Time")]
        [Required]
        public DateTime DepartureTime { get; set; }

        [Display(Name = "Status")]
        public FlightStatus Status { get; set; }

        [Display(Name = "Departure Airport")]
        [Required]
        public Guid DepartureAirportId { get; set; }

        [Display(Name = "Departure Airport")]
        public Airport DepartureAirport { get; set; } = null!;

        [Display(Name = "Arrival Airport")]
        [Required]
        public Guid ArrivalAirportId { get; set; }

        [Display(Name = "Arrival Airport")]
        public Airport ArrivalAirport { get; set; } = null!;

        [Display(Name = "Aircraft")]
        public Guid? AircraftId { get; set; }

        [Display(Name = "Aircraft")]
        public Aircraft? Aircraft { get; set; }

    }
}