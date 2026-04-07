namespace GoAir.Web.ViewModels.Home
{
    public class HomeLandingViewModel
    {
        public int AirportCount { get; set; }

        public int AircraftCount { get; set; }

        public int ScheduledFlightCount { get; set; }

        public int TicketCount { get; set; }

        public int ReviewCount { get; set; }

        public IEnumerable<Flight.FlightViewModel> UpcomingFlights { get; set; } = [];
    }
}