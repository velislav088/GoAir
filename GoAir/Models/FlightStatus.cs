using System.ComponentModel.DataAnnotations;

namespace GoAir.Models
{
    public enum FlightStatus
    {
        [Display(Name = "Checking In")]
        CheckingIn = 0,

        [Display(Name = "Boarding")]
        Boarding = 1,

        [Display(Name = "Departed")]
        Departed = 2,

        [Display(Name = "Delayed")]
        Delayed = 3,

        [Display(Name = "Deboarding")]
        Deboarding = 4
    }
}