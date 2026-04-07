namespace GoAir.Web.ViewModels.Ticket
{
    using System.ComponentModel.DataAnnotations;

    using Data.Common;
    using Data.Models;
    using Common;

    public class TicketFormViewModel
    {
        public Guid Id { get; set; }

        [Required]
        [Display(Name = "Seat Number")]
        [StringLength(EntityValidation.Ticket.SeatNumberMaxLength)]
        public string SeatNumber { get; set; } = string.Empty;

        [Display(Name = "Price")]
        [Range(EntityValidation.Ticket.MinPrice, EntityValidation.Ticket.MaxPrice)]
        public decimal Price { get; set; }

        [Display(Name = "Fare Class")]
        public FareClass FareClass { get; set; }

        [Display(Name = "Purchased On")]
        [DataType(DataType.DateTime)]
        public DateTime? PurchasedOn { get; set; }

        [Display(Name = "Flight")]
        public Guid FlightId { get; set; }

        public IEnumerable<LookupOptionViewModel> Flights { get; set; } = [];
    }
}