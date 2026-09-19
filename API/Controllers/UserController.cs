using API.DTOs;
using API.Data;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class UserController(
    UserManager<AppUser> userManager,
    ITokenService tokenService,
    AppDbContext context) : BaseApiController
{
    [HttpPost("login")] // api/user/login
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var user = await userManager.FindByEmailAsync(loginDto.Email);

        if (user == null) return Unauthorized("Invalid email address");

        var result = await userManager.CheckPasswordAsync(user, loginDto.Password);

        if (!result) return Unauthorized("Invalid password");

        await SetRefreshTokenCookie(user);

        return await user.ToDto(tokenService);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult> Logout()
    {
        await userManager.Users
            .Where(x => x.Id == User.GetMemberId())
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.RefreshToken, _ => null)
                .SetProperty(x => x.RefreshTokenExpiry, _ => null));

        Response.Cookies.Delete("refreshToken");

        return Ok();
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<UserDto>> RefreshToken()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (refreshToken == null) return NoContent();

        var user = await userManager.Users
            .FirstOrDefaultAsync(x => x.RefreshToken == refreshToken
                && x.RefreshTokenExpiry > DateTime.UtcNow);
        
        if (user == null) return Unauthorized();

        await SetRefreshTokenCookie(user);

        return await user.ToDto(tokenService);
    }
    
    [HttpPost("register")] // api/user/register
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        var roleExists = await context.ApplicationRoles
            .AnyAsync(role => role.Id == registerDto.RoleId);

        if (!roleExists)
        {
            ModelState.AddModelError(nameof(registerDto.RoleId), "The selected role is invalid.");
            return ValidationProblem();
        }

        var user = new AppUser
        {
            Email = registerDto.Email,
            DisplayName = registerDto.DisplayName,
            UserName = registerDto.Email,
            Member = new Member
            {
                DisplayName = registerDto.DisplayName,
                HouseNumber = registerDto.HouseNumber,
                Zone = registerDto.Zone,
                Barangay =  registerDto.Barangay,
                City = registerDto.City,
                Province = registerDto.Province,
                PhoneNumber = registerDto.PhoneNumber
            }
        };

        var result = await userManager.CreateAsync(user, registerDto.Password);
        
        if (!result.Succeeded)
        {
            var duplicateEmailReported = false;

            foreach (var error in result.Errors)
            {
                if (error.Code is "DuplicateUserName" or "DuplicateEmail")
                {
                    if (!duplicateEmailReported)
                    {
                        ModelState.AddModelError("identity", "An account with this email already exists.");
                        duplicateEmailReported = true;
                    }
                }
                else
                {
                    ModelState.AddModelError("identity", error.Description);
                }
            }    

            return ValidationProblem();
        }
        
        await userManager.AddToRoleAsync(user, "Member");

        context.MemberRoles.Add(new MemberRole
        {
            MemberId = user.Id,
            RoleId = registerDto.RoleId
        });
        await context.SaveChangesAsync();

        await SetRefreshTokenCookie(user);

        return await user.ToDto(tokenService);
    }

    private async Task SetRefreshTokenCookie(AppUser user)
    {
        var refreshToken = tokenService.GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await userManager.UpdateAsync(user);

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            // Cookies used for cross-site requests must use SameSite=None and Secure
            // Modern browsers require Secure when SameSite=None, so ensure the app is served over HTTPS in development
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddDays(7)
        };

        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }
}