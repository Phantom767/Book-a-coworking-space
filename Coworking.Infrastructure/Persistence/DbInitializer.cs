using Coworking.Domain.Entity;
using Microsoft.AspNetCore.Identity;

namespace Coworking.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAdminAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        // 1. Создаем роли если их нет
        if (!await roleManager.RoleExistsAsync("Admin"))
            await roleManager.CreateAsync(new ApplicationRole { Name = "Admin" });

        if (!await roleManager.RoleExistsAsync("User"))
            await roleManager.CreateAsync(new ApplicationRole { Name = "User" });

        // 2. Проверяем есть ли админ
        var adminEmail = "admin@admin.com";

        var admin = await userManager.FindByEmailAsync(adminEmail);

        if (admin == null)
        {
            var newAdmin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "Admin"
            };
            
            var result = await userManager.CreateAsync(newAdmin, "Admin123!");
            
            if (result.Succeeded)
                await userManager.AddToRoleAsync(newAdmin, "Admin");
            else
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
}