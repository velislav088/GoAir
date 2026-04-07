namespace GoAir.Web
{
    using Data;
    using Data.Models;
    using GCommon;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    public static class DataSeeder
    {
        public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
        {
            using IServiceScope scope = services.CreateScope();
            ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            RoleManager<IdentityRole> roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            await dbContext.Database.MigrateAsync();
            await EnsureRolesAsync(roleManager);
            await EnsureAdminUserAsync(userManager, configuration);
            await EnsureCatalogSeedAsync(dbContext);
        }
        private static async Task EnsureRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            await EnsureRoleExistsAsync(roleManager, ApplicationRoles.User);
            await EnsureRoleExistsAsync(roleManager, ApplicationRoles.Administrator);
        }
        private static async Task EnsureAdminUserAsync(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            string email = configuration["SeedAdmin:Email"] ?? "admin@goair.local";
            string password = configuration["SeedAdmin:Password"] ?? "GoAirAdmin123!";
            ApplicationUser? adminUser = await userManager.FindByEmailAsync(email);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                };
                IdentityResult createResult = await userManager.CreateAsync(adminUser, password);
                if (!createResult.Succeeded)
                {
                    throw new InvalidOperationException("Failed to create the seeded GoAir administrator account.");
                }
            }
            if (!await userManager.IsInRoleAsync(adminUser, ApplicationRoles.User))
            {
                await userManager.AddToRoleAsync(adminUser, ApplicationRoles.User);
            }
            if (!await userManager.IsInRoleAsync(adminUser, ApplicationRoles.Administrator))
            {
                await userManager.AddToRoleAsync(adminUser, ApplicationRoles.Administrator);
            }
        }
        private static async Task EnsureRoleExistsAsync(RoleManager<IdentityRole> roleManager, string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
        private static async Task EnsureCatalogSeedAsync(ApplicationDbContext dbContext)
        {
            if (await dbContext.Airports.AnyAsync())
            {
                return;
            }
            Airport sofia = new()
            {
                Id = Guid.NewGuid(),
                Name = "Sofia Airport",
                City = "Sofia",
                IATACode = "SOF",
            };
            Airport london = new()
            {
                Id = Guid.NewGuid(),
                Name = "Heathrow Airport",
                City = "London",
                IATACode = "LHR",
            };
            Airport frankfurt = new()
            {
                Id = Guid.NewGuid(),
                Name = "Frankfurt Airport",
                City = "Frankfurt",
                IATACode = "FRA",
            };
            Aircraft airbus = new()
            {
                Id = Guid.NewGuid(),
                Manufacturer = "Airbus",
                Model = "A320neo",
                Capacity = 180,
            };
            Aircraft boeing = new()
            {
                Id = Guid.NewGuid(),
                Manufacturer = "Boeing",
                Model = "737 MAX 8",
                Capacity = 178,
            };
            DateTime utcNow = DateTime.UtcNow;
            Flight firstFlight = new()
            {
                Id = Guid.NewGuid(),
                FlightNumber = "GA101",
                DepartureTime = utcNow.AddDays(1).AddHours(2),
                ArrivalTime = utcNow.AddDays(1).AddHours(5),
                Status = FlightStatus.OnTime,
                DepartureAirportId = sofia.Id,
                ArrivalAirportId = london.Id,
                AircraftId = airbus.Id,
            };
            Flight secondFlight = new()
            {
                Id = Guid.NewGuid(),
                FlightNumber = "GA202",
                DepartureTime = utcNow.AddDays(2).AddHours(3),
                ArrivalTime = utcNow.AddDays(2).AddHours(5),
                Status = FlightStatus.OnTime,
                DepartureAirportId = london.Id,
                ArrivalAirportId = frankfurt.Id,
                AircraftId = boeing.Id,
            };
            Flight thirdFlight = new()
            {
                Id = Guid.NewGuid(),
                FlightNumber = "GA303",
                DepartureTime = utcNow.AddDays(3).AddHours(1),
                ArrivalTime = utcNow.AddDays(3).AddHours(3),
                Status = FlightStatus.Delayed,
                DepartureAirportId = frankfurt.Id,
                ArrivalAirportId = sofia.Id,
                AircraftId = airbus.Id,
            };
            dbContext.Airports.AddRange(sofia, london, frankfurt);
            dbContext.Aircraft.AddRange(airbus, boeing);
            dbContext.Flights.AddRange(firstFlight, secondFlight, thirdFlight);
            await dbContext.SaveChangesAsync();
        }
    }
}
