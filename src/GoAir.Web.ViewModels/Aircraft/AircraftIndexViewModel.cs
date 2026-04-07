namespace GoAir.Web.ViewModels.Aircraft
{
    public class AircraftIndexViewModel
    {
        public string SearchTerm { get; set; } = string.Empty;
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public bool IsAdmin { get; set; }
        public IEnumerable<AircraftViewModel> Aircraft { get; set; } = [];
    }
}