using System.Text.Json;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class Seed
{
    public static async Task SeedUsers(AppDbContext context, UserManager<AppUser> userManager)
    {
        var memberData = await File.ReadAllTextAsync("Data/UserSeedData.json");
        var members = JsonSerializer.Deserialize<List<SeedUserDto>>(memberData);

        if (members == null)
        {
            Console.WriteLine("No members in seed data");
            return;
        }

        foreach (var member in members)
        {
            var user = await userManager.FindByIdAsync(member.Id);
            if (user == null)
            {
                user = new AppUser
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
                        HouseNumber = member.HouseNumber,
                        Zone = member.Zone,
                        Barangay = member.Barangay,
                        City = member.City,
                        Province = member.Province,
                        PhoneNumber = member.PhoneNumber,
                        LastActive = member.LastActive,
                        Created = member.Created
                    }
                };

                var result = await userManager.CreateAsync(user, "Pa$$w0rd");
                if (!result.Succeeded)
                {
                    Console.WriteLine(string.Join(", ", result.Errors.Select(error => error.Description)));
                    continue;
                }
            }

            var roles = member.Roles.Length > 0 ? member.Roles : ["Guardian"];
            await SyncIdentityRoles(userManager, user, roles);
            await SyncMemberRoles(context, user.Id, roles);
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

        await SyncIdentityRoles(userManager, admin, ["Administration", "Security"]);
    }

    private static async Task SyncIdentityRoles(
        UserManager<AppUser> userManager,
        AppUser user,
        IEnumerable<string> desiredRoles)
    {
        var roles = desiredRoles.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var currentRoles = await userManager.GetRolesAsync(user);
        var rolesToRemove = currentRoles.Except(roles, StringComparer.OrdinalIgnoreCase).ToArray();
        var rolesToAdd = roles.Except(currentRoles, StringComparer.OrdinalIgnoreCase).ToArray();

        if (rolesToRemove.Length > 0)
        {
            await userManager.RemoveFromRolesAsync(user, rolesToRemove);
        }

        if (rolesToAdd.Length > 0)
        {
            await userManager.AddToRolesAsync(user, rolesToAdd);
        }
    }

    private static async Task SyncMemberRoles(
        AppDbContext context,
        string memberId,
        IEnumerable<string> desiredRoleNames)
    {
        var roleNames = desiredRoleNames
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var applicationRoles = await context.ApplicationRoles
            .Where(role => roleNames.Contains(role.Name))
            .ToListAsync();

        var missingRoles = roleNames
            .Except(applicationRoles.Select(role => role.Name), StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (missingRoles.Length > 0)
        {
            throw new InvalidOperationException(
                $"Seeded member '{memberId}' references unknown application roles: {string.Join(", ", missingRoles)}");
        }

        var memberRoles = await context.MemberRoles
            .Where(memberRole => memberRole.MemberId == memberId)
            .ToListAsync();

        context.MemberRoles.RemoveRange(memberRoles);
        context.MemberRoles.AddRange(applicationRoles.Select(role => new MemberRole
        {
            MemberId = memberId,
            RoleId = role.Id
        }));

        await context.SaveChangesAsync();
    }

    public static async Task SeedRoles(DbContext context, RoleManager<IdentityRole> roleManager)
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
                Name = "Staff",
                Description = "Can manage student records."
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

        var identityRoles = new[] { "Parent", "Guardian", "Teacher", "Security", "Staff", "Administration" };
        foreach (var role in identityRoles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }
}