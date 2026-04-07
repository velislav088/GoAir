namespace GoAir.Web.ViewModels.Ticket
{
    public class TicketIndexViewModel
    {
        public string SearchTerm { get; set; } = string.Empty;

        public int CurrentPage { get; set; } = 1;

        public int TotalPages { get; set; }

        public bool IsAdmin { get; set; }

        public string CurrentUserId { get; set; } = string.Empty;

        public IEnumerable<TicketViewModel> Tickets { get; set; } = [];
    }
}