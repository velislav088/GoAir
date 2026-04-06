namespace GoAir.Data
{
    using Models;

    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;

    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<ApplicationUser> ApplicationUsers { get; set; } = null!;

        public DbSet<Aircraft> Aircraft { get; set; } = null!;

        public DbSet<Airport> Airports { get; set; } = null!;

        public DbSet<Flight> Flights { get; set; } = null!;

        public DbSet<Ticket> Tickets { get; set; } = null!;

        public DbSet<Review> Reviews { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Aircraft>(entity =>
            {
                entity
                    .HasMany(a => a.Flights)
                    .WithOne(f => f.Aircraft)
                    .HasForeignKey(f => f.AircraftId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Airport>(entity =>
            {
                entity
                    .HasIndex(a => a.IATACode)
                    .IsUnique();

                entity
                    .HasMany(a => a.DepartingFlights)
                    .WithOne(f => f.DepartureAirport)
                    .HasForeignKey(f => f.DepartureAirportId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity
                    .HasMany(a => a.ArrivingFlights)
                    .WithOne(f => f.ArrivalAirport)
                    .HasForeignKey(f => f.ArrivalAirportId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Flight>(entity =>
            {
                entity
                    .HasIndex(f => f.FlightNumber)
                    .IsUnique();
            });

            builder.Entity<Ticket>(entity =>
            {
                entity
                    .HasOne(t => t.Flight)
                    .WithMany(f => f.Tickets)
                    .HasForeignKey(t => t.FlightId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity
                    .HasOne(t => t.User)
                    .WithMany(u => u.Tickets)
                    .HasForeignKey(t => t.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity
                    .HasIndex(t => new { t.FlightId, t.SeatNumber })
                    .IsUnique();
            });

            builder.Entity<Review>(entity =>
            {
                entity
                    .HasOne(r => r.Flight)
                    .WithMany(f => f.Reviews)
                    .HasForeignKey(r => r.FlightId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity
                    .HasOne(r => r.User)
                    .WithMany(u => u.Reviews)
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}