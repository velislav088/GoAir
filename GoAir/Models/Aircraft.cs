using System.ComponentModel.DataAnnotations;

namespace GoAir.Models
{
    public class Aircraft
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Model { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string Manufacturer { get; set; } = null!;

        [Range(1, 600)]
        public int Capacity { get; set; }
    }
}