namespace GoAir.Web.ViewModels.Review
{
    using System.ComponentModel.DataAnnotations;

    public class ReviewViewModel
    {
        public Guid Id { get; set; }

        [Display(Name = "Rating")]
        public int Rating { get; set; }

        [Display(Name = "Comment")]
        public string Comment { get; set; } = string.Empty;

        [Display(Name = "Created On")]
        public DateTime CreatedOn { get; set; }

        [Display(Name = "User")]
        public string User { get; set; } = string.Empty;

        [Display(Name = "Flight")]
        public string Flight { get; set; } = string.Empty;
    }
}