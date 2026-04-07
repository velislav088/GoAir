namespace GoAir.Web.ViewModels.Review
{
    public class ReviewIndexViewModel
    {
        public string SearchTerm { get; set; } = string.Empty;

        public int CurrentPage { get; set; } = 1;

        public int TotalPages { get; set; }

        public bool IsAdmin { get; set; }

        public string CurrentUserId { get; set; } = string.Empty;

        public IEnumerable<ReviewViewModel> Reviews { get; set; } = [];
    }
}