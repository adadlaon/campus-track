using API.DTOs;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[Authorize(Policy = "CanManageUsers")]
public class UserManagementController(
    AppDbContext context,
    UserManager<AppUser> userManager,
    RoleManager<IdentityRole> roleManager) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserManagementResponseDto>>> GetUsers()
    {
        var users = await userManager.Users
            .Include(user => user.Member)
            .AsNoTracking()
            .OrderBy(user => user.DisplayName)
            .ToListAsync();

        var responses = new List<UserManagementResponseDto>(users.Count);
        foreach (var user in users)
        {
            responses.Add(await ToResponse(user));
        }

        return Ok(responses);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserManagementResponseDto>> GetUser(string id)
    {
        var user = await userManager.Users
            .Include(candidate => candidate.Member)
            .SingleOrDefaultAsync(candidate => candidate.Id == id);

        return user == null ? NotFound() : Ok(await ToResponse(user));
    }

    [HttpPost]
    public async Task<ActionResult<UserManagementResponseDto>> CreateUser(CreateUserRequestDto request)
    {
        if (await userManager.FindByEmailAsync(request.Email) != null)
        {
            return Conflict(new { message = "A user with this email already exists." });
        }

        var roles = request.Roles
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var rolesError = await ValidateRoles(roles);
        if (rolesError != null) return BadRequest(rolesError);

        var user = new AppUser
        {
            Email = request.Email,
            UserName = request.Email,
            DisplayName = request.DisplayName,
            ImageUrl = request.ImageUrl,
            Member = new Member
            {
                DisplayName = request.DisplayName,
                ImageUrl = request.ImageUrl,
                HouseNumber = request.HouseNumber,
                Zone = request.Zone,
                Barangay = request.Barangay,
                City = request.City,
                Province = request.Province,
                PhoneNumber = request.PhoneNumber
            }
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded) return BadRequest(IdentityValidationProblem(result));

        result = await userManager.AddToRolesAsync(user, roles);
        if (!result.Succeeded)
        {
            await userManager.DeleteAsync(user);
            return BadRequest(IdentityValidationProblem(result));
        }

        var applicationRoles = await context.ApplicationRoles
            .Where(role => roles.Contains(role.Name))
            .ToListAsync();

        context.MemberRoles.AddRange(applicationRoles.Select(role => new MemberRole
        {
            MemberId = user.Id,
            RoleId = role.Id
        }));
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, await ToResponse(user));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<UserManagementResponseDto>> UpdateUser(string id, UpdateUserRequestDto request)
    {
        var roles = request.Roles
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var rolesError = await ValidateRoles(roles);
        if (rolesError != null) return BadRequest(rolesError);

        var user = await userManager.Users
            .Include(candidate => candidate.Member)
            .SingleOrDefaultAsync(candidate => candidate.Id == id);

        if (user == null) return NotFound();

        var applicationRoles = await context.ApplicationRoles
            .Where(role => roles.Contains(role.Name))
            .ToListAsync();

        if (applicationRoles.Count != roles.Length)
        {
            return BadRequest(new { message = "One or more application roles do not exist." });
        }

        user.Email = request.Email;
        user.UserName = request.Email;
        user.DisplayName = request.DisplayName;
        user.ImageUrl = request.ImageUrl;
        user.Member ??= new Member { Id = user.Id, DisplayName = request.DisplayName, Barangay = request.Barangay, City = request.City, Province = request.Province, PhoneNumber = request.PhoneNumber };
        user.Member.DisplayName = request.DisplayName;
        user.Member.ImageUrl = request.ImageUrl;
        user.Member.HouseNumber = request.HouseNumber;
        user.Member.Zone = request.Zone;
        user.Member.Barangay = request.Barangay;
        user.Member.City = request.City;
        user.Member.Province = request.Province;
        user.Member.PhoneNumber = request.PhoneNumber;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded) return BadRequest(IdentityValidationProblem(result));

        var currentRoles = await userManager.GetRolesAsync(user);
        result = await userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!result.Succeeded) return BadRequest(IdentityValidationProblem(result));

        result = await userManager.AddToRolesAsync(user, roles);
        if (!result.Succeeded) return BadRequest(IdentityValidationProblem(result));

        var memberRoles = await context.MemberRoles
            .Where(memberRole => memberRole.MemberId == user.Id)
            .ToListAsync();

        context.MemberRoles.RemoveRange(memberRoles);
        context.MemberRoles.AddRange(applicationRoles.Select(role => new MemberRole
        {
            MemberId = user.Id,
            RoleId = role.Id
        }));
        await context.SaveChangesAsync();

        return Ok(await ToResponse(user));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var result = await userManager.DeleteAsync(user);
        return result.Succeeded ? NoContent() : BadRequest(IdentityValidationProblem(result));
    }

    private async Task<UserManagementResponseDto> ToResponse(AppUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        return new UserManagementResponseDto
        {
            Id = user.Id,
            Email = user.Email!,
            DisplayName = user.DisplayName,
            ImageUrl = user.ImageUrl,
            Roles = roles.ToArray(),
            HouseNumber = user.Member?.HouseNumber,
            Zone = user.Member?.Zone,
            Barangay = user.Member?.Barangay,
            City = user.Member?.City,
            Province = user.Member?.Province,
            PhoneNumber = user.Member?.PhoneNumber
        };
    }

    private async Task<ValidationProblemDetails?> ValidateRoles(IEnumerable<string> roles)
    {
        var requestedRoles = roles.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var invalidRoles = new List<string>();

        foreach (var role in requestedRoles)
        {
            if (!await roleManager.RoleExistsAsync(role)) invalidRoles.Add(role);
        }

        return invalidRoles.Count == 0
            ? null
            : new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["roles"] = [$"Unknown roles: {string.Join(", ", invalidRoles)}"]
            });
    }

    private static ValidationProblemDetails IdentityValidationProblem(IdentityResult result)
    {
        return new ValidationProblemDetails(result.Errors
            .GroupBy(error => error.Code)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.Description).ToArray()));
    }
}