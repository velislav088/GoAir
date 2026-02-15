using System.ComponentModel.DataAnnotations;

using static GoAir.Common.EntityValidation.Airport;

namespace GoAir.Models
{
    public class Airport
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(NameMaxLength)]
        public string Name { get; set; } = null!;

        [Required]
        [StringLength(CityMaxLength)]
        public string City { get; set; } = null!;

        [Range(MinTerminals, MaxTerminals)]
        public int Terminals { get; set; }

        public ICollection<Flight> DepartingFlights { get; set; } = new HashSet<Flight>();

        public ICollection<Flight> ArrivingFlights { get; set; } = new HashSet<Flight>();

    }
}