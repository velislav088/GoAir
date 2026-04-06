namespace GoAir.Data.Models
{
    using System.ComponentModel.DataAnnotations;

    public enum FareClass
    {
        [Display(Name = "Economy")]
        Economy = 0,

        [Display(Name = "Business")]
        Business = 1
    }
}