using GoAir.Data.Models;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GoAir.Data
{

    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
    {
        public DbSet<Aircraft> Aircraft { get; set; } = null!;

        public DbSet<Airport> Airports { get; set; } = null!;

        public DbSet<Flight> Flights { get; set; } = null!;

        public DbSet<Ticket> Tickets { get; set; } = null!;

        public DbSet<Review> Reviews { get; set; } = null!;
    }
}