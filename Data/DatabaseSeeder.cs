using Microsoft.EntityFrameworkCore;
using ServiPro.API.Models.Entities;

namespace ServiPro.API.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db, IConfiguration config, ILogger logger)
    {
        var adminEmail = config["Seed:AdminEmail"];
        var adminPassword = config["Seed:AdminPassword"];
        var adminName = config["Seed:AdminName"] ?? "Administrador";

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            logger.LogWarning("Seed:AdminEmail or Seed:AdminPassword not configured — skipping admin seed.");
            return;
        }

        var exists = await db.Users.AnyAsync(u => u.Role == UserRole.Admin);
        if (exists) return;

        db.Users.Add(new User
        {
            Email = adminEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
            Name = adminName,
            Role = UserRole.Admin,
            IsActive = true,
        });

        await db.SaveChangesAsync();
        logger.LogInformation("Admin seed account created: {Email}", adminEmail);
    }
}
