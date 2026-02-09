using System.ComponentModel.DataAnnotations;

namespace GoAir.Models
{
    public class Airport
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string City { get; set; } = null!;

        [Range(1, 10)]
        public int Terminals { get; set; }
    }
}