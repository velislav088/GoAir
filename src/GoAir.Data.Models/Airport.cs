using System.ComponentModel.DataAnnotations;

namespace GoAir.Data.Models
{
    public class Airport
    {
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string IATACode { get; set; } = null!;

        [Required]
        public string City { get; set; } = null!;

        public ICollection<Flight> DepartingFlights = new HashSet<Flight>();

        public ICollection<Flight> ArrivingFlights = new HashSet<Flight>();
    }
}