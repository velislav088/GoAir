namespace GoAir.Web.ViewModels.Airport
{
    public class AirportIndexViewModel
    {
        public string SearchTerm { get; set; } = string.Empty;
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public bool IsAdmin { get; set; }
        public IEnumerable<AirportViewModel> Airports { get; set; } = [];
    }
}