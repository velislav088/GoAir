namespace GoAir.Data.Models
{
    using System.ComponentModel.DataAnnotations;

    public enum FlightStatus
    {
        [Display(Name = "On Time")]
        OnTime = 0,

        [Display(Name = "Delayed")]
        Delayed = 1,

        [Display(Name = "Boarding")]
        Boarding = 2,

        [Display(Name = "Cancelled")]
        Cancelled = 3
    }
}