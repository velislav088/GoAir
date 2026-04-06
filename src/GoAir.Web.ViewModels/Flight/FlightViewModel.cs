namespace GoAir.Web.ViewModels.Flight
{
    using System.ComponentModel.DataAnnotations;

    public class FlightViewModel
    {
        public Guid Id { get; set; }

        [Display(Name = "Flight Number")]
        public string FlightNumber { get; set; } = string.Empty;

        [Display(Name = "Departure Time")]
        public DateTime DepartureTime { get; set; }

        [Display(Name = "Arrival Time")]
        public DateTime ArrivalTime { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; } = string.Empty;

        [Display(Name = "Departure Airport")]
        public string DepartureAirport { get; set; } = string.Empty;

        [Display(Name = "Arrival Airport")]
        public string ArrivalAirport { get; set; } = string.Empty;

        [Display(Name = "Aircraft")]
        public string Aircraft { get; set; } = string.Empty;
    }
}