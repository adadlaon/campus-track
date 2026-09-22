using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace API.Extensions;

public static class AppUserExtensions
{
    public static async Task<UserDto> ToDto(
        this AppUser user,
        UserManager<AppUser> userManager,
        ITokenService tokenService)
    {
        var roles = await userManager.GetRolesAsync(user);

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email!,
            DisplayName = user.DisplayName,
            ImageUrl = user.ImageUrl,
            Roles = roles.ToArray(),
            Token = await tokenService.CreateToken(user)
        };
    }
}