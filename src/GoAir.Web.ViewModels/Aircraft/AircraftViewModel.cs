namespace GoAir.Web.ViewModels.Aircraft
{
    using Data.Common;

    using System.ComponentModel.DataAnnotations;

    public class AircraftViewModel
    {
        public Guid Id { get; set; }

        [Required]
        [Display(Name = "Model")]
        [StringLength(EntityValidation.Aircraft.ModelMaxLength)]
        public string Model { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Manufacturer")]
        [StringLength(EntityValidation.Aircraft.ManufacturerMaxLength)]
        public string Manufacturer { get; set; } = string.Empty;

        [Display(Name = "Capacity")]
        [Range(EntityValidation.Aircraft.MinCapacity, EntityValidation.Aircraft.MaxCapacity)]
        public int Capacity { get; set; }
    }
}