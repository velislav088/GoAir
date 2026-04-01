using System.ComponentModel.DataAnnotations;

namespace GoAir.Data.Models
{
    public class Aircraft
    {
        public Guid Id { get; set; }

        [Required]
        public string Model { get; set; } = null!;

        [Required]
        public string Manufacturer { get; set; } = null!;

        [Required]
        public int Capacity { get; set; }

        public ICollection<Flight> Flights { get; set; } = new HashSet<Flight>();
    }
}