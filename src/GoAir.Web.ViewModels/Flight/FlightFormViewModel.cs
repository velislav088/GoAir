namespace GoAir.Web.ViewModels.Flight
{
    using Data.Common;
    using Data.Models;
    using Common;

    using System.ComponentModel.DataAnnotations;

    public class FlightFormViewModel
    {
        public Guid Id { get; set; }

        [Required]
        [Display(Name = "Flight Number")]
        [StringLength(EntityValidation.Flight.FlightNumberMaxLength, MinimumLength = EntityValidation.Flight.FlightNumberMinLength)]
        public string FlightNumber { get; set; } = string.Empty;

        [Display(Name = "Departure Time")]
        [DataType(DataType.DateTime)]
        public DateTime DepartureTime { get; set; }

        [Display(Name = "Arrival Time")]
        [DataType(DataType.DateTime)]
        public DateTime ArrivalTime { get; set; }

        [Display(Name = "Status")]
        public FlightStatus Status { get; set; }

        [Display(Name = "Departure Airport")]
        public Guid DepartureAirportId { get; set; }

        [Display(Name = "Arrival Airport")]
        public Guid ArrivalAirportId { get; set; }

        [Display(Name = "Aircraft")]
        public Guid AircraftId { get; set; }

        public IEnumerable<LookupOptionViewModel> DepartureAirports { get; set; } = [];

        public IEnumerable<LookupOptionViewModel> ArrivalAirports { get; set; } = [];

        public IEnumerable<LookupOptionViewModel> AircraftOptions { get; set; } = [];
    }
}