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