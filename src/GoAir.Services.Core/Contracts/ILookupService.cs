namespace GoAir.Services.Core.Contracts
{
    using Web.ViewModels.Common;

    public interface ILookupService
    {
        Task<IEnumerable<LookupOptionViewModel>> GetAircraftOptionsAsync();

        Task<IEnumerable<LookupOptionViewModel>> GetAirportOptionsAsync();

        Task<IEnumerable<LookupOptionViewModel>> GetFlightOptionsAsync();

        Task<IEnumerable<LookupOptionViewModel>> GetUserOptionsAsync();
    }
}