namespace GoAir.Web.ViewModels.Airport
{
    using Data.Common;

    using System.ComponentModel.DataAnnotations;

    public class AirportViewModel
    {
        public Guid Id { get; set; }

        [Required]
        [Display(Name = "Name")]
        [StringLength(EntityValidation.Airport.NameMaxLength)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Display(Name = "IATA Code")]
        [StringLength(EntityValidation.Airport.IataCodeLength, MinimumLength = EntityValidation.Airport.IataCodeLength)]
        public string IATACode { get; set; } = string.Empty;

        [Required]
        [Display(Name = "City")]
        [StringLength(EntityValidation.Airport.CityMaxLength)]
        public string City { get; set; } = string.Empty;
    }
}