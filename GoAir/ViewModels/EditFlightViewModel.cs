using System.ComponentModel.DataAnnotations;

using GoAir.Models;
using static GoAir.Common.EntityValidation.Flight;

namespace GoAir.ViewModels
{
    public class EditFlightViewModel : IValidatableObject
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

        [Display(Name = "Arrival Airport")]
        [Required]
        public Guid ArrivalAirportId { get; set; }

        [Display(Name = "Aircraft")]
        [Required]
        public Guid AircraftId { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DepartureAirportId == ArrivalAirportId)
            {
                yield return new ValidationResult(
                    "Departure and arrival airports cannot be the same.",
                    [nameof(DepartureAirportId), nameof(ArrivalAirportId)]);
            }

            if (DepartureTime <= DateTime.Now)
            {
                yield return new ValidationResult(
                    "Departure must be in the future.",
                    [nameof(DepartureTime)]);
            }
        }
    }
}