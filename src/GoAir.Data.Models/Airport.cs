using System.ComponentModel.DataAnnotations;

using GoAir.Data.Common;

namespace GoAir.Data.Models
{
    public class Airport
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(EntityValidation.Airport.NameMaxLength)]
        public string Name { get; set; } = null!;

        [Required]
        [StringLength(EntityValidation.Airport.IataCodeLength, MinimumLength = EntityValidation.Airport.IataCodeLength)]
        public string IATACode { get; set; } = null!;

        [Required]
        [StringLength(EntityValidation.Airport.CityMaxLength)]
        public string City { get; set; } = null!;

        public ICollection<Flight> DepartingFlights { get; set; } = new HashSet<Flight>();

        public ICollection<Flight> ArrivingFlights { get; set; } = new HashSet<Flight>();
    }
}