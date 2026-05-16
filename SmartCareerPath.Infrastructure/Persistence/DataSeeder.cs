using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SmartCareerPath.Domain.Entites;
using SmartCareerPath.Domain.Entites.Identity;

namespace SmartCareerPath.Infrastructure.Persistence
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<AppUser>>();

            // Seed roles
            string[] roles = ["Seeker", "Mentor", "Admin"];
            foreach (var role in roles)
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));

            // Seed default Admin user
            var adminEmail = "admin@smartcareer.com";
            if (await userManager.FindByEmailAsync(adminEmail) is null)
            {
                var admin = new Admin
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "System",
                    LastName = "Admin",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(admin, "Admin@12345");
                await userManager.AddToRoleAsync(admin, "Admin");
            }

            // Seed LookupTypes
            var db = services.GetRequiredService<AppDbContext>();
            if (!db.LookupTypes.Any())
            {
                db.LookupTypes.AddRange(
                    new LookupType { Name = "JobTitle" },
                    new LookupType { Name = "Industry" }
                );
                await db.SaveChangesAsync();
            }
        }
    }
}
