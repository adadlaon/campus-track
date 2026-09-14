using System.Text.Json;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class Seed
{
    public static async Task SeedUsers(UserManager<AppUser> userManager)
    {
        if (await userManager.Users.AnyAsync()) return;

        var memberData = await File.ReadAllTextAsync("Data/UserSeedData.json");
        var members = JsonSerializer.Deserialize<List<SeedUserDto>>(memberData);

        if (members == null)
        {
            Console.WriteLine("No members in seed data");
            return;
        }

        foreach (var member in members)
        {
            var user = new AppUser
            {
                Id = member.Id,
                Email = member.Email,
                UserName = member.Email,
                DisplayName = member.DisplayName,
                ImageUrl = member.ImageUrl,
                Member = new Member
                {
                    Id = member.Id,
                    DisplayName = member.DisplayName,
                    ImageUrl = member.ImageUrl,
                    LastActive = member.LastActive,
                    Created = member.Created
                }
            };

            var result = await userManager.CreateAsync(user, "Pa$$w0rd");
            if (!result.Succeeded)
            {
                Console.WriteLine(result.Errors.First().Description);
            }
            await userManager.AddToRoleAsync(user, "Member");
        }

        var admin = await userManager.FindByEmailAsync("admin@test.com");
        
        if (admin == null)
        {
            admin = new AppUser
            {
                UserName = "admin@test.com",
                Email = "admin@test.com",
                DisplayName = "Admin"
            };

            var result = await userManager.CreateAsync(admin, "Pa$$w0rd");

            if (!result.Succeeded)
            {
                throw new Exception(string.Join(
                    ", ", result.Errors.Select(e => e.Description)));
            }
        }

        await userManager.AddToRolesAsync(admin, ["Admin", "Moderator"]);
    }

    public static async Task SeedRoles(DbContext context)
    {
        var roles = new[]
        {
            new ApplicationRole
            {
                Name = "Parent",
                Description = "Can view attendance and notifications for linked students."
            },
            new ApplicationRole
            {
                Name = "Guardian",
                Description = "Can view attendance and notifications for linked students."
            },
            new ApplicationRole
            {
                Name = "Teacher",
                Description = "Can manage attendance records for assigned students."
            },
            new ApplicationRole
            {
                Name = "Security",
                Description = "Can monitor campus entry and exit logs."
            },
            new ApplicationRole
            {
                Name = "Administration",
                Description = "Can manage users, reports, and system settings."
            }
        };

        foreach (var role in roles)
        {
            var exists = await context.Set<ApplicationRole>()
                .AnyAsync(x => x.Name == role.Name);

            if (!exists)
            {
                context.Set<ApplicationRole>().Add(role);
            }
        }

        await context.SaveChangesAsync();
    }
}