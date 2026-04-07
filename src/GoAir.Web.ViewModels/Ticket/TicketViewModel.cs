namespace GoAir.Web.ViewModels.Ticket
{
    using System.ComponentModel.DataAnnotations;

    public class TicketViewModel
    {
        public Guid Id { get; set; }

        [Display(Name = "Seat Number")]
        public string SeatNumber { get; set; } = string.Empty;

        [Display(Name = "Price")]
        public decimal Price { get; set; }

        [Display(Name = "Fare Class")]
        public string FareClass { get; set; } = string.Empty;

        [Display(Name = "Purchased On")]
        public DateTime PurchasedOn { get; set; }

        [Display(Name = "User")]
        public string User { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty;

        [Display(Name = "Flight")]
        public string Flight { get; set; } = string.Empty;
    }
}