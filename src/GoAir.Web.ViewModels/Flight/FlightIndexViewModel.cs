namespace GoAir.Web.ViewModels.Flight
{
    public class FlightIndexViewModel
    {
        public string SearchTerm { get; set; } = string.Empty;

        public string SortOrder { get; set; } = FlightSorting.DepartureSoonest;

        public int CurrentPage { get; set; } = 1;

        public int TotalPages { get; set; }

        public bool IsAdmin { get; set; }

        public IEnumerable<FlightViewModel> Flights { get; set; } = [];
    }
}