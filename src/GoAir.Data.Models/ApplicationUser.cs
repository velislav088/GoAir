using Microsoft.AspNetCore.Identity;

namespace GoAir.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ICollection<Ticket> Tickets { get; set; } = new HashSet<Ticket>();

        public ICollection<Review> Reviews { get; set; } = new HashSet<Review>();
    }
}