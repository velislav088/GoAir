using System.ComponentModel.DataAnnotations;

using static GoAir.Common.EntityValidation.Aircraft;

namespace GoAir.Models
{
    public class Aircraft
    {
        public int Id { get; set; }

        [Required]
        [StringLength(ModelMaxLength)]
        public string Model { get; set; } = null!;

        [Required]
        [StringLength(ManufacturerMaxLength)]
        public string Manufacturer { get; set; } = null!;

        [Range(MinCapacity, MaxCapacity)]
        public int Capacity { get; set; }
    }
}