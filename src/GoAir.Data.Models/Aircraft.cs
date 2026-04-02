using System.ComponentModel.DataAnnotations;

using GoAir.Data.Common;

namespace GoAir.Data.Models
{
    public class Aircraft
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(EntityValidation.Aircraft.ModelMaxLength)]
        public string Model { get; set; } = null!;

        [Required]
        [StringLength(EntityValidation.Aircraft.ManufacturerMaxLength)]
        public string Manufacturer { get; set; } = null!;

        [Range(EntityValidation.Aircraft.MinCapacity, EntityValidation.Aircraft.MaxCapacity)]
        public int Capacity { get; set; }

        public ICollection<Flight> Flights { get; set; } = new HashSet<Flight>();
    }
}